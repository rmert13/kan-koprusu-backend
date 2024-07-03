using Enums;

namespace Entities;

public sealed class User
{
    // Primary Key
    public Guid Id { get; set; }

    // Properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string SocialSecurityNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public BloodType BloodType { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? DonationDescription { get; set; }
    public uint Roles { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public static class UIntExtensions
{
    public static List<Role> ToRoleList(this uint number)
    {
        List<Role> roles = [];

        for (int i = 0; i < sizeof(uint) * 8; i++)
        {
            if ((number & (1 << i)) != 0)
            {
                roles.Add((Role)i);
            }
        }

        return roles;
    }
}
