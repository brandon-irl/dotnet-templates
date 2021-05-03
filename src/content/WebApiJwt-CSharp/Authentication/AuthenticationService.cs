using System.Threading.Tasks;

namespace Company.WebApplication1.Authentication
{
    public class AuthenticationService
    {
        readonly UserService _userService;
        readonly TokenService _tokenService;
        public AuthenticationService(UserService userService, TokenService tokenService)
        {
            _userService = userService;
            _tokenService = tokenService;
        }

        public async Task<string> Authenticate(UserCredentials UserCredentials)
        {
            await _userService.ValidateCredentials(UserCredentials);
            return _tokenService.GetToken();
        }
    }
}