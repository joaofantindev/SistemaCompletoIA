namespace CompleteSystem.Models.DTOs;

public static class ProdutoMapperDTO
{
    public static ProdutoRespostaDTO ToResponseDTO(this Produto produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        Marca = produto.Marca,
        Preco = produto.Preco,
        CategoriaId = produto.CategoriaId,
        NomeCategoria = produto.Categoria?.Nome ?? string.Empty,
        ProprietarioId = produto.ProprietarioId,
        NomeProprietario = produto.Proprietario?.Nome ?? string.Empty,
        CadastradoEm = produto.CadastradoEm
    };

    public static ProdutoUpdateDTO ToUpdateDTO(this ProdutoRespostaDTO produto) => new()
    {
        Id = produto.Id,
        Nome = produto.Nome,
        Marca = produto.Marca,
        Preco = produto.Preco,
        CategoriaId = produto.CategoriaId
    };
}
