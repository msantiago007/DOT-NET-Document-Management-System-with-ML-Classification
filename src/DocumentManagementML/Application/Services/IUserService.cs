using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManagementML.Application.Services
{
    public interface IUserService
    {
        Task<bool> UserExistsAsync(Guid userId);
        Task<string?> GetUserNameAsync(Guid userId);
    }
} 