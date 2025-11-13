namespace faturamento_api.Dtos;

public class NotaUpdateDto
{
    public List<ItemNotaUpdateDto> Itens { get; set; } = new();
}