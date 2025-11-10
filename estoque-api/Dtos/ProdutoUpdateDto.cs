namespace estoque_api.Dtos;

public class ProdutoUpdateDto
{
    public string Descricao { get; set; } = default!;
    public int Saldo { get; set; }
}
