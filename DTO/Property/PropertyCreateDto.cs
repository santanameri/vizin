namespace vizin.DTO.Property;

public class PropertyCreateDto
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Endereco { get; set; }
    public bool? Disponivel { get; set; }
    public float? Diaria { get; set; }
    public int Capacidade { get; set; }
}