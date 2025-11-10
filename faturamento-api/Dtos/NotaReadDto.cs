namespace faturamento_api.Dtos;
public record NotaReadDto(int Id, long? Numero, string Status, List<NotaItemReadDto> Itens);
public record NotaItemReadDto(int Id, int ProdutoId, int Quantidade, decimal Preco);