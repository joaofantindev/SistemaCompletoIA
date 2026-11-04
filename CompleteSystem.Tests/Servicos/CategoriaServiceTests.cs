using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using CompleteSystem.Tests.Infraestrutura;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Servicos;

public class CategoriaServiceTests
{
    [Fact]
    public async Task CadastrarCategoriaAsync_deve_gravar_categoria_valida()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();

        var resposta = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "  Informática  ",
            Descricao = "  Computadores e periféricos  ",
            Ativa = true
        });

        Assert.True(resposta.Id > 0);
        Assert.Equal("Informática", resposta.Nome);
        Assert.Equal("Computadores e periféricos", resposta.Descricao);
        Assert.True(resposta.Ativa);
    }

    [Fact]
    public async Task CadastrarCategoriaAsync_deve_recusar_nome_duplicado_ignorando_maiusculas()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();

        await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Livros",
            Descricao = "Livros em geral"
        });

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
            {
                Nome = "livros",
                Descricao = "Outra descrição"
            }));

        Assert.Contains("categoria com este nome", excecao.Message);
    }

    [Fact]
    public async Task AtualizarCategoriaAsync_deve_alterar_os_dados()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Brinquedos",
            Descricao = "Brinquedos infantis"
        });

        var atualizou = await servicoCategoria.AtualizarCategoriaAsync(new CategoriaUpdateDTO
        {
            Id = categoria.Id,
            Nome = "Brinquedos educativos",
            Descricao = "Brinquedos para aprender",
            Ativa = false
        });

        var categoriaAtualizada = await servicoCategoria.ObterCategoriaPorIdentificadorAsync(categoria.Id);

        Assert.True(atualizou);
        Assert.NotNull(categoriaAtualizada);
        Assert.Equal("Brinquedos educativos", categoriaAtualizada!.Nome);
        Assert.False(categoriaAtualizada.Ativa);
    }

    [Fact]
    public async Task RemoverCategoriaAsync_deve_recusar_categoria_com_produtos()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Games",
            Descricao = "Jogos"
        });

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Dono", "dono@games.com");
        await DadosTeste.CriarProdutoDiretoAsync(contexto, "Controle", 200m, categoria.Id, proprietario.Id);

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoCategoria.RemoverCategoriaAsync(categoria.Id));

        Assert.Contains("possui produtos", excecao.Message);
        Assert.NotNull(await servicoCategoria.ObterCategoriaPorIdentificadorAsync(categoria.Id));
    }

    [Fact]
    public async Task RemoverCategoriaAsync_deve_recusar_categoria_com_regras()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Móveis",
            Descricao = "Móveis para casa"
        });

        await DadosTeste.CriarRegraDiretoAsync(contexto, categoria.Id, 5m);

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoCategoria.RemoverCategoriaAsync(categoria.Id));

        Assert.Contains("possui regras", excecao.Message);
    }

    [Fact]
    public async Task RemoverCategoriaAsync_deve_apagar_categoria_sem_dependencias()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Temporária",
            Descricao = "Categoria que será removida"
        });

        var removeu = await servicoCategoria.RemoverCategoriaAsync(categoria.Id);

        Assert.True(removeu);
        Assert.Null(await servicoCategoria.ObterCategoriaPorIdentificadorAsync(categoria.Id));
        Assert.Empty(await ObterCategoriasDoBancoAsync(escopo));
    }

    private static async Task<List<CompleteSystem.Models.Categoria>> ObterCategoriasDoBancoAsync(IServiceScope escopo)
    {
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();
        return await contexto.Categorias.AsNoTracking().ToListAsync();
    }
}
