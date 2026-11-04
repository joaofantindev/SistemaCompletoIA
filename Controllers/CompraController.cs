using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

[Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Cliente)]
public class CompraController : Controller
{
    private readonly ICompraService _servicoCompra;
    private readonly IProdutoService _servicoProduto;
    private readonly UserManager<Usuario> _gerenciadorUsuarios;

    public CompraController(
        ICompraService servicoCompra,
        IProdutoService servicoProduto,
        UserManager<Usuario> gerenciadorUsuarios)
    {
        _servicoCompra = servicoCompra;
        _servicoProduto = servicoProduto;
        _gerenciadorUsuarios = gerenciadorUsuarios;
    }

    public async Task<IActionResult> Index()
    {
        if (User.IsInRole(PerfisAcesso.Administrador))
        {
            var todasAsCompras = await _servicoCompra.ListarTodasComprasAsync();
            return View(todasAsCompras);
        }

        var identificadorCliente = ObterIdentificadorUsuarioAtual();

        if (identificadorCliente is null)
        {
            return Challenge();
        }

        var comprasDoCliente = await _servicoCompra.ListarComprasDoClienteAsync(identificadorCliente.Value);
        return View(comprasDoCliente);
    }

    public async Task<IActionResult> Create()
    {
        await CarregarProdutosDisponiveisAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CompraCreateDTO dadosCompra)
    {
        var identificadorCliente = ObterIdentificadorUsuarioAtual();

        if (identificadorCliente is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await CarregarProdutosDisponiveisAsync();
            return View(dadosCompra);
        }

        try
        {
            await _servicoCompra.RealizarCompraAsync(dadosCompra, identificadorCliente.Value);
            TempData["Sucesso"] = "Compra realizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await CarregarProdutosDisponiveisAsync();
            return View(dadosCompra);
        }
    }

    private async Task CarregarProdutosDisponiveisAsync()
    {
        var produtos = await _servicoProduto.ListarProdutosAsync();
        ViewBag.ProdutosDisponiveis = produtos;
    }

    private int? ObterIdentificadorUsuarioAtual()
    {
        var identificadorTexto = _gerenciadorUsuarios.GetUserId(User);
        return int.TryParse(identificadorTexto, out var identificadorUsuario) ? identificadorUsuario : null;
    }
}
