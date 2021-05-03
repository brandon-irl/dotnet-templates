using System;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Company.WebApplication1.Authentication
{
    public class UserService
    {

        public async Task ValidateCredentials(UserCredentials UserCredentials)
        {
            if(UserCredentials == null || UserCredentials.Username == null)
                throw new InvalidCredentialException();

            // TODO: validate creds against database
            await Task.CompletedTask;
        }

    }
}