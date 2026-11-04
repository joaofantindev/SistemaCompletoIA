namespace CompleteSystem.Models;

/// <summary>
/// Produto cadastrado por um usuário com perfil Owner (proprietário) e
/// sempre vinculado a uma categoria de produtos.
/// </summary>
public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public decimal Preco { get; set; }

    public int? CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? ProprietarioId { get; set; }
    public Usuario? Proprietario { get; set; }

    public DateTime CadastradoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
}
