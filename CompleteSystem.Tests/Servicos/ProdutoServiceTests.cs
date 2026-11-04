using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using CompleteSystem.Tests.Infraestrutura;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Servicos;

public class ProdutoServiceTests
{
    [Fact]
    public async Task CadastrarProdutoAsync_deve_gravar_produto_com_categoria_e_proprietario()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Eletrônicos",
            Descricao = "Aparelhos eletrônicos"
        });

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Loja do João", "loja@teste.com");

        var produto = await servicoProduto.CadastrarProdutoAsync(
            new ProdutoCreateDTO
            {
                Nome = "  Fone Bluetooth  ",
                Marca = "  SoundMax  ",
                Preco = 199.90m,
                CategoriaId = categoria.Id
            },
            proprietario.Id);

        Assert.True(produto.Id > 0);
        Assert.Equal("Fone Bluetooth", produto.Nome);
        Assert.Equal("SoundMax", produto.Marca);
        Assert.Equal("Eletrônicos", produto.NomeCategoria);
        Assert.Equal("Loja do João", produto.NomeProprietario);
        Assert.Equal(199.90m, produto.Preco);
    }

    [Fact]
    public async Task CadastrarProdutoAsync_deve_recusar_categoria_inexistente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Dono", "dono@teste.com");

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoProduto.CadastrarProdutoAsync(
                new ProdutoCreateDTO
                {
                    Nome = "Produto",
                    Marca = "Marca",
                    Preco = 10m,
                    CategoriaId = 9999
                },
                proprietario.Id));

        Assert.Contains("categoria informada não existe", excecao.Message);
    }

    [Fact]
    public async Task CadastrarProdutoAsync_deve_recusar_categoria_inativa()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Descontinuada",
            Descricao = "Categoria inativa",
            Ativa = false
        });

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Dono", "dono@teste.com");

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoProduto.CadastrarProdutoAsync(
                new ProdutoCreateDTO
                {
                    Nome = "Produto",
                    Marca = "Marca",
                    Preco = 10m,
                    CategoriaId = categoria.Id
                },
                proprietario.Id));

        Assert.Contains("categoria informada está inativa", excecao.Message);
    }

    [Fact]
    public async Task CadastrarProdutoAsync_deve_recusar_nome_repetido_na_mesma_categoria()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Papelaria",
            Descricao = "Materiais de escritório"
        });

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Dono", "dono@teste.com");

        var dadosProduto = new ProdutoCreateDTO
        {
            Nome = "Caneta Azul",
            Marca = "Bic",
            Preco = 2.50m,
            CategoriaId = categoria.Id
        };

        await servicoProduto.CadastrarProdutoAsync(dadosProduto, proprietario.Id);

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoProduto.CadastrarProdutoAsync(dadosProduto, proprietario.Id));

        Assert.Contains("com este nome nesta categoria", excecao.Message);
    }

    [Fact]
    public async Task AtualizarProdutoAsync_deve_alterar_produto_existente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Cozinha",
            Descricao = "Utensílios de cozinha"
        });

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Dono", "dono@teste.com");

        var produto = await servicoProduto.CadastrarProdutoAsync(
            new ProdutoCreateDTO
            {
                Nome = "Panela",
                Marca = "Panelux",
                Preco = 80m,
                CategoriaId = categoria.Id
            },
            proprietario.Id);

        var atualizou = await servicoProduto.AtualizarProdutoAsync(new ProdutoUpdateDTO
        {
            Id = produto.Id,
            Nome = "Panela Grande",
            Marca = "Panelux",
            Preco = 120m,
            CategoriaId = categoria.Id
        });

        var produtoAtualizado = await servicoProduto.ObterProdutoPorIdentificadorAsync(produto.Id);

        Assert.True(atualizou);
        Assert.NotNull(produtoAtualizado);
        Assert.Equal("Panela Grande", produtoAtualizado!.Nome);
        Assert.Equal(120m, produtoAtualizado.Preco);
    }

    [Fact]
    public async Task AtualizarProdutoAsync_deve_retornar_false_quando_produto_nao_existe()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();

        var atualizou = await servicoProduto.AtualizarProdutoAsync(new ProdutoUpdateDTO
        {
            Id = 12345,
            Nome = "Inexistente",
            Marca = "Nenhuma",
            Preco = 1m,
            CategoriaId = 1
        });

        Assert.False(atualizou);
    }

    [Fact]
    public async Task RemoverProdutoAsync_deve_apagar_produto_existente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoCategoria = escopo.ServiceProvider.GetRequiredService<ICategoriaService>();
        var servicoProduto = escopo.ServiceProvider.GetRequiredService<IProdutoService>();
        var contexto = escopo.ServiceProvider.GetRequiredService<CompleteSystem.Data.AppDbContext>();

        var categoria = await servicoCategoria.CadastrarCategoriaAsync(new CategoriaCreateDTO
        {
            Nome = "Jardim",
            Descricao = "Itens de jardim"
        });

        var proprietario = await DadosTeste.CriarUsuarioDiretoAsync(contexto, "Dono", "dono@teste.com");

        var produto = await servicoProduto.CadastrarProdutoAsync(
            new ProdutoCreateDTO
            {
                Nome = "Vaso",
                Marca = "Jardim Verde",
                Preco = 45m,
                CategoriaId = categoria.Id
            },
            proprietario.Id);

        Assert.True(await servicoProduto.RemoverProdutoAsync(produto.Id));
        Assert.Null(await servicoProduto.ObterProdutoPorIdentificadorAsync(produto.Id));
    }
}
