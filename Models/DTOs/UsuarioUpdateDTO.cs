using System.ComponentModel.DataAnnotations;
using CompleteSystem.Models;

namespace CompleteSystem.Models.DTOs;

/// <summary>
/// Dados de entrada do formulário de edição de usuário.
/// O perfil só pode ser alterado por um administrador.
/// </summary>
public class UsuarioUpdateDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(256, ErrorMessage = "O e-mail deve ter no máximo 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;

    [Required(ErrorMessage = "Selecione um perfil de acesso.")]
    public string Perfil { get; set; } = PerfisAcesso.Cliente;
}
