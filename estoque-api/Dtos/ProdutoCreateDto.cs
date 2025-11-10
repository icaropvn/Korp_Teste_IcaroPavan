namespace estoque_api.Dtos;

public class ProdutoCreateDto
{
    public string Descricao { get; set; } = default!;
    public int Saldo { get; set; }
}
