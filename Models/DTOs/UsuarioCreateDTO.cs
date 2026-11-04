using System.ComponentModel.DataAnnotations;
using CompleteSystem.Models;

namespace CompleteSystem.Models.DTOs;

/// <summary>
/// Dados de entrada do formulário de cadastro de usuário.
/// A senha existe apenas aqui e nunca é devolvida pela aplicação.
/// </summary>
public class UsuarioCreateDTO
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(256, ErrorMessage = "O e-mail deve ter no máximo 256 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione um perfil de acesso.")]
    public string Perfil { get; set; } = PerfisAcesso.Cliente;
}
