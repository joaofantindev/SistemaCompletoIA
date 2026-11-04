using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Infraestrutura;

/// <summary>
/// Sobe a aplicação real para testes de integração, usando um arquivo SQLite
/// temporário e um administrador de teste.
/// </summary>
public sealed class FabricaAplicacaoWebTeste : WebApplicationFactory<Program>
{
    private readonly string _caminhoArquivoBancoDados =
        Path.Combine(Path.GetTempPath(), $"completesystem-testes-{Guid.NewGuid():N}.db");

    public const string EmailAdministrador = "admin@teste.com";
    public const string SenhaAdministrador = "Admin@12345";

    protected override void ConfigureWebHost(IWebHostBuilder construtorAplicacao)
    {
        construtorAplicacao.UseEnvironment("Testing");

        construtorAplicacao.ConfigureAppConfiguration((_, construtorConfiguracao) =>
        {
            construtorConfiguracao.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_caminhoArquivoBancoDados}",
                ["AdministradorInicial:Nome"] = "Administrador de Teste",
                ["AdministradorInicial:Email"] = EmailAdministrador,
                ["AdministradorInicial:Senha"] = SenhaAdministrador
            });
        });
    }

    public async Task<UsuarioResponseDTO> CriarUsuarioAsync(
        string nome,
        string email,
        string perfil,
        string senha = "Senha@123")
    {
        using var escopo = Services.CreateScope();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        return await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = nome,
            Email = email,
            Senha = senha,
            Perfil = perfil
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (File.Exists(_caminhoArquivoBancoDados))
        {
            File.Delete(_caminhoArquivoBancoDados);
        }
    }
}
