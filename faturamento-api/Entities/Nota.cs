namespace faturamento_api.Entities;

public class Nota
{
    public int Id { get; set; }
    public long? Numero { get; set; }
    public string Status { get; set; } = "Aberta";
    public List<NotaItem> Itens { get; set; } = new();
}