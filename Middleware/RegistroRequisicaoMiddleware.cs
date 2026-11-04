using System.Security.Claims;
using CompleteSystem.Services;

namespace CompleteSystem.Middleware;

/// <summary>
/// Grava, para cada requisição HTTP, um registro de acesso/requisição.
/// Usa um escopo de DI próprio para não interferir no DbContext da requisição.
/// </summary>
public class RegistroRequisicaoMiddleware
{
    private static readonly string[] CaminhosIgnorados =
    {
        "/lib/",
        "/css/",
        "/js/",
        "/favicon.ico",
        "/CompleteSystem.styles.css"
    };

    private readonly RequestDelegate _proximoMiddleware;
    private readonly IServiceScopeFactory _fabricaEscopo;
    private readonly ILogger<RegistroRequisicaoMiddleware> _registrador;

    public RegistroRequisicaoMiddleware(
        RequestDelegate proximoMiddleware,
        IServiceScopeFactory fabricaEscopo,
        ILogger<RegistroRequisicaoMiddleware> registrador)
    {
        _proximoMiddleware = proximoMiddleware;
        _fabricaEscopo = fabricaEscopo;
        _registrador = registrador;
    }

    public async Task InvokeAsync(HttpContext contextoHttp)
    {
        await _proximoMiddleware(contextoHttp);

        if (DeveIgnorarRequisicao(contextoHttp))
        {
            return;
        }

        try
        {
            await RegistrarRequisicaoAsync(contextoHttp);
        }
        catch (Exception excecao)
        {
            _registrador.LogWarning(
                excecao,
                "Não foi possível registrar o log da requisição {Metodo} {Caminho}.",
                contextoHttp.Request.Method,
                contextoHttp.Request.Path);
        }
    }

    private async Task RegistrarRequisicaoAsync(HttpContext contextoHttp)
    {
        using var escopo = _fabricaEscopo.CreateScope();
        var servicoRegistroAcesso = escopo.ServiceProvider.GetRequiredService<IRegistroAcessoService>();

        var identificadorUsuario = ObterIdentificadorUsuario(contextoHttp);
        var email = contextoHttp.User.FindFirstValue(ClaimTypes.Email);
        var enderecoIp = contextoHttp.Connection.RemoteIpAddress?.ToString();
        var navegador = contextoHttp.Request.Headers.UserAgent.ToString();

        await servicoRegistroAcesso.RegistrarRequisicaoAsync(
            identificadorUsuario,
            email,
            contextoHttp.Request.Method,
            contextoHttp.Request.Path.Value ?? string.Empty,
            contextoHttp.Response.StatusCode,
            enderecoIp,
            navegador);
    }

    private static int? ObterIdentificadorUsuario(HttpContext contextoHttp)
    {
        var identificadorTexto = contextoHttp.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(identificadorTexto, out var identificadorUsuario)
            ? identificadorUsuario
            : null;
    }

    private static bool DeveIgnorarRequisicao(HttpContext contextoHttp)
    {
        var caminho = contextoHttp.Request.Path.Value ?? string.Empty;

        return CaminhosIgnorados.Any(caminhoIgnorado =>
            caminho.StartsWith(caminhoIgnorado, StringComparison.OrdinalIgnoreCase));
    }
}
