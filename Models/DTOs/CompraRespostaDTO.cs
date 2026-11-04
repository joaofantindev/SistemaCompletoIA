namespace CompleteSystem.Models.DTOs;

public class CompraRespostaDTO
{
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public int? ProdutoId { get; set; }
    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal PercentualDescontoAplicado { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime RealizadaEm { get; set; }
}
