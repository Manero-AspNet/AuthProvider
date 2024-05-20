using System.ComponentModel.DataAnnotations;

namespace AuthProvider.Models;

public class SignUpRequest
{
    [Required]
    public string FirstName { get; set; } = null!;


    [Required]
    public string LastName { get; set; } = null!;


    [Required]
    public string Email { get; set; } = null!;


    [Required]
    public string Password { get; set; } = null!;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = null!;
}
