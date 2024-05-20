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

public class SignUp(ILogger<SignUp> logger, ServiceBusClient serviceBusClient, ISignUpService signUpService)
{
    private readonly ILogger<SignUp> _logger = logger;
    private readonly ServiceBusClient _serviceBusClient = serviceBusClient;
    private readonly ISignUpService _signUpService = signUpService;

    [Function("SignUp")]
    [ServiceBusOutput("verification_request", Connection = "ServiceBus")]
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
                        if (vr != null)
                        {
                            var payload = _signUpService.GenerateServiceBusMessage(vr);

                            if(!string.IsNullOrEmpty(payload))
                            {
                                var sender = _serviceBusClient.CreateSender("verification_request");
                                await sender.SendMessageAsync(new ServiceBusMessage(payload)
                                {
                                    ContentType= "application/json",
                                });
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
