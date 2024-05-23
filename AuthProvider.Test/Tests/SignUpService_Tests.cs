using AuthProvider.Data.Contexts;
using AuthProvider.Data.Entities;
using AuthProvider.Models;
using AuthProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace AuthProvider.Test.Tests;

public class SignUpService_Tests
{
    private readonly Mock<ILogger<SignUpService>> _mockLogger;
    private readonly Mock<DataContext> _mockContext;
    private readonly Mock<UserManager<UserEntity>> _mockUserManager;
    private readonly SignUpService _service;

    public SignUpService_Tests()
    {
        _mockLogger = new Mock<ILogger<SignUpService>>();
        _mockContext = new Mock<DataContext>();
        _mockUserManager = new Mock<UserManager<UserEntity>>(
            new Mock<IUserStore<UserEntity>>().Object,
            null, null, null, null, null, null, null, null
        );
        _service = new SignUpService(_mockLogger.Object, _mockContext.Object, _mockUserManager.Object);
    }

    [Fact]
    public async Task UnpackSignUpRequest_ShouldReturnSignUpRequest_WhenValidRequest()
    {
        // Arrange
        var request = new SignUpRequest { Email = "test@example.com", FirstName = "Test", LastName = "User", Password = "Password123" };
        var httpRequest = new Mock<HttpRequest>();
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        await writer.WriteAsync(JsonConvert.SerializeObject(request));
        await writer.FlushAsync();
        stream.Position = 0;
        httpRequest.Setup(x => x.Body).Returns(stream);

        // Act
        var result = await _service.UnpackSignUpRequest(httpRequest.Object);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
    }

    //[Fact]
    //public async Task UserExists_ShouldReturnTrue_WhenUserExists()
    //{
    //    // Arrange
    //    var request = new SignUpRequest { Email = "test@example.com" };
    //    _mockContext.Setup(x => x.Users.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserEntity, bool>>>()))
    //        .ReturnsAsync(true);

    //    // Act
    //    var result = await _service.UserExists(request);

    //    // Assert
    //    Assert.True(result);
    //}

    //[Fact]
    //public async Task UserExists_ShouldReturnFalse_WhenUserDoesNotExist()
    //{
    //    // Arrange
    //    var request = new SignUpRequest { Email = "test@example.com" };
    //    _mockContext.Setup(x => x.Users.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserEntity, bool>>>()))
    //        .ReturnsAsync(false);

    //    // Act
    //    var result = await _service.UserExists(request);

    //    // Assert
    //    Assert.False(result);
    //}

    [Fact]
    public async Task CreateUser_ShouldReturnUserEntity_WhenCreationSucceeds()
    {
        // Arrange
        var request = new SignUpRequest { Email = "test@example.com", FirstName = "Test", LastName = "User", Password = "Password123" };
        var userEntity = new UserEntity { Email = request.Email, FirstName = request.FirstName, LastName = request.LastName, UserName = request.Email };
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.CreateUser(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnNull_WhenCreationFails()
    {
        // Arrange
        var request = new SignUpRequest { Email = "test@example.com", FirstName = "Test", LastName = "User", Password = "Password123" };
        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());

        // Act
        var result = await _service.CreateUser(request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GenerateVerificationRequest_ShouldReturnVerificationRequest_WhenValidUserEntity()
    {
        // Arrange
        var userEntity = new UserEntity { Email = "test@example.com" };

        // Act
        var result = _service.GenerateVerificationRequest(userEntity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userEntity.Email, result.Email);
    }

    [Fact]
    public void GenerateServiceBusMessage_ShouldReturnSerializedMessage_WhenValidRequest()
    {
        // Arrange
        var verificationRequest = new VerificationRequest { Email = "test@example.com" };

        // Act
        var result = _service.GenerateServiceBusMessage(verificationRequest);

        // Assert
        Assert.NotNull(result);
        var deserialized = JsonConvert.DeserializeObject<VerificationRequest>(result);
        Assert.Equal(verificationRequest.Email, deserialized!.Email);
    }
}
