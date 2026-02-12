using System.ComponentModel.DataAnnotations;

namespace vizin.DTO.Property;

public class PropertyCreateDto
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(100)]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Endereço é obrigatório")]
    public string FullAddress { get; set; }

    public bool Availability { get; set; }
    
    [Required(ErrorMessage = "Valor da diária é obrigatório")]
    [Range(1, 100000)]
    public decimal DailyValue { get; set; }

    [Required(ErrorMessage = "Capacidade é obrigatória")]
    [Range(1, 50, ErrorMessage = "A capacidade deve estar entre 1 e 50")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Tipo da acomodação é obrigatório")]
    [Range(1, 3)]
    public int AccomodationType { get; set; }

    [Required(ErrorMessage = "Categoria da propriedade é obrigatório")]
    [Range(1, 4)]
    public int PropertyCategory { get; set; }
}