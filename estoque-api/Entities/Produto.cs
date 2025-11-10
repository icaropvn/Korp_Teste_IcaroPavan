namespace estoque_api.Entities;

public class Produto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    public int Saldo { get; set; }
}