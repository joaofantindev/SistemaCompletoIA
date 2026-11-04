using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using CompleteSystem.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Servicos;

public class RegraCategoriaServiceTests
{
    [Fact]
    public async Task CadastrarRegraAsync_deve_gravar_regra_valida()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoRegra = escopo.ServiceProvider.GetRequiredService<IRegraCategoriaService>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Promoções",
            Descricao = "Categoria com desconto"
        });

        var regra = await servicoRegra.CadastrarRegraAsync(new RegraCategoriaCreateDTO
        {
            CategoriaId = categoria.Id,
            Descricao = "Desconto de volta às aulas",
            PercentualDesconto = 15m,
            Ativa = true
        });

        Assert.True(regra.Id > 0);
        Assert.Equal("Promoções", regra.NomeCategoria);
        Assert.Equal(15m, regra.PercentualDesconto);
    }

    [Fact]
    public async Task CadastrarRegraAsync_deve_recusar_categoria_inexistente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoRegra = escopo.ServiceProvider.GetRequiredService<IRegraCategoriaService>();

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoRegra.CadastrarRegraAsync(new RegraCategoriaCreateDTO
            {
                CategoriaId = 9999,
                Descricao = "Regra inválida",
                PercentualDesconto = 10m
            }));

        Assert.Contains("categoria informada não existe", excecao.Message);
    }

    [Fact]
    public async Task CadastrarRegraAsync_deve_recusar_descricao_repetida_na_categoria()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoRegra = escopo.ServiceProvider.GetRequiredService<IRegraCategoriaService>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Livros",
            Descricao = "Livros"
        });

        await servicoRegra.CadastrarRegraAsync(new RegraCategoriaCreateDTO
        {
            CategoriaId = categoria.Id,
            Descricao = "Desconto especial",
            PercentualDesconto = 5m
        });

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoRegra.CadastrarRegraAsync(new RegraCategoriaCreateDTO
            {
                CategoriaId = categoria.Id,
                Descricao = "desconto especial",
                PercentualDesconto = 10m
            }));

        Assert.Contains("com esta descrição nesta categoria", excecao.Message);
    }

    [Fact]
    public async Task AtualizarRegraAsync_deve_alterar_regra_existente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoRegra = escopo.ServiceProvider.GetRequiredService<IRegraCategoriaService>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Casa",
            Descricao = "Itens para casa"
        });

        var regra = await servicoRegra.CadastrarRegraAsync(new RegraCategoriaCreateDTO
        {
            CategoriaId = categoria.Id,
            Descricao = "Desconto inicial",
            PercentualDesconto = 5m
        });

        var atualizou = await servicoRegra.AtualizarRegraAsync(new RegraCategoriaUpdateDTO
        {
            Id = regra.Id,
            CategoriaId = categoria.Id,
            Descricao = "Desconto atualizado",
            PercentualDesconto = 12m,
            Ativa = false
        });

        var regraAtualizada = await servicoRegra.ObterRegraPorIdentificadorAsync(regra.Id);

        Assert.True(atualizou);
        Assert.NotNull(regraAtualizada);
        Assert.Equal(12m, regraAtualizada!.PercentualDesconto);
        Assert.False(regraAtualizada.Ativa);
    }

    [Fact]
    public async Task RemoverRegraAsync_deve_apagar_regra_existente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoRegra = escopo.ServiceProvider.GetRequiredService<IRegraCategoriaService>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Removível",
            Descricao = "Categoria de teste"
        });

        var regra = await servicoRegra.CadastrarRegraAsync(new RegraCategoriaCreateDTO
        {
            CategoriaId = categoria.Id,
            Descricao = "Regra removível",
            PercentualDesconto = 3m
        });

        Assert.True(await servicoRegra.RemoverRegraAsync(regra.Id));
        Assert.Null(await servicoRegra.ObterRegraPorIdentificadorAsync(regra.Id));
    }
}
