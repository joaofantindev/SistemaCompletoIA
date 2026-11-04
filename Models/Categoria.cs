namespace CompleteSystem.Models;

/// <summary>
/// Categoria de produtos. O gerente é responsável por criar, editar e remover.
/// </summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public ICollection<Produto> Produtos { get; set; } = new List<Produto>();
    public ICollection<RegraCategoria> Regras { get; set; } = new List<RegraCategoria>();
}
