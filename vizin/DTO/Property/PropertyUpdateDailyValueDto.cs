using System.ComponentModel.DataAnnotations;

namespace vizin.DTO.Property;

public class PropertyUpdateDailyValueDto
{
    [Required(ErrorMessage = "Daily value is required")]
    [Range(50, 100000, ErrorMessage = "Daily value must be between 50 and 100000")]
    public decimal? DailyValue { get; set; }
}
