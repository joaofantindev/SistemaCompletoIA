using CompleteSystem.Data;
using CompleteSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Tests.Infraestrutura;

/// <summary>
/// Dados auxiliares usados pelos testes de serviço.
/// </summary>
public static class DadosTeste
{
    public static async Task<Usuario> CriarUsuarioDiretoAsync(
        AppDbContext contextoBancoDados,
        string nome,
        string email)
    {
        var usuario = new Usuario
        {
            Nome = nome,
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        contextoBancoDados.Users.Add(usuario);
        await contextoBancoDados.SaveChangesAsync();
        return usuario;
    }

    public static async Task<Categoria> CriarCategoriaDiretoAsync(
        AppDbContext contextoBancoDados,
        string nome)
    {
        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = $"Categoria {nome}",
            Ativa = true,
            CriadaEm = DateTime.UtcNow
        };

        contextoBancoDados.Categorias.Add(categoria);
        await contextoBancoDados.SaveChangesAsync();
        return categoria;
    }

    public static async Task<Produto> CriarProdutoDiretoAsync(
        AppDbContext contextoBancoDados,
        string nome,
        decimal preco,
        int identificadorCategoria,
        int? identificadorProprietario = null)
    {
        var produto = new Produto
        {
            Nome = nome,
            Marca = "Marca Teste",
            Preco = preco,
            CategoriaId = identificadorCategoria,
            ProprietarioId = identificadorProprietario,
            CadastradoEm = DateTime.UtcNow
        };

        contextoBancoDados.Produtos.Add(produto);
        await contextoBancoDados.SaveChangesAsync();
        return produto;
    }

    public static async Task<RegraCategoria> CriarRegraDiretoAsync(
        AppDbContext contextoBancoDados,
        int identificadorCategoria,
        decimal percentualDesconto,
        bool ativa = true)
    {
        var regra = new RegraCategoria
        {
            CategoriaId = identificadorCategoria,
            Descricao = $"Desconto de {percentualDesconto}%",
            PercentualDesconto = percentualDesconto,
            Ativa = ativa,
            CriadaEm = DateTime.UtcNow
        };

        contextoBancoDados.RegrasCategoria.Add(regra);
        await contextoBancoDados.SaveChangesAsync();
        return regra;
    }
}
