namespace CompleteSystem.Models.DTOs;

public static class CompraMapperDTO
{
    public static CompraRespostaDTO ToResponseDTO(this Compra compra) => new()
    {
        Id = compra.Id,
        ClienteId = compra.ClienteId,
        NomeCliente = compra.Cliente?.Nome ?? string.Empty,
        ProdutoId = compra.ProdutoId,
        NomeProduto = compra.NomeProduto,
        Quantidade = compra.Quantidade,
        ValorUnitario = compra.ValorUnitario,
        PercentualDescontoAplicado = compra.PercentualDescontoAplicado,
        ValorTotal = compra.ValorTotal,
        RealizadaEm = compra.RealizadaEm
    };
}
