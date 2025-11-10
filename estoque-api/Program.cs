
using estoque_api.Data;
using Microsoft.EntityFrameworkCore;
using estoque_api.Entities;
using estoque_api.Dtos;

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

app.MapPost("/api/produtos", async (EstoqueDbContext db, ProdutoCreateDto dto) =>
{
    var produto = new Produto
    {
        Descricao = dto.Descricao,
        Saldo = dto.Saldo,
        Codigo = Guid.NewGuid().ToString("N")[..8]
    };

    db.Produto.Add(produto);
    await db.SaveChangesAsync();
    return Results.Created($"/api/produtos/{produto.Id}", dto);
});

app.MapPost("/api/estoque/baixas", async (EstoqueDbContext db, List<BaixaDto> itens) =>
{
    if (itens is null || itens.Count == 0)
        return Results.BadRequest(new { message = "Nenhum item enviado." });

    var invalidos = itens.Where(i => i.Qtd <= 0).ToList();
    if (invalidos.Count > 0)
        return Results.BadRequest(new {
            message = "Existem itens com quantidade inválida (<= 0)."
        });

    using var tx = await db.Database.BeginTransactionAsync();

    var results = new List<object>();
    foreach (var (produtoId, qtd) in itens.Select(i => (i.ProdutoId, i.Qtd)))
    {
        var afetadas = await db.Database.ExecuteSqlRawAsync(
            "UPDATE \"Produto\" SET \"Saldo\" = \"Saldo\" - {0} WHERE \"Id\" = {1} AND \"Saldo\" >= {0}",
            qtd, produtoId);

        if (afetadas == 1)
        {
            results.Add(new { produtoId, qtd, status = "ok" });
            continue;
        }

        var existe = await db.Produto.AnyAsync(p => p.Id == produtoId);
        if (!existe)
        {
            await tx.RollbackAsync();
            return Results.NotFound(new {
                message = "Produto não encontrado."
            });
        }

        var saldoAtual = await db.Produto
            .Where(p => p.Id == produtoId)
            .Select(p => p.Saldo)
            .FirstAsync();

        if (saldoAtual < qtd)
        {
            await tx.RollbackAsync();
            return Results.Conflict(new {
                message = "Saldo insuficiente."
            });
        }

        await tx.RollbackAsync();
        return Results.Conflict(new {
            message = "Conflito de concorrência ao atualizar o produto."
        });
    }

    await tx.CommitAsync();
    return Results.Ok(new {
        ok = true,
        updatedCount = results.Count,
        results
    });
});


app.Run();