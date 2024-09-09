using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MinimalChatApp.DomainModel.Dtos.Outgoing;
using MinimalChatApp.DomainModel.Models;

namespace MinimalChatApp.Repository.Users;
public interface IUserRepository
{
    Task<bool> CheckEmailExistAsync(string email);
    Task AddUserAsync(User user);
    Task SaveChangesAsync();
    Task<User> GetUserByEmailAsync(string email);

    Task<List<GetUserResDto>> GetUsersExceptCurrentUserAsync(Guid currentUserId);
}
