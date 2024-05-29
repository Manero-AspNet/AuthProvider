using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AuthProvider.Functions
{
    public class SignInFunctionRedirect
    {
      

        [Function("SignInFunctionRedirect")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route ="signin-facebook")] HttpRequest req)
        {
            string appId = "437966342173321";
            string redirectUri = "http://localhost:7234/signin-facebook-callback";
            string state = Guid.NewGuid().ToString();

            var uri = $"https://www.facebook.com/v20.0/dialog/oauth?client_id={appId}&redirect_uri={redirectUri}&state={state}&scope=email";
            return new RedirectResult(uri);
        }
    }
}
