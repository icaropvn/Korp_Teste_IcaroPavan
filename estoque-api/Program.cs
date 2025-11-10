
using estoque_api.Data;
using Microsoft.EntityFrameworkCore;
using estoque_api.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EstoqueDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/produtos", async (EstoqueDbContext db, string? q, int page = 1, int size = 20) =>
{
    var baseQ = db.Produto.AsQueryable();
    if (!string.IsNullOrWhiteSpace(q))
        baseQ = baseQ.Where(p => p.Codigo.Contains(q) || p.Descricao.Contains(q));

    var items = await baseQ
        .OrderBy(p => p.Descricao)
        .Skip((page-1)*size).Take(size)
        .Select(p => new { p.Id, p.Codigo, p.Descricao, p.Saldo })
        .ToListAsync();

    var total = await baseQ.CountAsync();
    return Results.Ok(new { items, total });
});

app.MapPost("/api/produtos", async (EstoqueDbContext db, Produto dto) =>
{
    db.Produto.Add(dto);
    await db.SaveChangesAsync();
    return Results.Created($"/api/produtos/{dto.Id}", dto);
});

app.MapPost("/api/estoque/baixas", async (EstoqueDbContext db, List<(int produtoId,int qtd)> itens) =>
{
    using var tx = await db.Database.BeginTransactionAsync();
    foreach (var (produtoId, qtd) in itens)
    {
        var afetadas = await db.Database.ExecuteSqlRawAsync(
            "UPDATE \"Produtos\" SET \"Saldo\" = \"Saldo\" - {0} WHERE \"Id\" = {1} AND \"Saldo\" >= {0}",
            qtd, produtoId);

        if(afetadas == 0)
            return Results.Conflict(new { message = $"Saldo insuficiente para produto {produtoId}" });
    }
    await tx.CommitAsync();
    return Results.Ok(new { ok = true });
});

app.Run();