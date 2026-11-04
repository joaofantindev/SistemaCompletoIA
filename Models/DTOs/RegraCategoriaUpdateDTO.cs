using System.ComponentModel.DataAnnotations;

namespace CompleteSystem.Models.DTOs;

public class RegraCategoriaUpdateDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida.")]
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "A descrição da regra é obrigatória.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "A descrição deve ter entre 3 e 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "100", ErrorMessage = "O desconto deve estar entre 0 e 100.")]
    public decimal PercentualDesconto { get; set; }

    public bool Ativa { get; set; } = true;
}
