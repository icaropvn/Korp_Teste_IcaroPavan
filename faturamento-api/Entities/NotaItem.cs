namespace faturamento_api.Entities;

public class NotaItem
{
    public int Id { get; set; }
    public int NotaId { get; set; }
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
}
