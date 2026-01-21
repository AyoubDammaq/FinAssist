using AuthService.Application.Commands.RefreshToken;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandlerTests
{
    [Fact]
    public async Task Handle_MissingRefreshToken_ThrowsUnauthorizedAccessException()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var token = new Mock<ITokenManagement>(MockBehavior.Strict);

        var sut = new RefreshTokenCommandHandler(repo.Object, token.Object);
        var cmd = new RefreshTokenCommand(new RefreshTokenRequestDto { Id = Guid.NewGuid(), RefreshToken = " " });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token manquant.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);

        var sut = new RefreshTokenCommandHandler(repo.Object, token.Object);
        var cmd = new RefreshTokenCommand(new RefreshTokenRequestDto { Id = userId, RefreshToken = "rt" });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token invalide.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_ExpiredRefreshToken_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);

        var sut = new RefreshTokenCommandHandler(repo.Object, token.Object);
        var cmd = new RefreshTokenCommand(new RefreshTokenRequestDto { Id = userId, RefreshToken = "rt" });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Refresh token expiré.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidRefresh_ReturnsNewTokens_AndUpdatesUser()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            RefreshToken = "old",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        token.Setup(t => t.GenerateToken(user)).ReturnsAsync("access.new");
        token.Setup(t => t.GenerateRefreshToken()).ReturnsAsync("refresh.new");

        var sut = new RefreshTokenCommandHandler(repo.Object, token.Object);
        var cmd = new RefreshTokenCommand(new RefreshTokenRequestDto { Id = userId, RefreshToken = "rt" });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.AccessToken.Should().Be("access.new");
        result.RefreshToken.Should().Be("refresh.new");

        repo.Verify(r => r.Update(It.Is<User>(u =>
            u.Id == userId &&
            u.RefreshToken == "refresh.new" &&
            u.RefreshTokenExpiryTime.HasValue &&
            u.RefreshTokenExpiryTime.Value > DateTime.UtcNow.AddDays(6) // ~7 jours
        )), Times.Once);

        repo.VerifyAll();
        token.VerifyAll();
    }
}