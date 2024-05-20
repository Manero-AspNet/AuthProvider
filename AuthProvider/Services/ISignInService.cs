using AuthProvider.Models;
using Microsoft.AspNetCore.Http;

namespace AuthProvider.Services
{
    public interface ISignInService
    {
        Task<SignInRequest> UnpackSignInRequest(HttpRequest req);
    }
} 