namespace vizin.DTO.Property;

public class PropertyResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string FullAddress { get; set; }
    public bool Availability { get; set; }
    public decimal DailyValue { get; set; }
    public int Capacity { get; set; }
    public int AccomodationType { get; set; }
    public int PropertyCategory { get; set; }
}
