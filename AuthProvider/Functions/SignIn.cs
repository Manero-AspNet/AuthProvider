using AuthProvider.Data.Contexts;
using AuthProvider.Data.Entities;
using AuthProvider.Models;
using AuthProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthProvider.Functions;

public class SignIn(ILogger<SignIn> logger, SignInManager<UserEntity> signInManager, ISignInService signInService, DataContext context)
{
    private readonly ILogger<SignIn> _logger = logger;
    private readonly SignInManager<UserEntity> _signInManager = signInManager;
    private readonly ISignInService _signInService = signInService;
    private readonly DataContext _context = context;

    [Function("SignIn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var signInRequest = await _signInService.UnpackSignInRequest(req);
            if (signInRequest != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == signInRequest.Email);
                if (user != null)
                {
                    var signInResponseResult = new SignInResponseResult
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        Username = user.UserName!
                    };
                    return new OkObjectResult(signInResponseResult);
                }

                //var result = await _signInManager.PasswordSignInAsync(signInRequest.Email, signInRequest.Password, signInRequest.RememberMe, false);
                //if (result.Succeeded)
                //{
                //    return new OkResult();
                //}
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignIn.Run() :: {ex.Message}");
        }
        return new BadRequestResult();
    }


}
