using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

[Authorize(Roles = PerfisAcesso.Administrador)]
public class UsuarioController : Controller
{
    private readonly IUsuarioService _servicoUsuario;

    public UsuarioController(IUsuarioService servicoUsuario)
    {
        _servicoUsuario = servicoUsuario;
    }

    public async Task<IActionResult> Index()
    {
        var usuarios = await _servicoUsuario.ListarUsuariosAsync();
        return View(usuarios);
    }

    public async Task<IActionResult> Details(int identificador)
    {
        var usuario = await _servicoUsuario.ObterUsuarioPorIdentificadorAsync(identificador);

        if (usuario is null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    public IActionResult Create()
    {
        PreencherPerfisDisponiveis();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCreateDTO dadosCadastroUsuario)
    {
        if (!ModelState.IsValid)
        {
            PreencherPerfisDisponiveis();
            return View(dadosCadastroUsuario);
        }

        try
        {
            await _servicoUsuario.CadastrarUsuarioAsync(dadosCadastroUsuario);
            TempData["Sucesso"] = "Usuário cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            PreencherPerfisDisponiveis();
            return View(dadosCadastroUsuario);
        }
    }

    public async Task<IActionResult> Edit(int identificador)
    {
        var usuario = await _servicoUsuario.ObterUsuarioPorIdentificadorAsync(identificador);

        if (usuario is null)
        {
            return NotFound();
        }

        PreencherPerfisDisponiveis();
        return View(usuario.ToUpdateDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int identificador, UsuarioUpdateDTO dadosAtualizacaoUsuario)
    {
        if (identificador != dadosAtualizacaoUsuario.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            PreencherPerfisDisponiveis();
            return View(dadosAtualizacaoUsuario);
        }

        try
        {
            var usuarioAtualizado = await _servicoUsuario.AtualizarUsuarioAsync(dadosAtualizacaoUsuario);

            if (!usuarioAtualizado)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Usuário atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            PreencherPerfisDisponiveis();
            return View(dadosAtualizacaoUsuario);
        }
    }

    public async Task<IActionResult> Delete(int identificador)
    {
        var usuario = await _servicoUsuario.ObterUsuarioPorIdentificadorAsync(identificador);

        if (usuario is null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int identificador)
    {
        try
        {
            var usuarioRemovido = await _servicoUsuario.RemoverUsuarioAsync(identificador);

            if (!usuarioRemovido)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Usuário removido.";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            TempData["Erro"] = excecao.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    private void PreencherPerfisDisponiveis()
    {
        ViewBag.PerfisDisponiveis = PerfisAcesso.Todos;
    }
}
