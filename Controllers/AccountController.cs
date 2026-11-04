using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<Usuario> _gerenciadorUsuarios;
    private readonly SignInManager<Usuario> _gerenciadorLogin;
    private readonly IRegistroAcessoService _servicoRegistroAcesso;

    public AccountController(
        UserManager<Usuario> gerenciadorUsuarios,
        SignInManager<Usuario> gerenciadorLogin,
        IRegistroAcessoService servicoRegistroAcesso)
    {
        _gerenciadorUsuarios = gerenciadorUsuarios;
        _gerenciadorLogin = gerenciadorLogin;
        _servicoRegistroAcesso = servicoRegistroAcesso;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? urlRetorno = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["UrlRetorno"] = urlRetorno;
        return View(new LoginDTO());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO dadosLogin, string? urlRetorno = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["UrlRetorno"] = urlRetorno;
            return View(dadosLogin);
        }

        var emailNormalizado = dadosLogin.Email.Trim().ToLowerInvariant();
        var usuario = await _gerenciadorUsuarios.FindByEmailAsync(emailNormalizado);

        if (usuario is null)
        {
            await _servicoRegistroAcesso.RegistrarLoginAsync(emailNormalizado, false, "Usuário não encontrado.");
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
            ViewData["UrlRetorno"] = urlRetorno;
            return View(dadosLogin);
        }

        if (!usuario.Ativo)
        {
            await _servicoRegistroAcesso.RegistrarLoginAsync(emailNormalizado, false, "Usuário inativo.");
            ModelState.AddModelError(string.Empty, "Este usuário está inativo. Procure um administrador.");
            ViewData["UrlRetorno"] = urlRetorno;
            return View(dadosLogin);
        }

        var resultadoLogin = await _gerenciadorLogin.PasswordSignInAsync(
            usuario,
            dadosLogin.Senha,
            dadosLogin.LembrarMe,
            lockoutOnFailure: true);

        if (resultadoLogin.Succeeded)
        {
            await _servicoRegistroAcesso.RegistrarLoginAsync(emailNormalizado, true);

            if (!string.IsNullOrWhiteSpace(urlRetorno) && Url.IsLocalUrl(urlRetorno))
            {
                return Redirect(urlRetorno);
            }

            return RedirectToAction("Index", "Home");
        }

        if (resultadoLogin.IsLockedOut)
        {
            await _servicoRegistroAcesso.RegistrarLoginAsync(emailNormalizado, false, "Usuário bloqueado por tentativas inválidas.");
            ModelState.AddModelError(string.Empty, "Usuário temporariamente bloqueado. Tente novamente mais tarde.");
        }
        else
        {
            await _servicoRegistroAcesso.RegistrarLoginAsync(emailNormalizado, false, "Senha inválida.");
            ModelState.AddModelError(string.Empty, "E-mail ou senha inválidos.");
        }

        ViewData["UrlRetorno"] = urlRetorno;
        return View(dadosLogin);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _gerenciadorLogin.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
