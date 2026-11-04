using CompleteSystem.Models.DTOs;

namespace CompleteSystem.Services;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoRespostaDTO>> ListarProdutosAsync();
    Task<IReadOnlyList<ProdutoRespostaDTO>> ListarProdutosPorCategoriaAsync(int identificadorCategoria);
    Task<ProdutoRespostaDTO?> ObterProdutoPorIdentificadorAsync(int identificadorProduto);
    Task<ProdutoRespostaDTO> CadastrarProdutoAsync(ProdutoCreateDTO dadosCadastroProduto, int identificadorProprietario);
    Task<bool> AtualizarProdutoAsync(ProdutoUpdateDTO dadosAtualizacaoProduto);
    Task<bool> RemoverProdutoAsync(int identificadorProduto);
}
