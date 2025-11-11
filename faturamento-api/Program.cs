
using faturamento_api.Data;
using Microsoft.EntityFrameworkCore;
using faturamento_api.Entities;
using faturamento_api.Dtos;
using Polly;

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

app.MapPost("/api/notas", async (FaturamentoDbContext db, NotaCreateDto dto) => {
    var n = new Nota { Status = "Aberta", Itens = dto.Itens.Select(i => new NotaItem { ProdutoId=i.ProdutoId, Quantidade=i.Quantidade, Preco=i.Preco }).ToList() };
    db.Nota.Add(n); await db.SaveChangesAsync();
    
    return Results.Created($"/api/notas/{n.Id}",
        new NotaReadDto(n.Id, n.Numero, n.Status, n.Itens.Select(x => new NotaItemReadDto(x.Id,x.ProdutoId,x.Quantidade,x.Preco)).ToList()));
});

app.MapGet("/api/notas", async (FaturamentoDbContext db, int page = 1, int size = 20, string? status = null) => {
    if (page < 1) page = 1;
    if (size < 1) size = 20;

    var query = db.Nota
        .AsNoTracking()
        .Include(n => n.Itens)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(status))
        query = query.Where(n => n.Status == status);

    var total = await query.CountAsync();

    var items = await query
        .OrderByDescending(n => n.Id)
        .Skip((page - 1) * size)
        .Take(size)
        .Select(n => new NotaReadDto(
            n.Id,
            n.Numero,
            n.Status,
            n.Itens.Select(i => new NotaItemReadDto(i.Id, i.ProdutoId, i.Quantidade, i.Preco)).ToList()
        ))
        .ToListAsync();

    return Results.Ok(new { items, total, page, size });
});

app.MapGet("/api/notas/{id:int}", async (FaturamentoDbContext db, int id) => {
    var n = await db.Nota.Include(x => x.Itens).FirstOrDefaultAsync(x => x.Id == id);
    
    return n is null ? Results.NotFound() :
        Results.Ok(new NotaReadDto(n.Id, n.Numero, n.Status, n.Itens.Select(x => new NotaItemReadDto(x.Id,x.ProdutoId,x.Quantidade,x.Preco)).ToList()));
});

app.MapPut("/api/notas/{id:int}", async (FaturamentoDbContext db, int id, NotaUpdateDto dto) =>
{
    if (dto?.Itens is null || dto.Itens.Count == 0)
        return Results.BadRequest(new { message = "A nota deve ter ao menos um item." });

    var invalidos = dto.Itens.Where(i => i.Quantidade <= 0 || i.Preco < 0).ToList();
    if (invalidos.Count > 0)
        return Results.BadRequest(new {
            message = "Existem itens inválidos.",
            errors = invalidos.Select(i => new {
                i.Id, i.ProdutoId, i.Quantidade, i.Preco,
                code = i.Quantidade <= 0 ? "invalid_quantity" : "invalid_price"
            })
        });

    var nota = await db.Nota
        .Include(n => n.Itens)
        .FirstOrDefaultAsync(n => n.Id == id);

    if (nota is null)
        return Results.NotFound(new { message = "Nota não encontrada." });

    if (!string.Equals(nota.Status, "Aberta", StringComparison.OrdinalIgnoreCase))
        return Results.Conflict(new { message = "A nota não pode ser alterada (status != Aberta)." });

    using var tx = await db.Database.BeginTransactionAsync();

    var idsEnviadosExistentes = dto.Itens.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();
    var atuais = nota.Itens.ToList();
    foreach (var existente in atuais)
    {
        if (!idsEnviadosExistentes.Contains(existente.Id))
        {
            db.Remove(existente);
        }
    }

    var atuaisPorId = nota.Itens.ToDictionary(i => i.Id, i => i);

    foreach (var itemDto in dto.Itens)
    {
        if (itemDto.Id is int itemId && atuaisPorId.TryGetValue(itemId, out var existente))
        {
            existente.ProdutoId = itemDto.ProdutoId;
            existente.Quantidade = itemDto.Quantidade;
            existente.Preco = itemDto.Preco;
        }
        else
        {
            nota.Itens.Add(new NotaItem
            {
                NotaId = nota.Id,
                ProdutoId = itemDto.ProdutoId,
                Quantidade = itemDto.Quantidade,
                Preco = itemDto.Preco
            });
        }
    }

    try
    {
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
    catch (DbUpdateConcurrencyException)
    {
        await tx.RollbackAsync();
        return Results.Conflict(new { message = "Conflito de concorrência ao atualizar a nota." });
    }

    var read = new NotaReadDto(
        nota.Id,
        nota.Numero,
        nota.Status,
        nota.Itens.Select(i => new NotaItemReadDto(i.Id, i.ProdutoId, i.Quantidade, i.Preco)).ToList()
    );
    return Results.Ok(read);
});

app.MapPost("/api/notas/{id:int}/impressao", async (
    FaturamentoDbContext db, IHttpClientFactory http, HttpRequest req, int id) =>
{
    var n = await db.Nota.Include(x => x.Itens).FirstOrDefaultAsync(x => x.Id == id);
    if (n is null) return Results.NotFound();
    if (n.Status != "Aberta") return Results.Conflict(new { message = "Nota já fechada" });

    var idemKey = req.Headers.TryGetValue("Idempotency-Key", out var k) ? k.ToString() : null;

    if (idemKey is not null)
    {
        var existe = await db.ChaveIdempotencia.FirstOrDefaultAsync(x => x.Chave == idemKey && x.Rota == $"/api/notas/{id}/impressao");
        if (existe is not null) return Results.StatusCode(existe.StatusHttp);
    }

    var itens = n.Itens.Select(i => new { produtoId = i.ProdutoId, qtd = i.Quantidade });
    var client = http.CreateClient("estoque");
    var resp = await client.PostAsJsonAsync("/api/baixas", itens);
    if (!resp.IsSuccessStatusCode)
        return Results.StatusCode((int)resp.StatusCode);

    n.Status = "Fechada";
    if (n.Numero is null)
    {
        var maxNumero = await db.Nota.MaxAsync(x => (long?)x.Numero) ?? 0;
        n.Numero = maxNumero + 1;
    }
    await db.SaveChangesAsync();

    if (idemKey is not null)
    {
        db.ChaveIdempotencia.Add(new IdempotencyKey
        {
            Chave = idemKey,
            Rota = $"/api/notas/{id}/impressao",
            StatusHttp = StatusCodes.Status200OK,
            RespostaJson = "{}"
        });
        await db.SaveChangesAsync();
    }
    
    return Results.Ok(new NotaFechamentoResponse(n.Id, n.Numero!.Value, n.Status));
});

app.Run();
