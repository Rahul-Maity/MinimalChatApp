using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using MinimalChatApp.DomainModel.Data;
using MinimalChatApp.DomainModel.Dtos.Outgoing;
using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Repository.Users;
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;


    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    

    public async Task<bool> CheckEmailExistAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);

    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<GetUserResDto>> GetUsersExceptCurrentUserAsync(Guid currentUserId)
    {
        return await _context.Users
            .Where(u => u.Id != currentUserId)
            .Select(u => new GetUserResDto
            {
                Id = u.Id,
                Name = u.FullName,
                Email = u.Email
            })
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
