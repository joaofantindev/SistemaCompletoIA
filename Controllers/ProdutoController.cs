using CompleteSystem.Models;
using CompleteSystem.Models.DTOs;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CompleteSystem.Controllers;

[Authorize]
public class ProdutoController : Controller
{
    private readonly IProdutoService _servicoProduto;
    private readonly ICategoriaService _servicoCategoria;
    private readonly UserManager<Usuario> _gerenciadorUsuarios;

    public ProdutoController(
        IProdutoService servicoProduto,
        ICategoriaService servicoCategoria,
        UserManager<Usuario> gerenciadorUsuarios)
    {
        _servicoProduto = servicoProduto;
        _servicoCategoria = servicoCategoria;
        _gerenciadorUsuarios = gerenciadorUsuarios;
    }

    public async Task<IActionResult> Index()
    {
        var produtos = await _servicoProduto.ListarProdutosAsync();
        return View(produtos);
    }

    public async Task<IActionResult> Details(int identificador)
    {
        var produto = await _servicoProduto.ObterProdutoPorIdentificadorAsync(identificador);

        if (produto is null)
        {
            return NotFound();
        }

        return View(produto);
    }

    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Proprietario)]
    public async Task<IActionResult> Create()
    {
        await CarregarCategoriasDisponiveisAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Proprietario)]
    public async Task<IActionResult> Create(ProdutoCreateDTO dadosCadastroProduto)
    {
        var identificadorProprietario = ObterIdentificadorUsuarioAtual();

        if (identificadorProprietario is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosCadastroProduto);
        }

        try
        {
            await _servicoProduto.CadastrarProdutoAsync(dadosCadastroProduto, identificadorProprietario.Value);
            TempData["Sucesso"] = "Produto cadastrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosCadastroProduto);
        }
    }

    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Proprietario)]
    public async Task<IActionResult> Edit(int identificador)
    {
        var produto = await _servicoProduto.ObterProdutoPorIdentificadorAsync(identificador);

        if (produto is null)
        {
            return NotFound();
        }

        await CarregarCategoriasDisponiveisAsync();
        return View(produto.ToUpdateDTO());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Proprietario)]
    public async Task<IActionResult> Edit(int identificador, ProdutoUpdateDTO dadosAtualizacaoProduto)
    {
        if (identificador != dadosAtualizacaoProduto.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosAtualizacaoProduto);
        }

        try
        {
            var produtoAtualizado = await _servicoProduto.AtualizarProdutoAsync(dadosAtualizacaoProduto);

            if (!produtoAtualizado)
            {
                return NotFound();
            }

            TempData["Sucesso"] = "Produto atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraDeNegocioException excecao)
        {
            ModelState.AddModelError(string.Empty, excecao.Message);
            await CarregarCategoriasDisponiveisAsync();
            return View(dadosAtualizacaoProduto);
        }
    }

    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Proprietario)]
    public async Task<IActionResult> Delete(int identificador)
    {
        var produto = await _servicoProduto.ObterProdutoPorIdentificadorAsync(identificador);

        if (produto is null)
        {
            return NotFound();
        }

        return View(produto);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = PerfisAcesso.Administrador + "," + PerfisAcesso.Proprietario)]
    public async Task<IActionResult> DeleteConfirmed(int identificador)
    {
        var produtoRemovido = await _servicoProduto.RemoverProdutoAsync(identificador);

        if (!produtoRemovido)
        {
            return NotFound();
        }

        TempData["Sucesso"] = "Produto removido.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CarregarCategoriasDisponiveisAsync()
    {
        var categorias = await _servicoCategoria.ListarCategoriasAsync();
        ViewBag.CategoriasDisponiveis = categorias.Where(categoria => categoria.Ativa).ToList();
    }

    private int? ObterIdentificadorUsuarioAtual()
    {
        var identificadorTexto = _gerenciadorUsuarios.GetUserId(User);
        return int.TryParse(identificadorTexto, out var identificadorUsuario) ? identificadorUsuario : null;
    }
}
