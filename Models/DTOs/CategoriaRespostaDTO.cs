namespace CompleteSystem.Models.DTOs;

public class CategoriaRespostaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativa { get; set; }
    public DateTime CriadaEm { get; set; }
    public int QuantidadeProdutos { get; set; }
}
