using CompleteSystem.Models;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

[Authorize(Roles = PerfisAcesso.Administrador)]
public class RegistroAcessoController : Controller
{
    private readonly IRegistroAcessoService _servicoRegistroAcesso;

    public RegistroAcessoController(IRegistroAcessoService servicoRegistroAcesso)
    {
        _servicoRegistroAcesso = servicoRegistroAcesso;
    }

    public async Task<IActionResult> Index(
        string? email,
        TipoAcaoRegistro? tipoAcao,
        DateTime? dataInicial,
        DateTime? dataFinal)
    {
        var registros = await _servicoRegistroAcesso.ListarRegistrosAsync(
            email,
            tipoAcao,
            dataInicial,
            dataFinal);

        ViewBag.EmailFiltro = email;
        ViewBag.TipoAcaoFiltro = tipoAcao;
        ViewBag.DataInicialFiltro = dataInicial?.ToString("yyyy-MM-dd");
        ViewBag.DataFinalFiltro = dataFinal?.ToString("yyyy-MM-dd");
        ViewBag.TiposAcao = Enum.GetValues<TipoAcaoRegistro>();

        return View(registros);
    }

    public async Task<IActionResult> Details(long identificador)
    {
        var registro = await _servicoRegistroAcesso.ObterRegistroPorIdentificadorAsync(identificador);

        if (registro is null)
        {
            return NotFound();
        }

        return View(registro);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long identificador)
    {
        var registroRemovido = await _servicoRegistroAcesso.RemoverRegistroAsync(identificador);

        if (!registroRemovido)
        {
            return NotFound();
        }

        TempData["Sucesso"] = "Registro de acesso removido.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Limpar(int quantidadeDias = 30)
    {
        var diasParaManter = quantidadeDias < 1 ? 1 : quantidadeDias;
        var limite = DateTime.UtcNow.AddDays(-diasParaManter);

        var quantidadeRemovida = await _servicoRegistroAcesso.RemoverRegistrosAnterioresAsync(limite);

        TempData["Sucesso"] = $"{quantidadeRemovida} registro(s) com mais de {diasParaManter} dia(s) removido(s).";
        return RedirectToAction(nameof(Index));
    }
}
