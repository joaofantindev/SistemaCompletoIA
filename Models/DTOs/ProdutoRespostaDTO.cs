namespace CompleteSystem.Models.DTOs;

/// <summary>
/// Dados de saída de produto. Traz os nomes de categoria e proprietário
/// já resolvidos para a View não precisar navegar entre entidades.
/// </summary>
public class ProdutoRespostaDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int? CategoriaId { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;
    public int? ProprietarioId { get; set; }
    public string NomeProprietario { get; set; } = string.Empty;
    public DateTime CadastradoEm { get; set; }
}
