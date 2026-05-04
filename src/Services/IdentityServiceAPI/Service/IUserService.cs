using IdentityServiceAPI.Models;

namespace IdentityServiceAPI.Service
{
    public interface IUserService
    {
        string GetUserId();
        bool IsAuthenticated();
        Task<UserCreationResultDto> AddUserAsync(SignUpUserDto signUpUser);
        Task<AuthenticationResultDto> AuthenticateUserAsync(SignInUserDto loginDto);
    }
}