namespace faturamento_api.Entities;

public class IdempotencyKey
{
    public int Id { get; set; }
    public string Chave { get; set; } = default!;
    public string Rota { get; set; } = default!;
    public int StatusHttp { get; set; }
    public string RespostaJson { get; set; } = default!;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
}
