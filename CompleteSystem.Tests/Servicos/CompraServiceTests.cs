using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using CompleteSystem.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Servicos;

public class CompraServiceTests
{
    [Fact]
    public async Task RealizarCompraAsync_deve_aplicar_desconto_da_regra_ativa()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var cliente = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente", "cliente@teste.com");
        var categoria = await DadosTeste.CriarCategoriaDiretoAsync(contexto, "Promoção");
        var produto = await DadosTeste.CriarProdutoDiretoAsync(contexto, "Camiseta", 100m, categoria.Id);
        await DadosTeste.CriarRegraDiretoAsync(contexto, categoria.Id, 10m);

        var compra = await servicoCompra.RealizarCompraAsync(
            new CompraCreateDTO { ProdutoId = produto.Id, Quantidade = 2 },
            cliente.Id);

        Assert.Equal(200m, compra.ValorUnitario * compra.Quantidade);
        Assert.Equal(10m, compra.PercentualDescontoAplicado);
        Assert.Equal(180m, compra.ValorTotal);
        Assert.Equal("Cliente", compra.NomeCliente);
        Assert.Equal("Camiseta", compra.NomeProduto);
    }

    [Fact]
    public async Task RealizarCompraAsync_deve_ignorar_regra_inativa()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var cliente = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente", "cliente@teste.com");
        var categoria = await DadosTeste.CriarCategoriaDiretoAsync(contexto, "Sem promoção");
        var produto = await DadosTeste.CriarProdutoDiretoAsync(contexto, "Calça", 200m, categoria.Id);
        await DadosTeste.CriarRegraDiretoAsync(contexto, categoria.Id, 50m, ativa: false);

        var compra = await servicoCompra.RealizarCompraAsync(
            new CompraCreateDTO { ProdutoId = produto.Id, Quantidade = 1 },
            cliente.Id);

        Assert.Equal(0m, compra.PercentualDescontoAplicado);
        Assert.Equal(200m, compra.ValorTotal);
    }

    [Fact]
    public async Task RealizarCompraAsync_deve_usar_o_maior_desconto_entre_as_regras_ativas()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var cliente = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente", "cliente@teste.com");
        var categoria = await DadosTeste.CriarCategoriaDiretoAsync(contexto, "Descontos");
        var produto = await DadosTeste.CriarProdutoDiretoAsync(contexto, "Tênis", 100m, categoria.Id);
        await DadosTeste.CriarRegraDiretoAsync(contexto, categoria.Id, 5m);
        await DadosTeste.CriarRegraDiretoAsync(contexto, categoria.Id, 20m);

        var compra = await servicoCompra.RealizarCompraAsync(
            new CompraCreateDTO { ProdutoId = produto.Id, Quantidade = 1 },
            cliente.Id);

        Assert.Equal(20m, compra.PercentualDescontoAplicado);
        Assert.Equal(80m, compra.ValorTotal);
    }

    [Fact]
    public async Task RealizarCompraAsync_deve_recusar_quantidade_menor_que_um()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoCompra.RealizarCompraAsync(
                new CompraCreateDTO { ProdutoId = 1, Quantidade = 0 },
                1));

        Assert.Contains("maior que zero", excecao.Message);
    }

    [Fact]
    public async Task RealizarCompraAsync_deve_recusar_produto_inexistente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var cliente = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente", "cliente@teste.com");

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoCompra.RealizarCompraAsync(
                new CompraCreateDTO { ProdutoId = 9999, Quantidade = 1 },
                cliente.Id));

        Assert.Contains("produto selecionado não existe", excecao.Message);
    }

    [Fact]
    public async Task RealizarCompraAsync_deve_recusar_produto_de_categoria_inativa()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var cliente = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente", "cliente@teste.com");
        var categoria = await DadosTeste.CriarCategoriaDiretoAsync(contexto, "Inativa");
        categoria.Ativa = false;
        await contexto.SaveChangesAsync();
        var produto = await DadosTeste.CriarProdutoDiretoAsync(contexto, "Produto", 50m, categoria.Id);

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoCompra.RealizarCompraAsync(
                new CompraCreateDTO { ProdutoId = produto.Id, Quantidade = 1 },
                cliente.Id));

        Assert.Contains("categoria deste produto está inativa", excecao.Message);
    }

    [Fact]
    public async Task ListarComprasDoClienteAsync_deve_trazer_somente_compras_do_cliente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCompra = escopo.ServiceProvider.GetRequiredService<ICompraService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var clienteUm = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente Um", "um@teste.com");
        var clienteDois = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Cliente Dois", "dois@teste.com");
        var categoria = await DadosTeste.CriarCategoriaDiretoAsync(contexto, "Geral");
        var produto = await DadosTeste.CriarProdutoDiretoAsync(contexto, "Item", 10m, categoria.Id);

        await servicoCompra.RealizarCompraAsync(new CompraCreateDTO { ProdutoId = produto.Id, Quantidade = 1 }, clienteUm.Id);
        await servicoCompra.RealizarCompraAsync(new CompraCreateDTO { ProdutoId = produto.Id, Quantidade = 1 }, clienteDois.Id);

        var comprasClienteUm = await servicoCompra.ListarComprasDoClienteAsync(clienteUm.Id);
        var todasAsCompras = await servicoCompra.ListarTodasComprasAsync();

        Assert.Single(comprasClienteUm);
        Assert.Equal(clienteUm.Id, comprasClienteUm[0].ClienteId);
        Assert.Equal(2, todasAsCompras.Count);
    }
}
