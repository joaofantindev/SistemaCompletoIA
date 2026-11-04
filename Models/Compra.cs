namespace CompleteSystem.Models;

/// <summary>
/// Registro de uma compra realizada por um cliente.
/// Os dados do produto são copiados (snapshot) para que o histórico não
/// mude caso o produto seja editado ou removido depois.
/// </summary>
public class Compra
{
    public int Id { get; set; }

    public int? ClienteId { get; set; }
    public Usuario? Cliente { get; set; }

    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal PercentualDescontoAplicado { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime RealizadaEm { get; set; } = DateTime.UtcNow;
}
