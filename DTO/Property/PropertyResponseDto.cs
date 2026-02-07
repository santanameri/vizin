namespace vizin.DTO.Property;

public class PropertyResponseDto
{
    public string Titulo { get; set; }
    public string? Description { get; set; }
    public string FullAddress { get; set; }
    public bool IsAvailable { get; set; }
    public float Diaria { get; set; }
    public int Capacity { get; set; }
    public int TipoDeAcomodacao { get; set; }
    public int Categoria { get; set; }
}