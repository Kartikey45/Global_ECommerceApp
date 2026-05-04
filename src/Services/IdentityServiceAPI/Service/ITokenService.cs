using IdentityServiceAPI.Models;

namespace IdentityServiceAPI.Service
{
    public interface ITokenService
    {
        string GenerateToken(User user, IList<string> roles, IList<string> permissions);
    }
}
