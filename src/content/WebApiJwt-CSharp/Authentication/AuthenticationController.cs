using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Company.WebApplication1.Authentication
{
 [Route("[controller]/[action]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationService authenticationService;

        public AuthenticationController(AuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Authenticate([FromBody] UserCredentials creds)
        {
            try
            {
                string token = await authenticationService.Authenticate(creds);
                return Ok(token);
            }
            catch (InvalidCredentialException)
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Validate()
        {
            await Task.CompletedTask;
            return Ok();
        }
    }
}