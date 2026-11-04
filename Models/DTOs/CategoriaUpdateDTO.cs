using System.ComponentModel.DataAnnotations;

namespace CompleteSystem.Models.DTOs;

public class CategoriaUpdateDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    public bool Ativa { get; set; } = true;
}
