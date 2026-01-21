using AuthService.Application.Commands.ForgotPassword;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using MediatR;
using Moq;

namespace AuthService.UnitTests.Application.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_MissingEmail_ThrowsArgumentException()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var token = new Mock<ITokenManagement>(MockBehavior.Strict);

        var sut = new ForgotPasswordCommandHandler(repo.Object, token.Object);
        var cmd = new ForgotPasswordCommand(new ForgotPasswordRequestDto { Email = " " });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email requis.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnit_AndDoesNotUpdate()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync((User?)null);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);

        var sut = new ForgotPasswordCommandHandler(repo.Object, token.Object);
        var cmd = new ForgotPasswordCommand(new ForgotPasswordRequestDto { Email = "a@b.com" });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserFound_SetsResetToken_AndUpdatesUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash"
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        token.Setup(t => t.GenerateResetToken()).ReturnsAsync("reset.token");

        var sut = new ForgotPasswordCommandHandler(repo.Object, token.Object);
        var cmd = new ForgotPasswordCommand(new ForgotPasswordRequestDto { Email = "a@b.com" });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.Is<User>(u =>
            u.ResetToken == "reset.token" &&
            u.ResetTokenExpiryTime.HasValue &&
            u.ResetTokenExpiryTime.Value > DateTime.UtcNow.AddMinutes(14) &&
            u.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);

        repo.VerifyAll();
        token.VerifyAll();
    }
}