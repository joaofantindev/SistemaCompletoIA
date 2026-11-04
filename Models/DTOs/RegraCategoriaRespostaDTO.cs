namespace CompleteSystem.Models.DTOs;

public class RegraCategoriaRespostaDTO
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal PercentualDesconto { get; set; }
    public bool Ativa { get; set; }
    public DateTime CriadaEm { get; set; }
}
