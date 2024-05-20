using AuthProvider.Functions;
using AuthProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthProvider.Services;

public class SignInService(ILogger<SignInService> logger) : ISignInService
{
    private readonly ILogger<SignInService> _logger = logger;

    public async Task<SignInRequest> UnpackSignInRequest(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (body != null)
            {
                var signInRequest = JsonConvert.DeserializeObject<SignInRequest>(body);
                if (signInRequest != null)
                {
                    return signInRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignIn.UnpackSignInRequest() :: {ex.Message}");
        }
        return null!;
    }
}
