namespace faturamento_api.Dtos;

public class NotaCreateDto
{
    public List<ItemNotaCreateDto> Itens { get; set; } = new();
}