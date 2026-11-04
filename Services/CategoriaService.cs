using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Services;

public class CategoriaService : ICategoriaService
{
    private readonly AppDbContext _contextoBancoDados;

    public CategoriaService(AppDbContext contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyList<CategoriaRespostaDTO>> ListarCategoriasAsync()
    {
        var categorias = await _contextoBancoDados.Categorias
            .AsNoTracking()
            .OrderBy(categoria => categoria.Nome)
            .Select(categoria => new CategoriaRespostaDTO
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                Ativa = categoria.Ativa,
                CriadaEm = categoria.CriadaEm,
                QuantidadeProdutos = categoria.Produtos.Count
            })
            .ToListAsync();

        return categorias;
    }

    public async Task<CategoriaRespostaDTO?> ObterCategoriaPorIdentificadorAsync(int identificadorCategoria)
    {
        var categoria = await _contextoBancoDados.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(categoria => categoria.Id == identificadorCategoria);

        if (categoria is null)
        {
            return null;
        }

        var quantidadeProdutos = await _contextoBancoDados.Produtos
            .AsNoTracking()
            .CountAsync(produto => produto.CategoriaId == categoria.Id);

        return categoria.ToResponseDTO(quantidadeProdutos);
    }

    public async Task<CategoriaRespostaDTO> CadastrarCategoriaAsync(CategoriaCreateDTO dadosCadastroCategoria)
    {
        var nomeCategoria = dadosCadastroCategoria.Nome.Trim();
        await GarantirQueNomeNaoEstaEmUsoAsync(nomeCategoria, identificadorCategoriaIgnorada: null);

        var novaCategoria = new Categoria
        {
            Nome = nomeCategoria,
            Descricao = dadosCadastroCategoria.Descricao.Trim(),
            Ativa = dadosCadastroCategoria.Ativa,
            CriadaEm = DateTime.UtcNow
        };

        _contextoBancoDados.Categorias.Add(novaCategoria);
        await _contextoBancoDados.SaveChangesAsync();

        return novaCategoria.ToResponseDTO();
    }

    public async Task<bool> AtualizarCategoriaAsync(CategoriaUpdateDTO dadosAtualizacaoCategoria)
    {
        var categoria = await _contextoBancoDados.Categorias
            .FirstOrDefaultAsync(categoria => categoria.Id == dadosAtualizacaoCategoria.Id);

        if (categoria is null)
        {
            return false;
        }

        var nomeCategoria = dadosAtualizacaoCategoria.Nome.Trim();
        await GarantirQueNomeNaoEstaEmUsoAsync(nomeCategoria, categoria.Id);

        categoria.Nome = nomeCategoria;
        categoria.Descricao = dadosAtualizacaoCategoria.Descricao.Trim();
        categoria.Ativa = dadosAtualizacaoCategoria.Ativa;

        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverCategoriaAsync(int identificadorCategoria)
    {
        var categoria = await _contextoBancoDados.Categorias
            .FirstOrDefaultAsync(categoria => categoria.Id == identificadorCategoria);

        if (categoria is null)
        {
            return false;
        }

        var categoriaPossuiProdutos = await _contextoBancoDados.Produtos
            .AsNoTracking()
            .AnyAsync(produto => produto.CategoriaId == identificadorCategoria);

        if (categoriaPossuiProdutos)
        {
            throw new RegraDeNegocioException(
                "Não é possível remover uma categoria que possui produtos cadastrados.");
        }

        var categoriaPossuiRegras = await _contextoBancoDados.RegrasCategoria
            .AsNoTracking()
            .AnyAsync(regra => regra.CategoriaId == identificadorCategoria);

        if (categoriaPossuiRegras)
        {
            throw new RegraDeNegocioException(
                "Não é possível remover uma categoria que possui regras cadastradas.");
        }

        _contextoBancoDados.Categorias.Remove(categoria);
        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    private async Task GarantirQueNomeNaoEstaEmUsoAsync(
        string nomeCategoria,
        int? identificadorCategoriaIgnorada)
    {
        var nomeNormalizado = nomeCategoria.ToLower();

        var categoriaComMesmoNome = await _contextoBancoDados.Categorias
            .AsNoTracking()
            .AnyAsync(categoria =>
                categoria.Nome.ToLower() == nomeNormalizado &&
                (!identificadorCategoriaIgnorada.HasValue || categoria.Id != identificadorCategoriaIgnorada.Value));

        if (categoriaComMesmoNome)
        {
            throw new RegraDeNegocioException("Já existe uma categoria com este nome.");
        }
    }
}
