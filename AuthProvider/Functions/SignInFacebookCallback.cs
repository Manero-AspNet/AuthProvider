using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace AuthProvider.Functions;

public class SignInFacebookCallback
{
    private readonly ILogger<SignInFacebookCallback> _logger;
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public SignInFacebookCallback(ILogger<SignInFacebookCallback> logger)
    {
        _logger = logger;
        _client = new ServiceBusClient(Environment.GetEnvironmentVariable("ServiceBus"));
        _sender = _client.CreateSender("facebook_response");
    }

    [Function("SignInFacebookCallback")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "signin-facebook-callback")] HttpRequest req)
    {
        try
        {
            string appId = "437966342173321";
            string clientSecret = "ac0c5a6070da45abb7fc5cf246c06a8a";
            string redirectUri = "http://localhost:7234/signin-facebook-callback";
            string code = req.Query["code"]!;
            string state = req.Query["state"]!;

            using var httpClient = new HttpClient();
            var tokenResponse = await httpClient.GetStringAsync($"https://graph.facebook.com/v20.0/oauth/access_token?client_id={appId}&redirect_uri={redirectUri}&client_secret={clientSecret}&code={code}&state={state}");
            var faceBookToken = JsonConvert.DeserializeObject<FacebookTokenResponse>(tokenResponse);


            var userResponse = await httpClient.GetStringAsync($"https://graph.facebook.com/v20.0/me?fields=id,name,email&access_token={faceBookToken!.AccessToken}");
            var user = JsonConvert.DeserializeObject<FacebookUserResponse>(userResponse);


            var claims = new List<Claim>
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
            if (!string.IsNullOrEmpty(accessToken))
            {
                var facebookMessage = new ServiceBusMessage(accessToken);

                await _sender.SendMessageAsync(facebookMessage);

                return new RedirectResult("https://localhost:7020/home");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignInFacebookCallback.Run() :: {ex.Message}");
        }
        return new BadRequestResult();
    }

    public class FacebookTokenResponse
    {
        [JsonProperty("access_token")] 
        public string AccessToken { get; set; } = null!;
    }

    public class FacebookUserResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; } = null!;

        [JsonProperty("name")]
        public string Name { get; set; } = null!;

        [JsonProperty("email")]
        public string Email { get; set; } = null!;
    }
}
