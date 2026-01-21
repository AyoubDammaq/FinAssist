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
        var userEmail = "a@b.com";

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        // Setup must match handler (GetByEmail)
        repo.Setup(r => r.GetByEmail(userEmail)).ReturnsAsync((User?)null);

        var sut = new LogoutCommandHandler(repo.Object);
        var cmd = new LogoutCommand(userEmail);

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
        var userEmail = "a@b.com";
        var userFromDb = new User
        {
            Id = userId,
            Email = userEmail,
            UserName = "user",
            PasswordHash = "hash",
            RefreshToken = "refresh.old",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        // Setup must match handler (GetByEmail)
        repo.Setup(r => r.GetByEmail(userEmail)).ReturnsAsync(userFromDb);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var sut = new LogoutCommandHandler(repo.Object);
        var cmd = new LogoutCommand(userEmail);

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