using AuthService.Application.Commands.Login;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync((User?)null);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        var mapper = MapperFactory.Create();

        var sut = new LoginCommandHandler(repo.Object, password.Object, token.Object, mapper);
        var cmd = new LoginCommand(new LoginRequestDto { Email = "a@b.com", Password = "Secret#123" });

        // Act
        var act = () => sut.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Nom d'utilisateur ou mot de passe incorrect.");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.VerifyPassword("bad", "hash", user)).ReturnsAsync(false);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        var mapper = MapperFactory.Create();

        var sut = new LoginCommandHandler(repo.Object, password.Object, token.Object, mapper);
        var cmd = new LoginCommand(new LoginRequestDto { Email = "a@b.com", Password = "bad" });

        // Act
        var act = () => sut.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Nom d'utilisateur ou mot de passe incorrect.");

        repo.VerifyAll();
        password.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.VerifyPassword("Secret#123", "hash", user)).ReturnsAsync(true);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        token.Setup(t => t.GenerateToken(user)).ReturnsAsync("access");
        token.Setup(t => t.GenerateRefreshToken()).ReturnsAsync("refresh");

        var mapper = MapperFactory.Create();

        var sut = new LoginCommandHandler(repo.Object, password.Object, token.Object, mapper);
        var cmd = new LoginCommand(new LoginRequestDto { Email = "a@b.com", Password = "Secret#123" });

        // Act
        var result = await sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access");
        result.RefreshToken.Should().Be("refresh");

        repo.VerifyAll();
        password.VerifyAll();
        token.VerifyAll();
    }
}