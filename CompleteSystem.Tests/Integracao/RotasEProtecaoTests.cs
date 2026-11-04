using System.Net;
using System.Text.RegularExpressions;
using CompleteSystem.Models;
using CompleteSystem.Services;
using CompleteSystem.Tests.Infraestrutura;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CompleteSystem.Tests.Integracao;

public class RotasEProtecaoTests
{
    private const string SenhaPadrao = "Senha@123";

    [Fact]
    public async Task RotaProtegida_sem_autenticacao_deve_redirecionar_para_login()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        var clienteHttp = fabrica.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var resposta = await clienteHttp.GetAsync("/Usuario");

        Assert.Equal(HttpStatusCode.Redirect, resposta.StatusCode);
        Assert.Contains("/Account/Login", resposta.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Login_do_administrador_deve_permitir_acessar_usuarios()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        var clienteHttp = await EntrarAsync(
            fabrica,
            FabricaAplicacaoWebTeste.EmailAdministrador,
            FabricaAplicacaoWebTeste.SenhaAdministrador);

        var resposta = await clienteHttp.GetAsync("/Usuario");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_com_senha_errada_nao_deve_autenticar()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        var clienteHttp = fabrica.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var tokenAntiforgery = await ObterTokenAntiforgeryAsync(clienteHttp);
        var respostaLogin = await clienteHttp.PostAsync("/Account/Login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Email"] = FabricaAplicacaoWebTeste.EmailAdministrador,
                ["Senha"] = "SenhaErrada@123",
                ["__RequestVerificationToken"] = tokenAntiforgery
            }));

        var respostaRotaProtegida = await clienteHttp.GetAsync("/Usuario");

        Assert.Equal(HttpStatusCode.OK, respostaLogin.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, respostaRotaProtegida.StatusCode);
    }

    [Fact]
    public async Task Cliente_pode_comprar_e_ver_produtos_mas_nao_pode_gerenciar_usuarios()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        await fabrica.CriarUsuarioAsync("Cliente", "cliente@teste.com", PerfisAcesso.Cliente, SenhaPadrao);
        var clienteHttp = await EntrarAsync(fabrica, "cliente@teste.com", SenhaPadrao);

        var respostaProdutos = await clienteHttp.GetAsync("/Produto");
        var respostaCompras = await clienteHttp.GetAsync("/Compra");
        var respostaUsuarios = await clienteHttp.GetAsync("/Usuario");
        var respostaLogs = await clienteHttp.GetAsync("/RegistroAcesso");

        Assert.Equal(HttpStatusCode.OK, respostaProdutos.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respostaCompras.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, respostaUsuarios.StatusCode);
        Assert.Contains("/Account/AccessDenied", respostaUsuarios.Headers.Location!.ToString());
        Assert.Equal(HttpStatusCode.Redirect, respostaLogs.StatusCode);
    }

    [Fact]
    public async Task Cliente_nao_pode_acessar_cadastro_de_categoria()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        await fabrica.CriarUsuarioAsync("Cliente", "cliente@teste.com", PerfisAcesso.Cliente, SenhaPadrao);
        var clienteHttp = await EntrarAsync(fabrica, "cliente@teste.com", SenhaPadrao);

        var respostaListagem = await clienteHttp.GetAsync("/Categoria");
        var respostaCadastro = await clienteHttp.GetAsync("/Categoria/Create");

        Assert.Equal(HttpStatusCode.OK, respostaListagem.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, respostaCadastro.StatusCode);
        Assert.Contains("/Account/AccessDenied", respostaCadastro.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Gerente_pode_gerenciar_categorias_e_regras_mas_nao_usuarios()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        await fabrica.CriarUsuarioAsync("Gerente", "gerente@teste.com", PerfisAcesso.Gerente, SenhaPadrao);
        var clienteHttp = await EntrarAsync(fabrica, "gerente@teste.com", SenhaPadrao);

        var respostaCadastroCategoria = await clienteHttp.GetAsync("/Categoria/Create");
        var respostaRegras = await clienteHttp.GetAsync("/RegraCategoria");
        var respostaUsuarios = await clienteHttp.GetAsync("/Usuario");

        Assert.Equal(HttpStatusCode.OK, respostaCadastroCategoria.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respostaRegras.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, respostaUsuarios.StatusCode);
    }

    [Fact]
    public async Task Proprietario_pode_cadastrar_produto_mas_nao_pode_gerenciar_usuarios()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        await fabrica.CriarUsuarioAsync("Proprietário", "dono@teste.com", PerfisAcesso.Proprietario, SenhaPadrao);
        var clienteHttp = await EntrarAsync(fabrica, "dono@teste.com", SenhaPadrao);

        var respostaCadastroProduto = await clienteHttp.GetAsync("/Produto/Create");
        var respostaUsuarios = await clienteHttp.GetAsync("/Usuario");

        Assert.Equal(HttpStatusCode.OK, respostaCadastroProduto.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, respostaUsuarios.StatusCode);
    }

    [Fact]
    public async Task Administrador_pode_acessar_logs_de_acesso()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        var clienteHttp = await EntrarAsync(
            fabrica,
            FabricaAplicacaoWebTeste.EmailAdministrador,
            FabricaAplicacaoWebTeste.SenhaAdministrador);

        var resposta = await clienteHttp.GetAsync("/RegistroAcesso");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_e_requisicoes_devem_ser_gravados_nos_logs()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        var clienteHttp = await EntrarAsync(
            fabrica,
            FabricaAplicacaoWebTeste.EmailAdministrador,
            FabricaAplicacaoWebTeste.SenhaAdministrador);

        await clienteHttp.GetAsync("/Produto");

        using var escopo = fabrica.Services.CreateScope();
        var servicoRegistroAcesso = escopo.ServiceProvider.GetRequiredService<IRegistroAcessoService>();
        var registros = await servicoRegistroAcesso.ListarRegistrosAsync(null, null, null, null);

        Assert.Contains(registros, registro =>
            registro.TipoAcao == nameof(TipoAcaoRegistro.LoginBemSucedido));
        Assert.Contains(registros, registro =>
            registro.TipoAcao == nameof(TipoAcaoRegistro.Requisicao) && registro.Caminho == "/Produto");
    }

    [Fact]
    public async Task Login_com_senha_errada_deve_ser_registrado_como_falha()
    {
        using var fabrica = new FabricaAplicacaoWebTeste();
        var clienteHttp = fabrica.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var tokenAntiforgery = await ObterTokenAntiforgeryAsync(clienteHttp);
        await clienteHttp.PostAsync("/Account/Login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Email"] = "ninguem@teste.com",
                ["Senha"] = "SenhaErrada@123",
                ["__RequestVerificationToken"] = tokenAntiforgery
            }));

        using var escopo = fabrica.Services.CreateScope();
        var servicoRegistroAcesso = escopo.ServiceProvider.GetRequiredService<IRegistroAcessoService>();
        var registros = await servicoRegistroAcesso.ListarRegistrosAsync("ninguem@teste.com", null, null, null);

        Assert.Contains(registros, registro =>
            registro.TipoAcao == nameof(TipoAcaoRegistro.LoginFalho));
    }

    private static async Task<HttpClient> EntrarAsync(
        FabricaAplicacaoWebTeste fabrica,
        string email,
        string senha)
    {
        var clienteHttp = fabrica.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var tokenAntiforgery = await ObterTokenAntiforgeryAsync(clienteHttp);

        var respostaLogin = await clienteHttp.PostAsync("/Account/Login", new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Senha"] = senha,
                ["LembrarMe"] = "false",
                ["__RequestVerificationToken"] = tokenAntiforgery
            }));

        Assert.Equal(HttpStatusCode.Redirect, respostaLogin.StatusCode);

        return clienteHttp;
    }

    private static async Task<string> ObterTokenAntiforgeryAsync(HttpClient clienteHttp)
    {
        var htmlDaPagina = await clienteHttp.GetStringAsync("/Account/Login");

        var correspondencia = Regex.Match(
            htmlDaPagina,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");

        Assert.True(correspondencia.Success, "Token antiforgery não encontrado na página de login.");
        return correspondencia.Groups[1].Value;
    }
}
