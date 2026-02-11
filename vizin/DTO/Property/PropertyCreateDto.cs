using System.ComponentModel.DataAnnotations;

namespace vizin.DTO.Property;

public class PropertyCreateDto
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    public string? Description { get; set; }

    [Required]
    public string FullAddress { get; set; }

    public bool Availability { get; set; }
    
    [Range(1, 100000)]
    public decimal DailyValue { get; set; }

    [Range(1, 50)]
    public int Capacity { get; set; }

    [Range(1, 3)]
    public int AccomodationType { get; set; }

    [Range(1, 4)]
    public int PropertyCategory { get; set; }
}