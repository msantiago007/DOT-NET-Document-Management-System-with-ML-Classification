/ IUserService.cs
namespace DocumentManagementML.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetUsersAsync(int skip = 0, int limit = 100);
        Task<UserDto> CreateUserAsync(UserCreateDto userDto);
        Task<UserDto> UpdateUserAsync(int id, UserUpdateDto userDto);
        Task DeactivateUserAsync(int id);
        Task<UserDto?> AuthenticateAsync(string username, string password);
    }
}