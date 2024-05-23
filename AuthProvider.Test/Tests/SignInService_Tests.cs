using AuthProvider.Models;
using AuthProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace AuthProvider.Test.Tests;

public class SignInService_Tests
{
    private readonly Mock<ILogger<SignInService>> _mockLogger;
    private readonly SignInService _service;

    public SignInService_Tests()
    {
        _mockLogger = new Mock<ILogger<SignInService>>();
        _service = new SignInService(_mockLogger.Object);
    }

    [Fact]
    public async Task UnpackSignInRequest_ShouldReturnSignInRequest_WhenValidRequest()
    {
        // Arrange
        var request = new SignInRequest { Email = "test@example.com", Password = "Password123" };
        var httpRequest = new Mock<HttpRequest>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(JsonConvert.SerializeObject(request));
        await writer.FlushAsync();
        stream.Position = 0;
        httpRequest.Setup(x => x.Body).Returns(stream);

        // Act
        var result = await _service.UnpackSignInRequest(httpRequest.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
        Assert.Equal(request.Password, result.Password);
    }

    [Fact]
    public async Task UnpackSignInRequest_ShouldReturnNull_WhenBodyIsEmpty()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var stream = new MemoryStream();
        httpRequest.Setup(x => x.Body).Returns(stream);

        // Act
        var result = await _service.UnpackSignInRequest(httpRequest.Object);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UnpackSignInRequest_ShouldReturnNull_WhenDeserializationFails()
    {
        // Arrange
        var httpRequest = new Mock<HttpRequest>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync("Invalid JSON");
        await writer.FlushAsync();
        stream.Position = 0;
        httpRequest.Setup(x => x.Body).Returns(stream);

        // Act
        var result = await _service.UnpackSignInRequest(httpRequest.Object);

        // Assert
        Assert.Null(result);
    }
}
