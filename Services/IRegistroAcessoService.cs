using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;

namespace CompleteSystem.Services;

public interface IRegistroAcessoService
{
    Task RegistrarLoginAsync(string emailTentativa, bool loginBemSucedido, string? detalhes = null);
    Task RegistrarRequisicaoAsync(
        int? identificadorUsuario,
        string? email,
        string metodoHttp,
        string caminho,
        int codigoStatus,
        string? enderecoIp,
        string? navegador);
    Task<IReadOnlyList<RegistroAcessoRespostaDTO>> ListarRegistrosAsync(
        string? email,
        TipoAcaoRegistro? tipoAcao,
        DateTime? dataInicial,
        DateTime? dataFinal);
    Task<RegistroAcessoRespostaDTO?> ObterRegistroPorIdentificadorAsync(long identificadorRegistro);
    Task<bool> RemoverRegistroAsync(long identificadorRegistro);
    Task<int> RemoverRegistrosAnterioresAsync(DateTime limite);
}
