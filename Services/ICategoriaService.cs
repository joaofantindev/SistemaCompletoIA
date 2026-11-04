using CompleteSystem.Models.DTOs;

namespace CompleteSystem.Services;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaRespostaDTO>> ListarCategoriasAsync();
    Task<CategoriaRespostaDTO?> ObterCategoriaPorIdentificadorAsync(int identificadorCategoria);
    Task<CategoriaRespostaDTO> CadastrarCategoriaAsync(CategoriaCreateDTO dadosCadastroCategoria);
    Task<bool> AtualizarCategoriaAsync(CategoriaUpdateDTO dadosAtualizacaoCategoria);
    Task<bool> RemoverCategoriaAsync(int identificadorCategoria);
}
