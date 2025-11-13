using faturamento_api.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using faturamento_api.Entities;
using faturamento_api.Dtos;
using Polly;
using Polly.CircuitBreaker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FaturamentoDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("http://localhost:5002");

builder.Services.AddHttpClient("estoque", c => c.BaseAddress = new Uri(builder.Configuration["Services:Estoque"]!))
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(200*i)))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/faturamento/notas", async (FaturamentoDbContext db, string? q) =>
{
    var baseQ = db.Nota
        .Include(n => n.Itens)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(q))
    {
        q = q.Trim().ToLower();

        baseQ = baseQ.Where(n =>
            n.Status.ToLower().Contains(q) || n.Numero.ToString().Contains(q)
        );
    }

    var notas = await baseQ
        .OrderBy(n => n.Status)
        .ThenBy(n => n.Numero)
        .Select(n => new
        {
            n.Id,
            n.Numero,
            n.Status,
            Itens = n.Itens.Select(i => new
            {
                i.Id,
                i.ProdutoId,
                i.Quantidade,
                i.Preco
            })
        })
        .ToListAsync();

    return Results.Ok(new { notas });
});

app.MapGet("/api/faturamento/notas/{id:int}", async (FaturamentoDbContext db, int id) =>
{
    var nota = await db.Nota
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nota == null)
        return Results.NotFound();

    return Results.Ok(nota);
});

app.MapPost("/api/faturamento/notas", async (FaturamentoDbContext db, NotaCreateDto dto, IHttpClientFactory http) =>
{
    if (dto == null || dto.Itens == null || dto.Itens.Count == 0)
        return Results.BadRequest(new { message = "A nota deve possuir ao menos um item." });

    for (int i = 0; i < dto.Itens.Count; i++)
    {
        var it = dto.Itens[i];
        if (it.Quantidade <= 0)
            return Results.BadRequest(new { message = $"Item #{i + 1}: quantidade deve ser maior que 0." });
        if (it.Preco < 0)
            return Results.BadRequest(new { message = $"Item #{i + 1}: preço não pode ser negativo." });
    }

    var client = http.CreateClient("estoque");

    foreach (var it in dto.Itens)
    {
        HttpResponseMessage resp;

        try
        {
            resp = await client.GetAsync(
                $"/api/estoque/produtos/{it.ProdutoId}/disponibilidade?quantidade={it.Quantidade}");
        }
        catch (BrokenCircuitException)
        {
            return Results.Problem(
                title: "Serviço de estoque indisponível",
                detail: "O serviço de estoque está temporariamente indisponível. Tente novamente em alguns instantes.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (HttpRequestException)
        {
            return Results.Problem(
                title: "Serviço de estoque indisponível",
                detail: "Não foi possível se comunicar com o serviço de estoque. Tente novamente em alguns instantes.",
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (TaskCanceledException)
        {
            return Results.Problem(
                title: "Tempo de resposta excedido",
                detail: "O serviço de estoque demorou para responder. Tente novamente mais tarde.",
                statusCode: StatusCodes.Status504GatewayTimeout);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { message = $"Produto {it.ProdutoId} não encontrado no estoque." });
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var body = await resp.Content.ReadAsStringAsync();
            using var json = System.Text.Json.JsonDocument.Parse(body);
            var root = json.RootElement;

            int saldo = root.TryGetProperty("saldo", out var sd) ? sd.GetInt32() : -1;
            int requerido = root.TryGetProperty("requerido", out var rq) ? rq.GetInt32() : it.Quantidade;

            return Results.Conflict(new
            {
                message = $"Saldo insuficiente para o produto {it.ProdutoId}.",
                saldo,
                requerido
            });
        }

        if (!resp.IsSuccessStatusCode)
        {
            return Results.Problem(
                title: "Erro ao consultar serviço de estoque",
                detail: $"Falha ao consultar disponibilidade do produto {it.ProdutoId} (HTTP {(int)resp.StatusCode}).",
                statusCode: StatusCodes.Status502BadGateway);
        }

        using var stream = await resp.Content.ReadAsStreamAsync();
        using var jsonOk = await System.Text.Json.JsonDocument.ParseAsync(stream);
        var rootOk = jsonOk.RootElement;

        bool suficiente = rootOk.TryGetProperty("suficiente", out var s) && s.GetBoolean();
        int saldoOk = rootOk.TryGetProperty("saldo", out var sdOk) ? sdOk.GetInt32() : -1;
        int requeridoOk = rootOk.TryGetProperty("requerido", out var rqOk) ? rqOk.GetInt32() : it.Quantidade;

        if (!suficiente)
        {
            return Results.Conflict(new
            {
                message = $"Saldo insuficiente para o produto {it.ProdutoId}.",
                produtoId = it.ProdutoId,
                saldo = saldoOk,
                requerido = requeridoOk
            });
        }
    }

    var nota = new Nota
    {
        Status = "Aberta",
        Itens = dto.Itens.Select(i => new ItemNota
        {
            ProdutoId = i.ProdutoId,
            Quantidade = i.Quantidade,
            Preco = i.Preco
        }).ToList()
    };

    db.Nota.Add(nota);
    await db.SaveChangesAsync();

    return Results.Created($"/api/faturamento/notas/{nota.Id}", new { nota.Id, nota.Status });
});

app.MapPut("/api/faturamento/notas/{id:int}", async (FaturamentoDbContext db, int id, NotaUpdateDto dto, IHttpClientFactory http) =>
{
    var nota = await db.Nota
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nota == null)
        return Results.NotFound(new { message = "Nota não encontrada." });

    if (!string.Equals(nota.Status, "Aberta", StringComparison.OrdinalIgnoreCase))
        return Results.Conflict(new { message = "A nota não está aberta e não pode ser editada." });

    if (dto == null || dto.Itens == null || dto.Itens.Count == 0)
        return Results.BadRequest(new { message = "A nota deve possuir ao menos um item." });

    for (int i = 0; i < dto.Itens.Count; i++)
    {
        var it = dto.Itens[i];
        if (it.Quantidade <= 0)
            return Results.BadRequest(new { message = $"Item #{i + 1}: quantidade deve ser maior que 0." });
        if (it.Preco < 0)
            return Results.BadRequest(new { message = $"Item #{i + 1}: preço não pode ser negativo." });
    }

    var client = http.CreateClient("estoque");

    foreach (var it in dto.Itens)
    {
        HttpResponseMessage resp;

        try
        {
            resp = await client.GetAsync($"/api/estoque/produtos/{it.ProdutoId}/disponibilidade?quantidade={it.Quantidade}");
        }
        catch (BrokenCircuitException)
        {
            return Results.Problem(
                title: "Serviço de estoque indisponível",
                detail: "O serviço de estoque está temporariamente indisponível. Tente novamente em alguns instantes.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (HttpRequestException)
        {
            return Results.Problem(
                title: "Serviço de estoque indisponível",
                detail: "Não foi possível se comunicar com o serviço de estoque. Tente novamente em alguns instantes.",
                statusCode: StatusCodes.Status502BadGateway);
        }
        catch (TaskCanceledException)
        {
            return Results.Problem(
                title: "Tempo de resposta excedido",
                detail: "O serviço de estoque demorou para responder. Tente novamente mais tarde.",
                statusCode: StatusCodes.Status504GatewayTimeout);
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Results.NotFound(new
            {
                message = $"Produto {it.ProdutoId} não encontrado no estoque."
            });
        }

        if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            var body = await resp.Content.ReadAsStringAsync();
            using var json = System.Text.Json.JsonDocument.Parse(body);
            var root = json.RootElement;

            int saldo = root.TryGetProperty("saldo", out var sd) ? sd.GetInt32() : -1;
            int requerido = root.TryGetProperty("requerido", out var rq) ? rq.GetInt32() : it.Quantidade;

            return Results.Conflict(new
            {
                message = $"Saldo insuficiente para o produto {it.ProdutoId}.",
                saldo,
                requerido
            });
        }

        if (!resp.IsSuccessStatusCode)
        {
            return Results.Problem(
                title: "Erro ao consultar serviço de estoque",
                detail: $"Falha ao consultar disponibilidade do produto {it.ProdutoId} (HTTP {(int)resp.StatusCode}).",
                statusCode: StatusCodes.Status502BadGateway);
        }

        using var stream = await resp.Content.ReadAsStreamAsync();
        using var jsonOk = await System.Text.Json.JsonDocument.ParseAsync(stream);
        var rootOk = jsonOk.RootElement;

        bool suficiente = rootOk.TryGetProperty("suficiente", out var s) && s.GetBoolean();
        int saldoOk = rootOk.TryGetProperty("saldo", out var sdOk) ? sdOk.GetInt32() : -1;
        int requeridoOk = rootOk.TryGetProperty("requerido", out var rqOk) ? rqOk.GetInt32() : it.Quantidade;

        if (!suficiente)
        {
            return Results.Conflict(new
            {
                message = $"Saldo insuficiente para o produto {it.ProdutoId}.",
                produtoId = it.ProdutoId,
                saldo = saldoOk,
                requerido = requeridoOk,
                status = 409,
                origem = "estoque"
            });
        }
    }

    db.ItemNota.RemoveRange(nota.Itens);
    nota.Itens = dto.Itens.Select(i => new ItemNota
    {
        ProdutoId = i.ProdutoId,
        Quantidade = i.Quantidade,
        Preco = i.Preco,
        NotaId = nota.Id
    }).ToList();

    await db.SaveChangesAsync();

    return Results.Ok(new { nota.Id, nota.Status });
});

app.MapDelete("/api/faturamento/notas/{id:int}", async (FaturamentoDbContext db, int id) =>
{
    var nota = await db.Nota.FindAsync(id);
    if (nota == null)
        return Results.NotFound(new { message = "Nota não encontrada." });

    if (!string.Equals(nota.Status, "Aberta", StringComparison.OrdinalIgnoreCase))
        return Results.Conflict(new { message = "A nota já foi fechada e não pode ser removida." });

    db.Nota.Remove(nota);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapPost("/api/faturamento/notas/{id:int}/impressao", async (FaturamentoDbContext db, IHttpClientFactory http, int id) =>
{
    var nota = await db.Nota
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nota == null)
        return Results.NotFound(new { message = "Nota não encontrada." });

    if (!string.Equals(nota.Status, "Aberta", StringComparison.OrdinalIgnoreCase))
        return Results.Conflict(new { message = "A nota já foi fechada e não pode ser impressa." });

    if (nota.Itens == null || nota.Itens.Count == 0)
        return Results.BadRequest(new { message = "A nota não possui itens para impressão." });

    var client = http.CreateClient("estoque");

    var payload = nota.Itens
        .Select(i => new { ProdutoId = i.ProdutoId, Qtd = i.Quantidade })
        .ToList();

    HttpResponseMessage resp;
    try
    {
        resp = await client.PostAsJsonAsync("/api/estoque/produtos/baixas", payload);
    }
    catch (BrokenCircuitException)
    {
        return Results.Problem(
            title: "Serviço de estoque indisponível",
            detail: "O serviço de estoque está temporariamente indisponível. Tente novamente em alguns instantes.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            title: "Serviço de estoque indisponível",
            detail: "Não foi possível se comunicar com o serviço de estoque. Tente novamente em alguns instantes.",
            statusCode: StatusCodes.Status502BadGateway);
    }
    catch (TaskCanceledException)
    {
        return Results.Problem(
            title: "Tempo de resposta excedido",
            detail: "O serviço de estoque demorou para responder. Tente novamente mais tarde.",
            statusCode: StatusCodes.Status504GatewayTimeout);
    }

    if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        var body = await resp.Content.ReadAsStringAsync();
        return Results.NotFound(string.IsNullOrWhiteSpace(body)
            ? new { message = "Produto não encontrado no estoque." }
            : System.Text.Json.JsonSerializer.Deserialize<object>(body));
    }

    if (resp.StatusCode == System.Net.HttpStatusCode.Conflict)
    {
        var body = await resp.Content.ReadAsStringAsync();
        return Results.Conflict(string.IsNullOrWhiteSpace(body)
            ? new { message = "Saldo insuficiente para um ou mais produtos da nota." }
            : System.Text.Json.JsonSerializer.Deserialize<object>(body));
    }

    if (resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
    {
        var body = await resp.Content.ReadAsStringAsync();
        return Results.BadRequest(string.IsNullOrWhiteSpace(body)
            ? new { message = "Requisição inválida ao serviço de estoque." }
            : System.Text.Json.JsonSerializer.Deserialize<object>(body));
    }

    if (!resp.IsSuccessStatusCode)
    {
        var body = await resp.Content.ReadAsStringAsync();
        var conteudo = string.IsNullOrWhiteSpace(body)
            ? "Falha ao baixar itens no estoque."
            : body;

        return Results.Problem(
            title: "Erro ao processar baixa de estoque",
            detail: conteudo,
            statusCode: (int)resp.StatusCode
        );
    }

    nota.Status = "Fechada";
    await db.SaveChangesAsync();

    return Results.Ok(new { nota.Id, nota.Status });
});

app.Run();
