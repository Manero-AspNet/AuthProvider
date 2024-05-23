using AuthProvider.Data.Contexts;
using AuthProvider.Data.Entities;
using AuthProvider.Models;
using AuthProvider.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AuthProvider.Functions;

public class SignUp
{
    private readonly ILogger<SignUp> _logger;
    private readonly ISignUpService _signUpService;
    private readonly ServiceBusClient _client;
    private ServiceBusSender _verificationSender;
    private ServiceBusSender _accountSender;

    public SignUp(ILogger<SignUp> logger, ISignUpService signUpService, ServiceBusClient client)
    {
        _logger = logger;
        _signUpService = signUpService;
        _client = client;
        _verificationSender = _client.CreateSender("verification_request");
        _accountSender = _client.CreateSender("create_account_request");
    }

    [Function("SignUp")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        try
        {
            var signUpRequest = await _signUpService.UnpackSignUpRequest(req);
            if (signUpRequest != null)
            {
                var exists = await _signUpService.UserExists(signUpRequest);
                if (!exists)
                {
                    var userEntity = await _signUpService.CreateUser(signUpRequest);
                    if (userEntity != null)
                    {
                        var vr = _signUpService.GenerateVerificationRequest(userEntity);
                        var ar = await _signUpService.GenerateAccountRequest(userEntity);
                        if (vr != null && ar != null!)
                        {
                            var verifictionPayload = _signUpService.GenerateServiceBusMessage(vr);
                            var accountPayload = _signUpService.GenerateServiceBusMessage(ar);

                            if(!string.IsNullOrEmpty(verifictionPayload) && !string.IsNullOrEmpty(accountPayload))
                            {
                                var verificationMessage = new ServiceBusMessage(verifictionPayload)
                                {
                                    ContentType= "application/json",
                                };

                                var accountMessage = new ServiceBusMessage(accountPayload)
                                {
                                    ContentType = "application/json",
                                };
                                await _verificationSender.SendMessageAsync(verificationMessage);
                                await _accountSender.SendMessageAsync(accountMessage);

                                return new CreatedResult();
                            }
                        }
                    }
                }
                else
                {
                    return new ConflictResult();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SignUp.Run() :: {ex.Message}");
        }
        return new BadRequestResult();
    }
}
