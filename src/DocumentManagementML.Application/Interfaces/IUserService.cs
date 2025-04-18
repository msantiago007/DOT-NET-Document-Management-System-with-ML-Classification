// IUserService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Application.DTOs;

namespace DocumentManagementML.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<IEnumerable<UserDto>> GetUsersAsync(int skip = 0, int limit = 100);
        Task<UserDto> CreateUserAsync(UserCreateDto userDto);
        Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto);
        Task DeactivateUserAsync(Guid id);
        Task<UserDto?> AuthenticateAsync(string username, string password);
        
        // Methods from the other interface
        Task<bool> UserExistsAsync(Guid userId);
        Task<string?> GetUserNameAsync(Guid userId);
    }
}