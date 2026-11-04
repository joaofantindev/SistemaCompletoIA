using System.ComponentModel.DataAnnotations;

namespace CompleteSystem.Models.DTOs;

public class CompraCreateDTO
{
    [Required(ErrorMessage = "Selecione um produto.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um produto válido.")]
    public int? ProdutoId { get; set; }

    [Range(1, 9999, ErrorMessage = "A quantidade deve estar entre 1 e 9999.")]
    public int Quantidade { get; set; } = 1;
}
