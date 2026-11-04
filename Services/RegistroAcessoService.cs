using CompleteSystem.Data;
using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CompleteSystem.Services;

public class RegistroAcessoService : IRegistroAcessoService
{
    private readonly AppDbContext _contextoBancoDados;

    public RegistroAcessoService(AppDbContext contextoBancoDados)
    {
        _contextoBancoDados = contextoBancoDados;
    }

    public async Task RegistrarLoginAsync(string emailTentativa, bool loginBemSucedido, string? detalhes = null)
    {
        var emailNormalizado = emailTentativa.Trim().ToLowerInvariant();

        var usuario = await _contextoBancoDados.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Email == emailNormalizado);

        var registro = new RegistroAcesso
        {
            UsuarioId = loginBemSucedido ? usuario?.Id : null,
            Email = emailNormalizado,
            TipoAcao = loginBemSucedido ? TipoAcaoRegistro.LoginBemSucedido : TipoAcaoRegistro.LoginFalho,
            MetodoHttp = "POST",
            Caminho = "/Account/Login",
            CodigoStatus = loginBemSucedido ? StatusCodes.Status302Found : StatusCodes.Status401Unauthorized,
            Detalhes = detalhes,
            OcorridoEm = DateTime.UtcNow
        };

        _contextoBancoDados.RegistrosAcesso.Add(registro);
        await _contextoBancoDados.SaveChangesAsync();
    }

    public async Task RegistrarRequisicaoAsync(
        int? identificadorUsuario,
        string? email,
        string metodoHttp,
        string caminho,
        int codigoStatus,
        string? enderecoIp,
        string? navegador)
    {
        var tipoAcao = codigoStatus is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden
            ? TipoAcaoRegistro.AcessoNegado
            : TipoAcaoRegistro.Requisicao;

        var registro = new RegistroAcesso
        {
            UsuarioId = identificadorUsuario,
            Email = email,
            TipoAcao = tipoAcao,
            MetodoHttp = metodoHttp,
            Caminho = caminho,
            CodigoStatus = codigoStatus,
            EnderecoIp = enderecoIp,
            Navegador = navegador,
            OcorridoEm = DateTime.UtcNow
        };

        _contextoBancoDados.RegistrosAcesso.Add(registro);
        await _contextoBancoDados.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<RegistroAcessoRespostaDTO>> ListarRegistrosAsync(
        string? email,
        TipoAcaoRegistro? tipoAcao,
        DateTime? dataInicial,
        DateTime? dataFinal)
    {
        var consulta = _contextoBancoDados.RegistrosAcesso
            .AsNoTracking()
            .Include(registro => registro.Usuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();
            consulta = consulta.Where(registro => registro.Email != null &&
                registro.Email.ToLower().Contains(emailNormalizado));
        }

        if (tipoAcao.HasValue)
        {
            consulta = consulta.Where(registro => registro.TipoAcao == tipoAcao.Value);
        }

        if (dataInicial.HasValue)
        {
            consulta = consulta.Where(registro => registro.OcorridoEm >= dataInicial.Value);
        }

        if (dataFinal.HasValue)
        {
            var fimDoDia = dataFinal.Value.Date.AddDays(1).AddTicks(-1);
            consulta = consulta.Where(registro => registro.OcorridoEm <= fimDoDia);
        }

        var registros = await consulta
            .OrderByDescending(registro => registro.OcorridoEm)
            .Take(500)
            .ToListAsync();

        return registros.Select(registro => registro.ToResponseDTO()).ToList();
    }

    public async Task<RegistroAcessoRespostaDTO?> ObterRegistroPorIdentificadorAsync(long identificadorRegistro)
    {
        var registro = await _contextoBancoDados.RegistrosAcesso
            .AsNoTracking()
            .Include(registro => registro.Usuario)
            .FirstOrDefaultAsync(registro => registro.Id == identificadorRegistro);

        return registro?.ToResponseDTO();
    }

    public async Task<bool> RemoverRegistroAsync(long identificadorRegistro)
    {
        var registro = await _contextoBancoDados.RegistrosAcesso
            .FirstOrDefaultAsync(registro => registro.Id == identificadorRegistro);

        if (registro is null)
        {
            return false;
        }

        _contextoBancoDados.RegistrosAcesso.Remove(registro);
        await _contextoBancoDados.SaveChangesAsync();
        return true;
    }

    public async Task<int> RemoverRegistrosAnterioresAsync(DateTime limite)
    {
        var registrosAntigos = await _contextoBancoDados.RegistrosAcesso
            .Where(registro => registro.OcorridoEm < limite)
            .ToListAsync();

        _contextoBancoDados.RegistrosAcesso.RemoveRange(registrosAntigos);
        await _contextoBancoDados.SaveChangesAsync();
        return registrosAntigos.Count;
    }
}
