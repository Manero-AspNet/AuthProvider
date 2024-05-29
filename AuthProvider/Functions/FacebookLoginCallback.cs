using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AuthProvider.Functions;

public class FacebookLoginCallback
{
    private readonly ILogger<FacebookLoginCallback> _logger;

    public FacebookLoginCallback(ILogger<FacebookLoginCallback> logger)
    {
        _logger = logger;
    }

    [Function("FacebookLoginCallback")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Callback")] HttpRequest req)
    {
        var remoteError = await UnpackRequest(req);
        if (remoteError != null)
        {
            return new RedirectResult("/signin");
        }

        return new BadRequestResult();

    }
    public async Task<string> UnpackRequest(HttpRequest req)
    {
        var request = await new StreamReader(req.Body).ReadToEndAsync();
        if(!string.IsNullOrWhiteSpace(request))
        {
            return request;
        }
        return null!;
    }
}
