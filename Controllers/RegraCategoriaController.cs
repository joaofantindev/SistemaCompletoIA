using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

[Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
public class RegraCategoriaController : Controller
{
    private readonly IRegraCategoriaService _servicoRegraCategoria;
    private readonly ICategoriaService _servicoCategoria;

    public RegraCategoriaController(
        IRegraCategoriaService servicoRegraCategoria,
        ICategoriaService servicoCategoria)
    {
        _servicoRegraCategoria = servicoRegraCategoria;
        _servicoCategoria = servicoCategoria;
    }

    public async Task<IActionResult> Index()
    {
        var regras = await _servicoRegraCategoria.ListarRegrasAsync();
        return View(regras);
    }

    public async Task<IActionResult> Details(int identificador)
    {
        var regra = await _servicoRegraCategoria.ObterRegraPorIdentificadorAsync(identificador);

        if (regra is null)
        {
            return NotFound();
        }

        return View(regra);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasDisponiveisAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegraCategoriaCreateDTO dadosCadastroRegra)
    {
        if (!ModelState.IsValid)
        {
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosCadastroRegra);
        }

        try
        {
            await _servicoRegraCategoria.CadastrarRegraAsync(dadosCadastroRegra);
            TempData["Sucesso"] = "Regra cadastrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosCadastroRegra);
        }
    }

    public async Task<IActionResult> Edit(int identificador)
    {
        var regra = await _servicoRegraCategoria.ObterRegraPorIdentificadorAsync(identificador);

        if (regra is null)
        {
            return NotFound();
        }

        await CarregarCategoriasDisponiveisAsync();
        return View(regra.ToUpdateDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int identificador, RegraCategoriaUpdateDTO dadosAtualizacaoRegra)
    {
        if (identificador != dadosAtualizacaoRegra.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosAtualizacaoRegra);
        }

        try
        {
            var regraAtualizada = await _servicoRegraCategoria.AtualizarRegraAsync(dadosAtualizacaoRegra);

            if (!regraAtualizada)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Regra atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosAtualizacaoRegra);
        }
    }

    public async Task<IActionResult> Delete(int identificador)
    {
        var regra = await _servicoRegraCategoria.ObterRegraPorIdentificadorAsync(identificador);

        if (regra is null)
        {
            return NotFound();
        }

        return View(regra);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int identificador)
    {
        var regraRemovida = await _servicoRegraCategoria.RemoverRegraAsync(identificador);

        if (!regraRemovida)
        {
            return NotFound();
        }

        TempData["Sucesso"] = "Regra removida.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarCategoriasDisponiveisAsync()
    {
        var categorias = await _servicoCategoria.ListarCategoriasAsync();
        ViewBag.CategoriasDisponiveis = categorias;
    }
}
