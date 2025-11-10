namespace faturamento_api.Dtos;

public record NotaItemUpsertDto(int? Id, int ProdutoId, int Quantidade, decimal Preco);