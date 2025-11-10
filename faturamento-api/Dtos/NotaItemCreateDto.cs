namespace faturamento_api.Dtos;

public record NotaItemCreateDto(int ProdutoId, int Quantidade, decimal Preco);