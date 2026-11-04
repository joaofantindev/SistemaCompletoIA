using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Services;

public class CompraService : ICompraService
{
    private readonly AppDbContext _contextoBancoDados;

    public CompraService(AppDbContext contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    public async Task<IReadOnlyList<CompraRespostaDTO>> ListarComprasDoClienteAsync(int identificadorCliente)
    {
        var compras = await ConsultaDeCompras()
            .Where(compra => compra.ClienteId == identificadorCliente)
            .OrderByDescending(compra => compra.RealizadaEm)
            .ToListAsync();

        return compras.Select(compra => compra.ToResponseDTO()).ToList();
    }

    public async Task<IReadOnlyList<CompraRespostaDTO>> ListarTodasComprasAsync()
    {
        var compras = await ConsultaDeCompras()
            .OrderByDescending(compra => compra.RealizadaEm)
            .ToListAsync();

        return compras.Select(compra => compra.ToResponseDTO()).ToList();
    }

    public async Task<CompraRespostaDTO> RealizarCompraAsync(
        CompraCreateDTO dadosCompra,
        int identificadorCliente)
    {
        if (dadosCompra.ProdutoId is null or <= 0)
        {
            throw new RegraDeNegocioException("Selecione um produto válido para comprar.");
        }

        if (dadosCompra.Quantidade <= 0)
        {
            throw new RegraDeNegocioException("A quantidade comprada deve ser maior que zero.");
        }

        var clienteExiste = await _contextoBancoDados.Users
            .AsNoTracking()
            .AnyAsync(usuario => usuario.Id == identificadorCliente);

        if (!clienteExiste)
        {
            throw new RegraDeNegocioException("O cliente da compra não foi encontrado.");
        }

        var produto = await _contextoBancoDados.Produtos
            .AsNoTracking()
            .Include(produto => produto.Categoria)
            .FirstOrDefaultAsync(produto => produto.Id == dadosCompra.ProdutoId.Value);

        if (produto is null)
        {
            throw new RegraDeNegocioException("O produto selecionado não existe.");
        }

        if (produto.Categoria is { Ativa: false })
        {
            throw new RegraDeNegocioException("A categoria deste produto está inativa.");
        }

        var percentualDesconto = await ObterPercentualDescontoDaCategoriaAsync(produto.CategoriaId);

        var valorUnitario = produto.Preco;
        var valorTotalSemDesconto = valorUnitario * dadosCompra.Quantidade;
        var valorTotal = Math.Round(
            valorTotalSemDesconto * (1 - (percentualDesconto / 100m)),
            2,
            MidpointRounding.AwayFromZero);

        var novaCompra = new Compra
        {
            ClienteId = identificadorCliente,
            ProdutoId = produto.Id,
            NomeProduto = produto.Nome,
            Quantidade = dadosCompra.Quantidade,
            ValorUnitario = valorUnitario,
            PercentualDescontoAplicado = percentualDesconto,
            ValorTotal = valorTotal,
            RealizadaEm = DateTime.UtcNow
        };

        _contextoBancoDados.Compras.Add(novaCompra);
        await _contextoBancoDados.SaveChangesAsync();

        var compraSalva = await ConsultaDeCompras()
            .FirstAsync(compra => compra.Id == novaCompra.Id);

        return compraSalva.ToResponseDTO();
    }

    private IQueryable<Compra> ConsultaDeCompras()
    {
        return _contextoBancoDados.Compras
            .AsNoTracking()
            .Include(compra => compra.Cliente)
            .Include(compra => compra.Produto);
    }

    private async Task<decimal> ObterPercentualDescontoDaCategoriaAsync(int? identificadorCategoria)
    {
        if (identificadorCategoria is null)
        {
            return 0m;
        }

        var regraAtiva = await _contextoBancoDados.RegrasCategoria
            .AsNoTracking()
            .Where(regra => regra.CategoriaId == identificadorCategoria.Value && regra.Ativa)
            .OrderByDescending(regra => regra.PercentualDesconto)
            .FirstOrDefaultAsync();

        return regraAtiva?.PercentualDesconto ?? 0m;
    }
}
