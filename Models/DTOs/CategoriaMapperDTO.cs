namespace CompleteSystem.Models.DTOs;

public static class CategoriaMapperDTO
{
    public static CategoriaRespostaDTO ToResponseDTO(this Categoria categoria, int quantidadeProdutos = 0) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        Ativa = categoria.Ativa,
        CriadaEm = categoria.CriadaEm,
        QuantidadeProdutos = quantidadeProdutos
    };

    public static CategoriaUpdateDTO ToUpdateDTO(this CategoriaRespostaDTO categoria) => new()
    {
        Id = categoria.Id,
        Nome = categoria.Nome,
        Descricao = categoria.Descricao,
        Ativa = categoria.Ativa
    };
}
