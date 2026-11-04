using CompleteSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Data;

/// <summary>
/// Prepara o banco na inicialização: aplica migrations, cria os perfis de
/// acesso e garante um administrador inicial para o primeiro login.
/// </summary>
public static class InicializadorBancoDados
{
    private const string EmailAdministradorPadrao = "admin@completesystem.com";
    private const string SenhaAdministradorPadrao = "Admin@12345";
    private const string NomeAdministradorPadrao = "Administrador do Sistema";

    public static async Task InicializarAsync(
        IServiceProvider provedorServicos,
        IConfiguration configuracao,
        ILogger logger)
    {
        using var escopo = provedorServicos.CreateScope();
        var servicosDoEscopo = escopo.ServiceProvider;

        var contextoBancoDados = servicosDoEscopo.GetRequiredService<AppDbContext>();
        await contextoBancoDados.Database.MigrateAsync();

        await CriarPerfisDeAcessoAsync(servicosDoEscopo);
        await CriarAdministradorInicialAsync(servicosDoEscopo, configuracao, logger);
    }

    private static async Task CriarPerfisDeAcessoAsync(IServiceProvider servicos)
    {
        var gerenciadorPerfis = servicos.GetRequiredService<RoleManager<IdentityRole<int>>>();

        foreach (var nomePerfil in PerfisAcesso.Todos)
        {
            if (await gerenciadorPerfis.RoleExistsAsync(nomePerfil))
            {
                continue;
            }

            var resultadoCriacao = await gerenciadorPerfis.CreateAsync(new IdentityRole<int>(nomePerfil));

            if (!resultadoCriacao.Succeeded)
            {
                var mensagens = string.Join(" ", resultadoCriacao.Errors.Select(erro => erro.Description));
                throw new InvalidOperationException($"Não foi possível criar o perfil '{nomePerfil}': {mensagens}");
            }
        }
    }

    private static async Task CriarAdministradorInicialAsync(
        IServiceProvider servicos,
        IConfiguration configuracao,
        ILogger logger)
    {
        var gerenciadorUsuarios = servicos.GetRequiredService<UserManager<Usuario>>();

        var emailAdministrador = configuracao["AdministradorInicial:Email"] ?? EmailAdministradorPadrao;
        var senhaAdministrador = configuracao["AdministradorInicial:Senha"] ?? SenhaAdministradorPadrao;
        var nomeAdministrador = configuracao["AdministradorInicial:Nome"] ?? NomeAdministradorPadrao;

        var emailNormalizado = emailAdministrador.Trim().ToLowerInvariant();
        var administradorExistente = await gerenciadorUsuarios.FindByEmailAsync(emailNormalizado);

        if (administradorExistente is not null)
        {
            return;
        }

        var administrador = new Usuario
        {
            Nome = nomeAdministrador.Trim(),
            UserName = emailNormalizado,
            Email = emailNormalizado,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var resultadoCriacao = await gerenciadorUsuarios.CreateAsync(administrador, senhaAdministrador);

        if (!resultadoCriacao.Succeeded)
        {
            var mensagens = string.Join(" ", resultadoCriacao.Errors.Select(erro => erro.Description));
            logger.LogError("Não foi possível criar o administrador inicial: {Mensagens}", mensagens);
            return;
        }

        await gerenciadorUsuarios.AddToRoleAsync(administrador, PerfisAcesso.Administrador);
        logger.LogInformation("Administrador inicial criado com o e-mail {Email}.", emailNormalizado);
    }
}
