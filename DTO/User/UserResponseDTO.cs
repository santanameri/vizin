using System.ComponentModel.DataAnnotations;

namespace vizin.DTO.User;
public class UserResponseDTO
{
    public Guid Id {get;set;}
    public string Email {get;set;}
    public string Name {get;set;}
    public DateTime? CreatedAt {get;set;}
}

public class CreateUserRequestDTO
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password deve ter no mínimo 6 caracteres")]
    public string Password { get; set; }

    [Required(ErrorMessage = "O tipo de usuário é obrigatório")]
    [Range(1, 2, ErrorMessage = "Tipo de usuário inválido. Use 1 para Anfitrião ou 2 para Hóspede")]
    public UserType Type { get; set; }
}


