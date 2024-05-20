using AuthProvider.Data.Entities;
using AuthProvider.Models;
using Microsoft.AspNetCore.Http;

namespace AuthProvider.Services
{
    public interface ISignUpService
    {
        Task<UserEntity> CreateUser(SignUpRequest request);
        string GenerateServiceBusMessage(VerificationRequest request);
        VerificationRequest GenerateVerificationRequest(UserEntity entity);
        Task<SignUpRequest> UnpackSignUpRequest(HttpRequest req);
        Task<bool> UserExists(SignUpRequest request);
    }
}