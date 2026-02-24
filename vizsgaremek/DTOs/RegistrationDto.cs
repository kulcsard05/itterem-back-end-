using System.ComponentModel.DataAnnotations;

namespace vizsgaremek.DTOs;

public class RegistrationDto
{
 [Required]
 public string TeljesNev { get; set; } = null!;

 [Required]
 [EmailAddress]
 public string Email { get; set; } = null!;

 [Required]
 public string Telefonszam { get; set; } = null!;

 // Client sends plain password; server generates Salt and stores Hash.
 [Required]
 public string Jelszo { get; set; } = null!;
}
