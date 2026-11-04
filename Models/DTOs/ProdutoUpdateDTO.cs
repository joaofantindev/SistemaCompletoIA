using System.ComponentModel.DataAnnotations;

namespace CompleteSystem.Models.DTOs;

/// <summary>
/// Dados de entrada para a edição de produto.
/// </summary>
public class ProdutoUpdateDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A marca é obrigatória.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "A marca deve ter entre 1 e 100 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999.99", ErrorMessage = "Informe um preço maior que zero.")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida.")]
    public int? CategoriaId { get; set; }
}
