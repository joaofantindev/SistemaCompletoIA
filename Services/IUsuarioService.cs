using CompleteSystem.Models.DTOs;

namespace CompleteSystem.Services;

/// <summary>
/// Contrato do serviço de usuários. O controller conhece apenas esta interface.
/// </summary>
public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioResponseDTO>> ListarUsuariosAsync();
    Task<UsuarioResponseDTO?> ObterUsuarioPorIdentificadorAsync(int identificadorUsuario);
    Task<UsuarioResponseDTO> CadastrarUsuarioAsync(UsuarioCreateDTO dadosCadastroUsuario);
    Task<bool> AtualizarUsuarioAsync(UsuarioUpdateDTO dadosAtualizacaoUsuario);
    Task<bool> RemoverUsuarioAsync(int identificadorUsuario);
}
