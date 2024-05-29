using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using static AuthProvider.Functions.SignInFacebookCallback;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net.Http;
using Azure.Messaging.ServiceBus;

namespace AuthProvider.Functions;
    
public class SignInFacebook
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public SignInFacebook()
    {
        _client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBus"));
        _sender = _client.CreateSender("facebook_response");
    }

    [Function("SignInFacebook")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "signin-facebook/{action?}")] HttpRequest req, string? action)
    {
        string appId = "437966342173321";
        string clientSecret = "ac0c5a6070da45abb7fc5cf246c06a8a";
        string redirectUri = "http://localhost:7234/signin-facebook-callback";


        if (string.IsNullOrEmpty(action))
        {
            string state = Guid.NewGuid().ToString();
            var uri = $"https://www.facebook.com/v20.0/dialog/oauth?client_id={appId}&redirect_uri={redirectUri}&state={state}&scope=email";
            return new RedirectResult(uri);
        }
        else if (action == "callback")
        {
            string code = req.Query["code"]!;
            string state = req.Query["state"]!;

            using var http = new HttpClient();
            var tokenResponse = await http.GetStringAsync($"https://graph.facebook.com/v20.0/oauth/access_token?client_id={appId}&redirect_uri={redirectUri}&client_secret={clientSecret}&code={code}&state={state}");
            var faceBookToken = JsonConvert.DeserializeObject<FacebookTokenResponse>(tokenResponse);

            var userResponse = await http.GetStringAsync($"https://graph.facebook.com/v20.0/me?fields=id,name,email&access_token={faceBookToken!.AccessToken}");
            var user = JsonConvert.DeserializeObject<FacebookUserResponse>(userResponse);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user!.Id),
                new Claim(ClaimTypes.Name, user!.Name),
                new Claim(ClaimTypes.Email, user!.Email),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ac0c5a6070da45abb7fc5cf246c06a8a123123"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "http://localhost:7234",
                audience: "Manero",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            var facebookMessage = new ServiceBusMessage(accessToken);

            await _sender.SendMessageAsync(facebookMessage);

            return new OkResult();
        }

        return new UnauthorizedResult();
    }
}
