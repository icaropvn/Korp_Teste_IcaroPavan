
using faturamento_api.Data;
using Microsoft.EntityFrameworkCore;
using faturamento_api.Entities;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FaturamentoDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("estoque", c => c.BaseAddress = new Uri(builder.Configuration["Services:Estoque"]!))
    .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(200*i)))
    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5)));

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/notas", async (FaturamentoDbContext db) =>
{
    var nota = new Nota { Status = "Aberta" };
    db.Nota.Add(nota);
    await db.SaveChangesAsync();
    return Results.Created($"/api/notas/{nota.Id}", new { nota.Id, nota.Numero, nota.Status });
});

app.MapGet("/api/notas/{id:int}", async (FaturamentoDbContext db, int id) =>
{
    var n = await db.Nota.Include(x => x.Itens).FirstOrDefaultAsync(x => x.Id == id);
    return n is null ? Results.NotFound() : Results.Ok(n);
});

app.MapPut("/api/notas/{id:int}", async (FaturamentoDbContext db, int id, Nota entrada) =>
{
    var n = await db.Nota.Include(x => x.Itens).FirstOrDefaultAsync(x => x.Id == id);
    if (n is null) return Results.NotFound();
    if (n.Status != "Aberta") return Results.Conflict(new { message = "Nota fechada é imutável" });

    await db.SaveChangesAsync();
    return Results.Ok(n);
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
    var resp = await client.PostAsJsonAsync("/api/estoque/baixas", itens);
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
            Chave = idemKey, Rota = $"/api/notas/{id}/impressao",
            StatusHttp = StatusCodes.Status200OK, RespostaJson = "{}"
        });
        await db.SaveChangesAsync();
    }
    return Results.Ok(new { n.Id, n.Numero, n.Status });
});

app.Run();
