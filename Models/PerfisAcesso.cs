namespace CompleteSystem.Models;

/// <summary>
/// Nomes dos perfis (roles) usados pelo ASP.NET Core Identity.
/// Os valores são constantes para evitar texto solto espalhado pelo código.
/// </summary>
public static class PerfisAcesso
{
    public const string Administrador = "Admin";
    public const string Gerente = "Manager";
    public const string Proprietario = "Owner";
    public const string Cliente = "Client";

    public static readonly IReadOnlyList<string> Todos = new[]
    {
        Administrador,
        Gerente,
        Proprietario,
        Cliente
    };
}
