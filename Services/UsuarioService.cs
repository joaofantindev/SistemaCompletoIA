using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Services;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _contextoBancoDados;
    private readonly UserManager<Usuario> _gerenciadorUsuarios;
    private readonly RoleManager<IdentityRole<int>> _gerenciadorPerfis;

    public UsuarioService(
        AppDbContext contextoBancoDados,
        UserManager<Usuario> gerenciadorUsuarios,
        RoleManager<IdentityRole<int>> gerenciadorPerfis)
    {
        _contextoBancoDados = contextoBancoDados;
        _gerenciadorUsuarios = gerenciadorUsuarios;
        _gerenciadorPerfis = gerenciadorPerfis;
    }

    public async Task<IReadOnlyList<UsuarioResponseDTO>> ListarUsuariosAsync()
    {
        var usuarios = await _contextoBancoDados.Users
            .AsNoTracking()
            .OrderBy(usuario => usuario.Nome)
            .ToListAsync();

        var usuariosResposta = new List<UsuarioResponseDTO>(usuarios.Count);

        foreach (var usuario in usuarios)
        {
            usuariosResposta.Add(await MapearUsuarioParaRespostaAsync(usuario));
        }

        return usuariosResposta;
    }

    public async Task<UsuarioResponseDTO?> ObterUsuarioPorIdentificadorAsync(int identificadorUsuario)
    {
        var usuario = await _contextoBancoDados.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == identificadorUsuario);

        return usuario is null ? null : await MapearUsuarioParaRespostaAsync(usuario);
    }

    public async Task<UsuarioResponseDTO> CadastrarUsuarioAsync(UsuarioCreateDTO dadosCadastroUsuario)
    {
        var emailNormalizado = NormalizarEmail(dadosCadastroUsuario.Email);
        var perfilSolicitado = dadosCadastroUsuario.Perfil.Trim();

        await GarantirQuePerfilExisteAsync(perfilSolicitado);

        var usuarioComMesmoEmail = await _gerenciadorUsuarios.FindByEmailAsync(emailNormalizado);

        if (usuarioComMesmoEmail is not null)
        {
            throw new RegraDeNegocioException("Já existe um usuário com este e-mail.");
        }

        var novoUsuario = new Usuario
        {
            Nome = dadosCadastroUsuario.Nome.Trim(),
            UserName = emailNormalizado,
            Email = emailNormalizado,
            EmailConfirmed = true,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };

        var resultadoCriacao = await _gerenciadorUsuarios.CreateAsync(novoUsuario, dadosCadastroUsuario.Senha);
        GarantirQueOperacaoDeuCerto(resultadoCriacao, "Não foi possível cadastrar o usuário.");

        var resultadoPerfil = await _gerenciadorUsuarios.AddToRoleAsync(novoUsuario, perfilSolicitado);
        GarantirQueOperacaoDeuCerto(resultadoPerfil, "Não foi possível atribuir o perfil ao usuário.");

        return novoUsuario.ToResponseDTO(perfilSolicitado);
    }

    public async Task<bool> AtualizarUsuarioAsync(UsuarioUpdateDTO dadosAtualizacaoUsuario)
    {
        var usuario = await _gerenciadorUsuarios.FindByIdAsync(dadosAtualizacaoUsuario.Id.ToString());

        if (usuario is null)
        {
            return false;
        }

        var emailNormalizado = NormalizarEmail(dadosAtualizacaoUsuario.Email);
        var usuarioComMesmoEmail = await _gerenciadorUsuarios.FindByEmailAsync(emailNormalizado);

        if (usuarioComMesmoEmail is not null && usuarioComMesmoEmail.Id != usuario.Id)
        {
            throw new RegraDeNegocioException("Já existe outro usuário com este e-mail.");
        }

        var perfilSolicitado = dadosAtualizacaoUsuario.Perfil.Trim();
        await GarantirQuePerfilExisteAsync(perfilSolicitado);

        var perfisAtuais = await _gerenciadorUsuarios.GetRolesAsync(usuario);
        var perfilAtual = perfisAtuais.FirstOrDefault() ?? string.Empty;

        if (!string.Equals(perfilAtual, perfilSolicitado, StringComparison.Ordinal))
        {
            await GarantirQueNaoRemoveUltimoAdministradorAsync(usuario);
        }

        usuario.Nome = dadosAtualizacaoUsuario.Nome.Trim();
        usuario.Email = emailNormalizado;
        usuario.UserName = emailNormalizado;
        usuario.Ativo = dadosAtualizacaoUsuario.Ativo;

        var resultadoAtualizacao = await _gerenciadorUsuarios.UpdateAsync(usuario);
        GarantirQueOperacaoDeuCerto(resultadoAtualizacao, "Não foi possível atualizar o usuário.");

        if (!string.Equals(perfilAtual, perfilSolicitado, StringComparison.Ordinal))
        {
            if (perfisAtuais.Count > 0)
            {
                var resultadoRemocao = await _gerenciadorUsuarios.RemoveFromRolesAsync(usuario, perfisAtuais);
                GarantirQueOperacaoDeuCerto(resultadoRemocao, "Não foi possível remover o perfil antigo do usuário.");
            }

            var resultadoNovoPerfil = await _gerenciadorUsuarios.AddToRoleAsync(usuario, perfilSolicitado);
            GarantirQueOperacaoDeuCerto(resultadoNovoPerfil, "Não foi possível atribuir o novo perfil ao usuário.");
        }

        return true;
    }

    public async Task<bool> RemoverUsuarioAsync(int identificadorUsuario)
    {
        var usuario = await _gerenciadorUsuarios.FindByIdAsync(identificadorUsuario.ToString());

        if (usuario is null)
        {
            return false;
        }

        await GarantirQueNaoRemoveUltimoAdministradorAsync(usuario);

        var resultadoRemocao = await _gerenciadorUsuarios.DeleteAsync(usuario);
        GarantirQueOperacaoDeuCerto(resultadoRemocao, "Não foi possível remover o usuário.");

        return true;
    }

    private async Task<UsuarioResponseDTO> MapearUsuarioParaRespostaAsync(Usuario usuario)
    {
        var perfis = await _gerenciadorUsuarios.GetRolesAsync(usuario);
        return usuario.ToResponseDTO(perfis.FirstOrDefault());
    }

    private async Task GarantirQuePerfilExisteAsync(string nomePerfil)
    {
        if (string.IsNullOrWhiteSpace(nomePerfil) || !await _gerenciadorPerfis.RoleExistsAsync(nomePerfil))
        {
            throw new RegraDeNegocioException("O perfil de acesso informado não existe.");
        }
    }

    private async Task GarantirQueNaoRemoveUltimoAdministradorAsync(Usuario usuario)
    {
        var usuarioEhAdministrador = await _gerenciadorUsuarios.IsInRoleAsync(usuario, PerfisAcesso.Administrador);

        if (!usuarioEhAdministrador)
        {
            return;
        }

        var administradores = await _gerenciadorUsuarios.GetUsersInRoleAsync(PerfisAcesso.Administrador);

        var existeOutroAdministradorAtivo = administradores.Any(administrador =>
            administrador.Id != usuario.Id && administrador.Ativo);

        if (!existeOutroAdministradorAtivo)
        {
            throw new RegraDeNegocioException(
                "Não é possível remover ou rebaixar o último administrador ativo do sistema.");
        }
    }

    private static string NormalizarEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static void GarantirQueOperacaoDeuCerto(IdentityResult resultado, string mensagemPadrao)
    {
        if (resultado.Succeeded)
        {
            return;
        }

        var mensagensDeErro = string.Join(" ", resultado.Errors.Select(erro => erro.Description));

        throw new RegraDeNegocioException(
            string.IsNullOrWhiteSpace(mensagensDeErro) ? mensagemPadrao : mensagensDeErro);
    }
}
