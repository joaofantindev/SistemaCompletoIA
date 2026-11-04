namespace CompleteSystem.Models.DTOs;

/// <summary>
/// Dados devolvidos ao controller e às Views. Não contém senha nem hash.
/// </summary>
public class UsuarioResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public string Perfil { get; set; } = string.Empty;
}
