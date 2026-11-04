using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

[Authorize]
public class CategoriaController : Controller
{
    private readonly ICategoriaService _servicoCategoria;

    public CategoriaController(ICategoriaService servicoCategoria)
    {
        _servicoCategoria = servicoCategoria;
    }

    public async Task<IActionResult> Index()
    {
        var categorias = await _servicoCategoria.ListarCategoriasAsync();
        return View(categorias);
    }

    public async Task<IActionResult> Details(int identificador)
    {
        var categoria = await _servicoCategoria.ObterCategoriaPorIdentificadorAsync(identificador);

        if (categoria is null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
    public async Task<IActionResult> Create(CategoriaCreateDTO dadosCadastroCategoria)
    {
        if (!ModelState.IsValid)
        {
            return View(dadosCadastroCategoria);
        }

        try
        {
            await _servicoCategoria.CadastrarCategoriaAsync(dadosCadastroCategoria);
            TempData["Sucesso"] = "Categoria cadastrada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            return View(dadosCadastroCategoria);
        }
    }

    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
    public async Task<IActionResult> Edit(int identificador)
    {
        var categoria = await _servicoCategoria.ObterCategoriaPorIdentificadorAsync(identificador);

        if (categoria is null)
        {
            return NotFound();
        }

        return View(categoria.ToUpdateDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
    public async Task<IActionResult> Edit(int identificador, CategoriaUpdateDTO dadosAtualizacaoCategoria)
    {
        if (identificador != dadosAtualizacaoCategoria.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(dadosAtualizacaoCategoria);
        }

        try
        {
            var categoriaAtualizada = await _servicoCategoria.AtualizarCategoriaAsync(dadosAtualizacaoCategoria);

            if (!categoriaAtualizada)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Categoria atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            return View(dadosAtualizacaoCategoria);
        }
    }

    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
    public async Task<IActionResult> Delete(int identificador)
    {
        var categoria = await _servicoCategoria.ObterCategoriaPorIdentificadorAsync(identificador);

        if (categoria is null)
        {
            return NotFound();
        }

        return View(categoria);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Gerente)]
    public async Task<IActionResult> DeleteConfirmed(int identificador)
    {
        try
        {
            var categoriaRemovida = await _servicoCategoria.RemoverCategoriaAsync(identificador);

            if (!categoriaRemovida)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Categoria removida.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            TempData["Erro"] = excecao.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
