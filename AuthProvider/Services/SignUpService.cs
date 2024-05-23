using AuthProvider.Data.Contexts;
using AuthProvider.Data.Entities;
using AuthProvider.Functions;
using AuthProvider.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthProvider.Services;

public class SignUpService(ILogger<SignUpService> logger, DataContext context, UserManager<UserEntity> userManager) : ISignUpService
{
    private readonly ILogger<SignUpService> _logger = logger;
    private readonly DataContext _context = context;
    private readonly UserManager<UserEntity> _userManager = userManager;

    public async Task<SignUpRequest> UnpackSignUpRequest(HttpRequest req)
    {
        try
        {
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            if (!string.IsNullOrEmpty(body))
            {
                var signUpRequest = JsonConvert.DeserializeObject<SignUpRequest>(body);
                if (signUpRequest != null)
                {
                    return signUpRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.UnpackSignUpRequest() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> UserExists(SignUpRequest request)
    {
        try
        {
            var result = await _context.Users.AnyAsync(x => x.Email == request.Email);
            if (result)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.UserExists() :: {ex.Message}");
        }
        return false;
    }

    public async Task<UserEntity> CreateUser(SignUpRequest request)
    {
        try
        {
            var userEntity = new UserEntity
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email
            };

            if (userEntity != null)
            {
                var result = await _userManager.CreateAsync(userEntity, request.Password);

                if (result.Succeeded)
                {
                    return userEntity;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.CreateUser() :: {ex.Message}");
        }
        return null!;
    }

    public VerificationRequest GenerateVerificationRequest(UserEntity entity)
    {
        try
        {
            if (entity != null)
            {
                var verificationRequest = new VerificationRequest
                {
                    Email = entity.Email!,
                };
                if (verificationRequest != null)
                {
                    return verificationRequest;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.GenerateVerificationRequest() :: {ex.Message}");
        }
        return null!;

    }

    public async Task<AccountRequest> GenerateAccountRequest(UserEntity entity)
    {
        try
        {
            if (entity != null)
            {
                var userEntity = await _userManager.FindByEmailAsync(entity.Email!);

                if (userEntity != null)
                {
                    var accountRequest = new AccountRequest
                    {
                        UserId = userEntity.Id,
                        FirstName = userEntity.FirstName,
                        LastName = userEntity.LastName,
                        Email = userEntity.Email!,
                    };
                    if (accountRequest != null)
                    {
                        return accountRequest;
                    }
                }


            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.GenerateVerificationRequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusMessage(VerificationRequest request)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(request);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.GenerateServiceBusMessage() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusMessage(AccountRequest request)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(request);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUpService.GenerateServiceBusMessage() :: {ex.Message}");
        }
        return null!;
    }
}
