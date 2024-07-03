using System.ComponentModel.DataAnnotations;
using Enums;

namespace Controllers.Dtos.Register;

public sealed record RegisterRequest(
    [Required]
    string FirstName,

    [Required]
    string LastName,

    [Required, EmailAddress]
    string Email,

    [Required, MinLength(6), MaxLength(32)]
    string Password,

    [Required]
    string SocialSecurityNumber,

    [Required]
    string Gender,

    [Required]
    string BloodType,

    [Required]
    string City,

    [Required]
    string District,

    [Required]
    string DateOfBirth,

    [Required]
    string PhoneNumber
);
