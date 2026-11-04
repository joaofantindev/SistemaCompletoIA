using CompleteSystem.Data;
using CompleteSystem.Middleware;
using CompleteSystem.Models;
using CompleteSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var construtorAplicacao = WebApplication.CreateBuilder(args);

construtorAplicacao.Services.AddControllersWithViews();

construtorAplicacao.Services.AddDbContext<AppDbContext>(opcoes =>
    opcoes.UseSqlite(
        construtorAplicacao.Configuration.GetConnectionString("DefaultConnection")));

construtorAplicacao.Services
    .AddIdentity<Usuario, IdentityRole<int>>(opcoes =>
    {
        opcoes.Password.RequiredLength = 8;
        opcoes.Password.RequireDigit = true;
        opcoes.Password.RequireUppercase = true;
        opcoes.Password.RequireLowercase = true;
        opcoes.Password.RequireNonAlphanumeric = true;
        opcoes.User.RequireUniqueEmail = true;
        opcoes.SignIn.RequireConfirmedAccount = false;
        opcoes.Lockout.MaxFailedAccessAttempts = 5;
        opcoes.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

construtorAplicacao.Services.ConfigureApplicationCookie(opcoes =>
{
    opcoes.LoginPath = "/Account/Login";
    opcoes.AccessDeniedPath = "/Account/AccessDenied";
    opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
    opcoes.SlidingExpiration = true;
});

construtorAplicacao.Services.AddScoped<IUsuarioService, UsuarioService>();
construtorAplicacao.Services.AddScoped<IProdutoService, ProdutoService>();
construtorAplicacao.Services.AddScoped<ICategoriaService, CategoriaService>();
construtorAplicacao.Services.AddScoped<IRegraCategoriaService, RegraCategoriaService>();
construtorAplicacao.Services.AddScoped<ICompraService, CompraService>();
construtorAplicacao.Services.AddScoped<IRegistroAcessoService, RegistroAcessoService>();

var aplicacao = construtorAplicacao.Build();

await InicializadorBancoDados.InicializarAsync(
    aplicacao.Services,
    aplicacao.Configuration,
    aplicacao.Logger);

if (!aplicacao.Environment.IsDevelopment() && !aplicacao.Environment.IsEnvironment("Testing"))
{
    aplicacao.UseExceptionHandler("/Home/Error");
    aplicacao.UseHsts();
}

if (!aplicacao.Environment.IsEnvironment("Testing"))
{
    aplicacao.UseHttpsRedirection();
}

aplicacao.UseRouting();

aplicacao.UseAuthentication();
aplicacao.UseMiddleware<RegistroRequisicaoMiddleware>();
aplicacao.UseAuthorization();

aplicacao.MapStaticAssets();

aplicacao.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

aplicacao.Run();

/// <summary>
/// Exposição necessária para os testes de integração com WebApplicationFactory.
/// </summary>
public partial class Program
{
}
