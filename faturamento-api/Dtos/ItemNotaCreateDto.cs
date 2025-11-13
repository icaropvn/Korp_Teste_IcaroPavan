namespace faturamento_api.Dtos;

public class ItemNotaCreateDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }
}