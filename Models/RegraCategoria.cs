namespace CompleteSystem.Models;

/// <summary>
/// Regra de negócio vinculada a uma categoria. Hoje a regra aplicada é um
/// percentual de desconto usado no momento da compra.
/// </summary>
public class RegraCategoria
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public decimal PercentualDesconto { get; set; }
    public bool Ativa { get; set; } = true;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
}
