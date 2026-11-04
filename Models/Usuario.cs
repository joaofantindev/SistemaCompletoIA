using Microsoft.AspNetCore.Identity;

namespace CompleteSystem.Models;

/// <summary>
/// Usuário do sistema. Herda de IdentityUser para que o ASP.NET Core Identity
/// cuide de senha, normalização de e-mail, bloqueio e perfis de acesso.
/// </summary>
public class Usuario : IdentityUser<int>
{
    public string Nome { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Produto> ProdutosCadastrados { get; set; } = new List<Produto>();
    public ICollection<Compra> ComprasRealizadas { get; set; } = new List<Compra>();
}
