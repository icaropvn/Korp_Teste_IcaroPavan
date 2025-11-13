
using estoque_api.Data;
using Microsoft.EntityFrameworkCore;
using estoque_api.Entities;
using estoque_api.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<EstoqueDbContext>(opts =>
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.WebHost.UseUrls("http://localhost:5001");

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/estoque/produtos", async (EstoqueDbContext db, string? q) =>
{
    var baseQ = db.Produto.AsQueryable();

    if(!string.IsNullOrWhiteSpace(q))
    {
        q = q.ToLower().Trim();
        baseQ = baseQ.Where(p =>
            p.Codigo.ToLower().Contains(q) ||
            p.Descricao.ToLower().Contains(q));
    }

    var produtos = await baseQ
        .OrderBy(p => p.Descricao)
        .Select(p => new { p.Id, p.Codigo, p.Descricao, p.Saldo })
        .ToListAsync();

    return Results.Ok(new { produtos });
});

app.MapGet("/api/estoque/produtos/{id:int}", async (EstoqueDbContext db, int id) =>
{
    var p = await db.Produto
        .Where(x => x.Id == id)
        .Select(x => new { x.Id, x.Codigo, x.Descricao, x.Saldo })
        .FirstOrDefaultAsync();

    return p is null ? Results.NotFound() : Results.Ok(p);
});

app.MapPost("/api/estoque/produtos", async (EstoqueDbContext db, ProdutoCreateDto dto) =>
{
    var ultimoCodigo = await db.Produto
        .OrderByDescending(p => p.Id)
        .Select(p => p.Codigo)
        .FirstOrDefaultAsync();

    int proximoNumero = 1;

    if (!string.IsNullOrEmpty(ultimoCodigo) && ultimoCodigo.Length > 1)
    {
        if (int.TryParse(ultimoCodigo[1..], out var n))
            proximoNumero = n + 1;
    }

    string novoCodigo = $"P{proximoNumero.ToString().PadLeft(5, '0')}";

    var produto = new Produto
    {
        Descricao = dto.Descricao,
        Saldo = dto.Saldo,
        Codigo = novoCodigo
    };

    db.Produto.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/api/produtos/{produto.Id}", new
    {
        produto.Id,
        produto.Codigo,
        produto.Descricao,
        produto.Saldo
    });
});

app.MapPut("/api/estoque/produtos/{id:int}", async (EstoqueDbContext db, int id, ProdutoUpdateDto dto) =>
{
    var produto = await db.Produto.FirstOrDefaultAsync(p => p.Id == id);
    if (produto is null)
        return Results.NotFound(new { message = "Produto não encontrado." });

    produto.Descricao = dto.Descricao;
    produto.Saldo = dto.Saldo;

    await db.SaveChangesAsync();

    return Results.Ok(new {
        produto.Id,
        produto.Codigo,
        produto.Descricao,
        produto.Saldo
    });
});

app.MapDelete("/api/estoque/produtos/{id:int}", async (EstoqueDbContext db, int id) =>
{
    var produto = await db.Produto.FindAsync(id);
    if (produto is null)
        return Results.NotFound(new { message = "Produto não encontrado." });

    db.Produto.Remove(produto);
    await db.SaveChangesAsync();

    return Results.Ok(new { message = "Produto removido com sucesso." });
});

app.MapPost("/api/estoque/produtos/baixas", async (EstoqueDbContext db, List<BaixaProdutoDto> itens) =>
{
    if(itens is null || itens.Count == 0)
        return Results.BadRequest(new { message = "Nenhum item enviado." });

    var invalidos = itens.Where(i => i.Qtd <= 0).ToList();
    if(invalidos.Count > 0)
        return Results.BadRequest(new
        {
            message = "Existem itens com quantidade inválida (menor que 0)."
        });

    var ids = itens.Select(i => i.ProdutoId).Distinct().ToList();
    var produtos = await db.Produto
        .Where(p => ids.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id);

    foreach(var item in itens)
    {
        if(!produtos.TryGetValue(item.ProdutoId, out var prod))
            return Results.NotFound(new { message = $"Produto {item.ProdutoId} não encontrado." });

        if(prod.Saldo < item.Qtd)
            return Results.Conflict(new { message = $"Saldo insuficiente para o produto {item.ProdutoId}." });

        prod.Saldo -= item.Qtd;
    }

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        ok = true,
        updatedCount = itens.Count,
        results = itens.Select(i => new { produtoId = i.ProdutoId, qtd = i.Qtd, status = "ok" }).ToList()
    });
});

app.MapGet("/api/estoque/produtos/{id:int}/disponibilidade", async (EstoqueDbContext db, int id, int quantidade) =>
{
    var produto = await db.Produto.FirstOrDefaultAsync(p => p.Id == id);

    if(produto is null)
        return Results.NotFound(new { message = "Produto não encontrado." });

    var suficiente = produto.Saldo >= quantidade;

    if (!suficiente)
        return Results.Conflict(new
        {
            id,
            saldo = produto.Saldo,
            requerido = quantidade,
            suficiente = false
        });

    return Results.Ok(new
    {
        id,
        saldo = produto.Saldo,
        requerido = quantidade,
        suficiente = true
    });
});

app.Run();