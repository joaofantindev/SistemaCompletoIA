using CompleteSystem.Models.DTOs;

namespace CompleteSystem.Services;

public interface ICompraService
{
    Task<IReadOnlyList<CompraRespostaDTO>> ListarComprasDoClienteAsync(int identificadorCliente);
    Task<IReadOnlyList<CompraRespostaDTO>> ListarTodasComprasAsync();
    Task<CompraRespostaDTO> RealizarCompraAsync(CompraCreateDTO dadosCompra, int identificadorCliente);
}
