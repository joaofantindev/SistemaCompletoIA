using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using CompleteSystem.Tests.Infraestrutura;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Servicos;

public class UsuarioServiceTests
{
    private const string SenhaValida = "Senha@123";

    [Fact]
    public async Task CadastrarUsuarioAsync_deve_gerar_hash_e_atribuir_perfil()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();
        var gerenciadorUsuarios = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        var resposta = await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "  Maria Silva  ",
            Email = "  MARIA@TESTE.COM  ",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Cliente
        });

        var usuario = await gerenciadorUsuarios.FindByEmailAsync("maria@teste.com");

        Assert.NotNull(usuario);
        Assert.Equal("Maria Silva", resposta.Nome);
        Assert.Equal("maria@teste.com", resposta.Email);
        Assert.Equal(PerfisAcesso.Cliente, resposta.Perfil);
        Assert.NotEqual(SenhaValida, usuario!.PasswordHash);
        Assert.True(await gerenciadorUsuarios.CheckPasswordAsync(usuario, SenhaValida));
        Assert.True(await gerenciadorUsuarios.IsInRoleAsync(usuario, PerfisAcesso.Cliente));
    }

    [Fact]
    public async Task CadastrarUsuarioAsync_deve_recusar_email_duplicado()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var dadosUsuario = new UsuarioCreateDTO
        {
            Nome = "João",
            Email = "joao@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Cliente
        };

        await servicoUsuario.CadastrarUsuarioAsync(dadosUsuario);

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
            {
                Nome = "Outro João",
                Email = "JOAO@TESTE.COM",
                Senha = SenhaValida,
                Perfil = PerfisAcesso.Cliente
            }));

        Assert.Contains("Já existe um usuário com este e-mail", excecao.Message);
    }

    [Fact]
    public async Task CadastrarUsuarioAsync_deve_recusar_perfil_inexistente()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
            {
                Nome = "Usuário",
                Email = "usuario@teste.com",
                Senha = SenhaValida,
                Perfil = "SuperUsuario"
            }));

        Assert.Contains("perfil de acesso informado não existe", excecao.Message);
    }

    [Fact]
    public async Task AtualizarUsuarioAsync_deve_trocar_o_perfil_do_usuario()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();
        var gerenciadorUsuarios = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        var usuario = await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "Carlos",
            Email = "carlos@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Cliente
        });

        var atualizou = await servicoUsuario.AtualizarUsuarioAsync(new UsuarioUpdateDTO
        {
            Id = usuario.Id,
            Nome = "Carlos Gerente",
            Email = "carlos@teste.com",
            Ativo = true,
            Perfil = PerfisAcesso.Gerente
        });

        var usuarioAtualizado = await gerenciadorUsuarios.FindByEmailAsync("carlos@teste.com");

        Assert.True(atualizou);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal("Carlos Gerente", usuarioAtualizado!.Nome);
        Assert.True(await gerenciadorUsuarios.IsInRoleAsync(usuarioAtualizado, PerfisAcesso.Gerente));
        Assert.False(await gerenciadorUsuarios.IsInRoleAsync(usuarioAtualizado, PerfisAcesso.Cliente));
    }

    [Fact]
    public async Task AtualizarUsuarioAsync_nao_deve_rebaixar_o_ultimo_administrador_ativo()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var administrador = await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "Administrador",
            Email = "admin@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Administrador
        });

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoUsuario.AtualizarUsuarioAsync(new UsuarioUpdateDTO
            {
                Id = administrador.Id,
                Nome = administrador.Nome,
                Email = administrador.Email,
                Ativo = true,
                Perfil = PerfisAcesso.Cliente
            }));

        Assert.Contains("último administrador ativo", excecao.Message);
    }

    [Fact]
    public async Task RemoverUsuarioAsync_nao_deve_remover_o_ultimo_administrador_ativo()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var administrador = await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "Administrador",
            Email = "admin@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Administrador
        });

        var excecao = await Assert.ThrowsAsync<RegraDeNegocioException>(() =>
            servicoUsuario.RemoverUsuarioAsync(administrador.Id));

        Assert.Contains("último administrador ativo", excecao.Message);
    }

    [Fact]
    public async Task AtualizarUsuarioAsync_deve_permitir_rebaixar_quando_existe_outro_administrador()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var administradorUm = await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "Administrador Um",
            Email = "admin1@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Administrador
        });

        await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "Administrador Dois",
            Email = "admin2@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Administrador
        });

        var atualizou = await servicoUsuario.AtualizarUsuarioAsync(new UsuarioUpdateDTO
        {
            Id = administradorUm.Id,
            Nome = administradorUm.Nome,
            Email = administradorUm.Email,
            Ativo = true,
            Perfil = PerfisAcesso.Cliente
        });

        var usuarioAtualizado = await servicoUsuario.ObterUsuarioPorIdentificadorAsync(administradorUm.Id);

        Assert.True(atualizou);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal(PerfisAcesso.Cliente, usuarioAtualizado!.Perfil);
    }

    [Fact]
    public async Task RemoverUsuarioAsync_deve_apagar_usuario_comum()
    {
        using var fabrica = new FabricaServicosTeste();
        using var escopo = fabrica.CriarEscopo();
        var servicoUsuario = escopo.ServiceProvider.GetRequiredService<IUsuarioService>();

        var usuario = await servicoUsuario.CadastrarUsuarioAsync(new UsuarioCreateDTO
        {
            Nome = "Cliente",
            Email = "cliente@teste.com",
            Senha = SenhaValida,
            Perfil = PerfisAcesso.Cliente
        });

        Assert.True(await servicoUsuario.RemoverUsuarioAsync(usuario.Id));
        Assert.Null(await servicoUsuario.ObterUsuarioPorIdentificadorAsync(usuario.Id));
    }
}
