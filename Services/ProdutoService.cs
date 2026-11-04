using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Services;

public class ProdutoService : IProdutoService
{
    private readonly AppDbContext _contextoBancoDados;

    public ProdutoService(AppDbContext contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyList<ProdutoRespostaDTO>> ListarProdutosAsync()
    {
        var produtos = await ConsultaDeProdutos()
            .OrderBy(produto => produto.Nome)
            .ToListAsync();

        return produtos.Select(produto => produto.ToResponseDTO()).ToList();
    }

    public async Task<IReadOnlyList<ProdutoRespostaDTO>> ListarProdutosPorCategoriaAsync(int identificadorCategoria)
    {
        var produtos = await ConsultaDeProdutos()
            .Where(produto => produto.CategoriaId == identificadorCategoria)
            .OrderBy(produto => produto.Nome)
            .ToListAsync();

        return produtos.Select(produto => produto.ToResponseDTO()).ToList();
    }

    public async Task<ProdutoRespostaDTO?> ObterProdutoPorIdentificadorAsync(int identificadorProduto)
    {
        var produto = await ConsultaDeProdutos()
            .FirstOrDefaultAsync(produto => produto.Id == identificadorProduto);

        return produto?.ToResponseDTO();
    }

    public async Task<ProdutoRespostaDTO> CadastrarProdutoAsync(
        ProdutoCreateDTO dadosCadastroProduto,
        int identificadorProprietario)
    {
        var identificadorCategoria = await ValidarCategoriaAsync(dadosCadastroProduto.CategoriaId);
        var nomeProduto = dadosCadastroProduto.Nome.Trim();

        await GarantirQueProprietarioExisteAsync(identificadorProprietario);
        await GarantirQueNomeNaoRepeteNaCategoriaAsync(nomeProduto, identificadorCategoria, identificadorProdutoIgnorado: null);

        var novoProduto = new Produto
        {
            Nome = nomeProduto,
            Marca = dadosCadastroProduto.Marca.Trim(),
            Preco = dadosCadastroProduto.Preco,
            CategoriaId = identificadorCategoria,
            ProprietarioId = identificadorProprietario,
            CadastradoEm = DateTime.UtcNow
        };

        _contextoBancoDados.Produtos.Add(novoProduto);
        await _contextoBancoDados.SaveChangesAsync();

        return await ObterProdutoPorIdentificadorAsync(novoProduto.Id)
            ?? novoProduto.ToResponseDTO();
    }

    public async Task<bool> AtualizarProdutoAsync(ProdutoUpdateDTO dadosAtualizacaoProduto)
    {
        var produto = await _contextoBancoDados.Produtos
            .FirstOrDefaultAsync(produto => produto.Id == dadosAtualizacaoProduto.Id);

        if (produto is null)
        {
            return false;
        }

        var identificadorCategoria = await ValidarCategoriaAsync(dadosAtualizacaoProduto.CategoriaId);
        var nomeProduto = dadosAtualizacaoProduto.Nome.Trim();

        await GarantirQueNomeNaoRepeteNaCategoriaAsync(
            nomeProduto,
            identificadorCategoria,
            identificadorProdutoIgnorado: produto.Id);

        produto.Nome = nomeProduto;
        produto.Marca = dadosAtualizacaoProduto.Marca.Trim();
        produto.Preco = dadosAtualizacaoProduto.Preco;
        produto.CategoriaId = identificadorCategoria;

        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverProdutoAsync(int identificadorProduto)
    {
        var produto = await _contextoBancoDados.Produtos
            .FirstOrDefaultAsync(produto => produto.Id == identificadorProduto);

        if (produto is null)
        {
            return false;
        }

        _contextoBancoDados.Produtos.Remove(produto);
        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    private IQueryable<Produto> ConsultaDeProdutos()
    {
        return _contextoBancoDados.Produtos
            .AsNoTracking()
            .Include(produto => produto.Categoria)
            .Include(produto => produto.Proprietario);
    }

    private async Task<int> ValidarCategoriaAsync(int? identificadorCategoria)
    {
        if (identificadorCategoria is null or <= 0)
        {
            throw new RegraDeNegocioException("Selecione uma categoria válida para o produto.");
        }

        var categoria = await _contextoBancoDados.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(categoria => categoria.Id == identificadorCategoria.Value);

        if (categoria is null)
        {
            throw new RegraDeNegocioException("A categoria informada não existe.");
        }

        if (!categoria.Ativa)
        {
            throw new RegraDeNegocioException("A categoria informada está inativa.");
        }

        return categoria.Id;
    }

    private async Task GarantirQueProprietarioExisteAsync(int identificadorProprietario)
    {
        var proprietarioExiste = await _contextoBancoDados.Users
            .AsNoTracking()
            .AnyAsync(usuario => usuario.Id == identificadorProprietario);

        if (!proprietarioExiste)
        {
            throw new RegraDeNegocioException("O proprietário do produto não foi encontrado.");
        }
    }

    private async Task GarantirQueNomeNaoRepeteNaCategoriaAsync(
        string nomeProduto,
        int identificadorCategoria,
        int? identificadorProdutoIgnorado)
    {
        var nomeNormalizado = nomeProduto.ToLower();

        var produtoComMesmoNome = await _contextoBancoDados.Produtos
            .AsNoTracking()
            .AnyAsync(produto =>
                produto.CategoriaId == identificadorCategoria &&
                produto.Nome.ToLower() == nomeNormalizado &&
                (!identificadorProdutoIgnorado.HasValue || produto.Id != identificadorProdutoIgnorado.Value));

        if (produtoComMesmoNome)
        {
            throw new RegraDeNegocioException("Já existe um produto com este nome nesta categoria.");
        }
    }
}
