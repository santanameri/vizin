namespace vizin.DTO.ReviewRequestDto;

using System.ComponentModel.DataAnnotations;

public class ReviewRequestDto
{
    [Required(ErrorMessage = "A nota é obrigatória.")]
    [Range(1, 5, ErrorMessage = "A nota deve ser entre 1 e 5 estrelas.")]
    public int Stars { get; set; }

    [Required(ErrorMessage = "O comentário não pode estar vazio.")]
    [StringLength(500, ErrorMessage = "O comentário pode ter no máximo 500 caracteres.")]
    public string Comment { get; set; } = string.Empty;
}