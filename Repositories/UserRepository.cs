using Data;
using Entities;
using Enums;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public sealed class UserRepository(
    AppDbContext appDbContext
)
{
    private readonly AppDbContext _appDbContext = appDbContext;

    public async Task<User> CreateAsync(User user)
    {
        await _appDbContext.Users.AddAsync(user);
        await _appDbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User?> FindByIdAsync(Guid id)
    {
        return await _appDbContext.Users.FindAsync(id);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _appDbContext.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<List<User>> FindByBloodTypeAsync(BloodType bloodType)
    {
        return await _appDbContext.Users
            .Where(user => user.BloodType == bloodType)
            .ToListAsync();
    }

    public async Task<List<User>> FindByCityAsync(string city)
    {
        return await _appDbContext.Users
            .Where(user => user.City == city)
            .ToListAsync();
    }

    public async Task<List<User>> FindByDistrictAsync(string district)
    {
        return await _appDbContext.Users
            .Where(user => user.District == district)
            .ToListAsync();
    }

    public async Task<List<User>> GetDonorsAsync()
    {
        return await _appDbContext.Users
            .Where(user => (user.Roles & (1 << (int)Role.Donor)) == (1 << (int)Role.Donor))
            .ToListAsync();
    }

    public async Task<List<User>> GetBeneficiariesAsync()
    {
        return await _appDbContext.Users
            .Where(user => (user.Roles & (1 << (int)Role.Beneficiary)) == (1 << (int)Role.Beneficiary))
            .ToListAsync();
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _appDbContext.Users.ToListAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _appDbContext.Users.Update(user);
        await _appDbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _appDbContext.Users.Remove(user);
        await _appDbContext.SaveChangesAsync();
    }
}
