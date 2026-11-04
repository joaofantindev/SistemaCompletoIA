using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Services;

public class RegraCategoriaService : IRegraCategoriaService
{
    private readonly AppDbContext _contextoBancoDados;

    public RegraCategoriaService(AppDbContext contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyList<RegraCategoriaRespostaDTO>> ListarRegrasAsync()
    {
        var regras = await ConsultaDeRegras()
            .OrderBy(regra => regra.Categoria!.Nome)
            .ThenBy(regra => regra.Descricao)
            .ToListAsync();

        return regras.Select(regra => regra.ToResponseDTO()).ToList();
    }

    public async Task<RegraCategoriaRespostaDTO?> ObterRegraPorIdentificadorAsync(int identificadorRegra)
    {
        var regra = await ConsultaDeRegras()
            .FirstOrDefaultAsync(regra => regra.Id == identificadorRegra);

        return regra?.ToResponseDTO();
    }

    public async Task<RegraCategoriaRespostaDTO> CadastrarRegraAsync(RegraCategoriaCreateDTO dadosCadastroRegra)
    {
        var identificadorCategoria = await ValidarCategoriaAsync(dadosCadastroRegra.CategoriaId);
        var descricaoRegra = dadosCadastroRegra.Descricao.Trim();

        await GarantirQueDescricaoNaoRepeteNaCategoriaAsync(
            descricaoRegra,
            identificadorCategoria,
            identificadorRegraIgnorada: null);

        var novaRegra = new RegraCategoria
        {
            CategoriaId = identificadorCategoria,
            Descricao = descricaoRegra,
            PercentualDesconto = dadosCadastroRegra.PercentualDesconto,
            Ativa = dadosCadastroRegra.Ativa,
            CriadaEm = DateTime.UtcNow
        };

        _contextoBancoDados.RegrasCategoria.Add(novaRegra);
        await _contextoBancoDados.SaveChangesAsync();

        return await ObterRegraPorIdentificadorAsync(novaRegra.Id)
            ?? novaRegra.ToResponseDTO();
    }

    public async Task<bool> AtualizarRegraAsync(RegraCategoriaUpdateDTO dadosAtualizacaoRegra)
    {
        var regra = await _contextoBancoDados.RegrasCategoria
            .FirstOrDefaultAsync(regra => regra.Id == dadosAtualizacaoRegra.Id);

        if (regra is null)
        {
            return false;
        }

        var identificadorCategoria = await ValidarCategoriaAsync(dadosAtualizacaoRegra.CategoriaId);
        var descricaoRegra = dadosAtualizacaoRegra.Descricao.Trim();

        await GarantirQueDescricaoNaoRepeteNaCategoriaAsync(
            descricaoRegra,
            identificadorCategoria,
            identificadorRegraIgnorada: regra.Id);

        regra.CategoriaId = identificadorCategoria;
        regra.Descricao = descricaoRegra;
        regra.PercentualDesconto = dadosAtualizacaoRegra.PercentualDesconto;
        regra.Ativa = dadosAtualizacaoRegra.Ativa;

        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoverRegraAsync(int identificadorRegra)
    {
        var regra = await _contextoBancoDados.RegrasCategoria
            .FirstOrDefaultAsync(regra => regra.Id == identificadorRegra);

        if (regra is null)
        {
            return false;
        }

        _contextoBancoDados.RegrasCategoria.Remove(regra);
        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    private IQueryable<RegraCategoria> ConsultaDeRegras()
    {
        return _contextoBancoDados.RegrasCategoria
            .AsNoTracking()
            .Include(regra => regra.Categoria);
    }

    private async Task<int> ValidarCategoriaAsync(int? identificadorCategoria)
    {
        if (identificadorCategoria is null or <= 0)
        {
            throw new RegraDeNegocioException("Selecione uma categoria válida para a regra.");
        }

        var categoriaExiste = await _contextoBancoDados.Categorias
            .AsNoTracking()
            .AnyAsync(categoria => categoria.Id == identificadorCategoria.Value);

        if (!categoriaExiste)
        {
            throw new RegraDeNegocioException("A categoria informada não existe.");
        }

        return identificadorCategoria.Value;
    }

    private async Task GarantirQueDescricaoNaoRepeteNaCategoriaAsync(
        string descricaoRegra,
        int identificadorCategoria,
        int? identificadorRegraIgnorada)
    {
        var descricaoNormalizada = descricaoRegra.ToLower();

        var regraComMesmaDescricao = await _contextoBancoDados.RegrasCategoria
            .AsNoTracking()
            .AnyAsync(regra =>
                regra.CategoriaId == identificadorCategoria &&
                regra.Descricao.ToLower() == descricaoNormalizada &&
                (!identificadorRegraIgnorada.HasValue || regra.Id != identificadorRegraIgnorada.Value));

        if (regraComMesmaDescricao)
        {
            throw new RegraDeNegocioException("Já existe uma regra com esta descrição nesta categoria.");
        }
    }
}
