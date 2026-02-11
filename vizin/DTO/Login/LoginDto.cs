using System.ComponentModel.DataAnnotations;

namespace vizin.DTO.Login;

public class LoginDto
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; }
}