using System.ComponentModel.DataAnnotations;

namespace Controllers.Dtos;

public sealed record UpdateProfileRequest(
    Guid? SessionId,

    string? FirstName,

    string? LastName,

    [EmailAddress]
    string? Email,

    [MinLength(6), MaxLength(32)]
    string? Password,

    string? SocialSecurityNumber,

    string? Gender,

    string? BloodType,

    string? City,

    string? District,

    string? DonationDescription,

    string? DateOfBirth,

    string? PhoneNumber
);
