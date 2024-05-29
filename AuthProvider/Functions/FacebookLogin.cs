using AuthProvider.Data.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AuthProvider.Functions;

public class FacebookLogin
{
    private readonly ILogger<FacebookLogin> _logger;
    private readonly SignInManager<DataContext> _signInManager;

    public FacebookLogin(ILogger<FacebookLogin> logger)
    {
        _logger = logger;
    }

    [Function("FacebookLogin")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "FacebookLogin")] HttpRequest req)
    {
        var provider = "Facebook";

        var redirectUrl = "/Callback";
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);

    }
}
