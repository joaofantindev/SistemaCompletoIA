using CompleteSystem.Models.DTOs;

namespace CompleteSystem.Services;

public interface IRegraCategoriaService
{
    Task<IReadOnlyList<RegraCategoriaRespostaDTO>> ListarRegrasAsync();
    Task<RegraCategoriaRespostaDTO?> ObterRegraPorIdentificadorAsync(int identificadorRegra);
    Task<RegraCategoriaRespostaDTO> CadastrarRegraAsync(RegraCategoriaCreateDTO dadosCadastroRegra);
    Task<bool> AtualizarRegraAsync(RegraCategoriaUpdateDTO dadosAtualizacaoRegra);
    Task<bool> RemoverRegraAsync(int identificadorRegra);
}
