using AuthService.Application.Commands.Logout;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace AuthService.UnitTests.Application.Commands.Logout;

public sealed class LogoutCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnit_AndDoesNotUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        var sut = new LogoutCommandHandler(repo.Object);
        var cmd = new LogoutCommand(new User { Id = userId, Email = "a@b.com", UserName = "user", PasswordHash = "hash" });

        // Act
        var result = await sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);
        repo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserFound_ClearsRefreshToken_AndUpdatesUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userFromDb = new User
        {
            Id = userId,
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            RefreshToken = "refresh.old",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(userFromDb);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var sut = new LogoutCommandHandler(repo.Object);
        var cmd = new LogoutCommand(new User { Id = userId, Email = "a@b.com", UserName = "user", PasswordHash = "hash" });

        // Act
        var result = await sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.Is<User>(u =>
            u.Id == userId &&
            u.RefreshToken == null &&
            u.RefreshTokenExpiryTime == null
        )), Times.Once);

        repo.VerifyAll();
    }
}