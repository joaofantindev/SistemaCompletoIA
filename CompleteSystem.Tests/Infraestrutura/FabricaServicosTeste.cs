using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CompleteSystem.Tests.Infraestrutura;

/// <summary>
/// Monta um provedor de serviços real (SQLite em memória + Identity) para os
/// testes de serviço. Cada instância tem um banco isolado.
/// </summary>
public sealed class FabricaServicosTeste : IDisposable
{
    private readonly SqliteConnection _conexao;
    public ServiceProvider ProvedorServicos { get; }

    public FabricaServicosTeste()
    {
        _conexao = new SqliteConnection("DataSource=:memory:");
        _conexao.Open();

        var servicos = new ServiceCollection();

        servicos.AddLogging(opcoes => opcoes.SetMinimumLevel(LogLevel.Warning));
        servicos.AddDbContext<AppDbContext>(opcoes => opcoes.UseSqlite(_conexao));

        servicos
            .AddIdentityCore<Usuario>(opcoes =>
            {
                opcoes.Password.RequiredLength = 8;
                opcoes.Password.RequireDigit = true;
                opcoes.Password.RequireUppercase = true;
                opcoes.Password.RequireLowercase = true;
                opcoes.Password.RequireNonAlphanumeric = true;
                opcoes.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<AppDbContext>();

        servicos.AddScoped<IUsuarioService, UsuarioService>();
        servicos.AddScoped<IProdutoService, ProdutoService>();
        servicos.AddScoped<ICategoriaService, CategoriaService>();
        servicos.AddScoped<IRegraCategoriaService, RegraCategoriaService>();
        servicos.AddScoped<ICompraService, CompraService>();
        servicos.AddScoped<IRegistroAcessoService, RegistroAcessoService>();

        ProvedorServicos = servicos.BuildServiceProvider();

        CriarEstruturaECriarPerfis();
    }

    public IServiceScope CriarEscopo() => ProvedorServicos.CreateScope();

    public void Dispose()
    {
        ProvedorServicos.Dispose();
        _conexao.Dispose();
    }

    private void CriarEstruturaECriarPerfis()
    {
        using var escopo = ProvedorServicos.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<AppDbContext>();
        contexto.Database.EnsureCreated();

        var gerenciadorPerfis = escopo.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        foreach (var nomePerfil in PerfisAcesso.Todos)
        {
            var resultado = gerenciadorPerfis.CreateAsync(new IdentityRole<int>(nomePerfil))
                .GetAwaiter()
                .GetResult();

            if (!resultado.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Não foi possível criar o perfil {nomePerfil} no banco de teste.");
            }
        }
    }
}
