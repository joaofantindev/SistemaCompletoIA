using System.Reflection;
using CompleteSystem.Controllers;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CompleteSystem.Tests.Seguranca;

/// <summary>
/// Garante que os perfis de acesso estão declarados nos controllers e que
/// nenhum DTO de resposta expõe senha ou hash.
/// </summary>
public class AutorizacaoTests
{
    [Fact]
    public void PerfisDeAcesso_devem_ser_exatamente_os_quatro_esperados()
    {
        Assert.Equal(
            new[] { "Admin", "Manager", "Owner", "Client" },
            PerfisAcesso.Todos);
    }

    [Fact]
    public void UsuarioController_deve_exigir_perfil_administrador()
    {
        var perfis = ObterPerfisDaClasse(typeof(UsuarioController));

        Assert.Contains(PerfisAcesso.Administrador, perfis);
        Assert.Single(perfis);
    }

    [Theory]
    [InlineData(nameof(CategoriaController.Create))]
    [InlineData(nameof(CategoriaController.Edit))]
    [InlineData(nameof(CategoriaController.Delete))]
    [InlineData(nameof(CategoriaController.DeleteConfirmed))]
    public void CategoriaController_gerenciamento_deve_exigir_admin_ou_gerente(string nomeAcao)
    {
        var perfis = ObterPerfisDaAcao(typeof(CategoriaController), nomeAcao);

        Assert.Contains(PerfisAcesso.Administrador, perfis);
        Assert.Contains(PerfisAcesso.Gerente, perfis);
        Assert.DoesNotContain(PerfisAcesso.Cliente, perfis);
        Assert.DoesNotContain(PerfisAcesso.Proprietario, perfis);
    }

    [Theory]
    [InlineData(nameof(ProdutoController.Create))]
    [InlineData(nameof(ProdutoController.Edit))]
    [InlineData(nameof(ProdutoController.Delete))]
    [InlineData(nameof(ProdutoController.DeleteConfirmed))]
    public void ProdutoController_gerenciamento_deve_exigir_admin_ou_proprietario(string nomeAcao)
    {
        var perfis = ObterPerfisDaAcao(typeof(ProdutoController), nomeAcao);

        Assert.Contains(PerfisAcesso.Administrador, perfis);
        Assert.Contains(PerfisAcesso.Proprietario, perfis);
        Assert.DoesNotContain(PerfisAcesso.Cliente, perfis);
        Assert.DoesNotContain(PerfisAcesso.Gerente, perfis);
    }

    [Fact]
    public void RegraCategoriaController_deve_exigir_admin_ou_gerente()
    {
        var perfis = ObterPerfisDaClasse(typeof(RegraCategoriaController));

        Assert.Contains(PerfisAcesso.Administrador, perfis);
        Assert.Contains(PerfisAcesso.Gerente, perfis);
    }

    [Fact]
    public void CompraController_deve_exigir_admin_ou_cliente()
    {
        var perfis = ObterPerfisDaClasse(typeof(CompraController));

        Assert.Contains(PerfisAcesso.Administrador, perfis);
        Assert.Contains(PerfisAcesso.Cliente, perfis);
    }

    [Fact]
    public void RegistroAcessoController_deve_exigir_administrador()
    {
        var perfis = ObterPerfisDaClasse(typeof(RegistroAcessoController));

        Assert.Equal(new[] { PerfisAcesso.Administrador }, perfis);
    }

    [Fact]
    public void Login_deve_permitir_acesso_anonimo()
    {
        var metodoLogin = typeof(AccountController)
            .GetMethods()
            .First(metodo => metodo.Name == nameof(AccountController.Login)
                && metodo.GetParameters().Any(parametro => parametro.ParameterType == typeof(LoginDTO)));

        Assert.NotNull(metodoLogin.GetCustomAttribute<AllowAnonymousAttribute>());
    }

    [Fact]
    public void DtoDeRespostaDeUsuario_nao_deve_expor_senha_ou_hash()
    {
        var nomesDasPropriedades = typeof(UsuarioResponseDTO)
            .GetProperties()
            .Select(propriedade => propriedade.Name)
            .ToList();

        Assert.DoesNotContain(nomesDasPropriedades, nome =>
            nome.Contains("Senha", StringComparison.OrdinalIgnoreCase) ||
            nome.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
            nome.Contains("Hash", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(typeof(ProdutoRespostaDTO))]
    [InlineData(typeof(CategoriaRespostaDTO))]
    [InlineData(typeof(RegraCategoriaRespostaDTO))]
    [InlineData(typeof(CompraRespostaDTO))]
    public void DtosDeResposta_nao_devem_expor_senha_ou_hash(Type tipoDoDto)
    {
        var nomesDasPropriedades = tipoDoDto
            .GetProperties()
            .Select(propriedade => propriedade.Name)
            .ToList();

        Assert.DoesNotContain(nomesDasPropriedades, nome =>
            nome.Contains("Senha", StringComparison.OrdinalIgnoreCase) ||
            nome.Contains("Password", StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> ObterPerfisDaClasse(Type tipoDoController)
    {
        var atributo = tipoDoController.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(atributo);
        Assert.False(string.IsNullOrWhiteSpace(atributo!.Roles));

        return atributo.Roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static IReadOnlyList<string> ObterPerfisDaAcao(Type tipoDoController, string nomeDaAcao)
    {
        var metodo = tipoDoController
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .First(metodo => metodo.Name == nomeDaAcao);

        var atributo = metodo.GetCustomAttribute<AuthorizeAttribute>()
            ?? tipoDoController.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(atributo);
        Assert.False(string.IsNullOrWhiteSpace(atributo!.Roles));

        return atributo.Roles
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
