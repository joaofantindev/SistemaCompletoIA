namespace CompleteSystem.Models.DTOs;

public static class RegraCategoriaMapperDTO
{
    public static RegraCategoriaRespostaDTO ToResponseDTO(this RegraCategoria regra) => new()
    {
        Id = regra.Id,
        CategoriaId = regra.CategoriaId,
        NomeCategoria = regra.Categoria?.Nome ?? string.Empty,
        Descricao = regra.Descricao,
        PercentualDesconto = regra.PercentualDesconto,
        Ativa = regra.Ativa,
        CriadaEm = regra.CriadaEm
    };

    public static RegraCategoriaUpdateDTO ToUpdateDTO(this RegraCategoriaRespostaDTO regra) => new()
    {
        Id = regra.Id,
        CategoriaId = regra.CategoriaId,
        Descricao = regra.Descricao,
        PercentualDesconto = regra.PercentualDesconto,
        Ativa = regra.Ativa
    };
}
