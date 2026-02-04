using AuthService.Application.Commands.ForgotPassword;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace AuthService.UnitTests.Application.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_MissingEmail_ThrowsArgumentException()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        var email = new Mock<IEmailManagment>(MockBehavior.Strict);
        var logger = new Mock<ILogger<ForgotPasswordCommandHandler>>();

        var sut = new ForgotPasswordCommandHandler(repo.Object, token.Object, email.Object, logger.Object);
        var cmd = new ForgotPasswordCommand(new ForgotPasswordRequestDto { Email = " " });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email requis.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnit_AndDoesNotUpdate_AndDoesNotSendEmail()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("finassistservice267@gmail.com")).ReturnsAsync((User?)null);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        var email = new Mock<IEmailManagment>(MockBehavior.Strict);
        var logger = new Mock<ILogger<ForgotPasswordCommandHandler>>();

        email.Setup(e => e.CheckEmailValidation("finassistservice267@gmail.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new ForgotPasswordCommandHandler(repo.Object, token.Object, email.Object, logger.Object);
        var cmd = new ForgotPasswordCommand(new ForgotPasswordRequestDto { Email = "finassistservice267@gmail.com" });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        email.Verify(e => e.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        repo.VerifyAll();
        token.VerifyAll();
        email.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserFound_SetsResetToken_AndUpdatesUser_AndSendsEmail()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "ayoubdammak81@gmail.com",
            UserName = "user",
            PasswordHash = "hash"
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("ayoubdammak81@gmail.com")).ReturnsAsync(user);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var token = new Mock<ITokenManagement>(MockBehavior.Strict);
        token.Setup(t => t.GenerateResetToken()).ReturnsAsync("reset.token");
        token.Setup(t => t.HashToken("reset.token")).Returns("reset.token");

        var email = new Mock<IEmailManagment>(MockBehavior.Strict);
        email.Setup(e => e.CheckEmailValidation("ayoubdammak81@gmail.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        email.Setup(e => e.SendPasswordResetEmailAsync("ayoubdammak81@gmail.com", "reset.token")).ReturnsAsync(true);

        var logger = new Mock<ILogger<ForgotPasswordCommandHandler>>();

        var sut = new ForgotPasswordCommandHandler(repo.Object, token.Object, email.Object, logger.Object);
        var cmd = new ForgotPasswordCommand(new ForgotPasswordRequestDto { Email = "ayoubdammak81@gmail.com" });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.Is<User>(u =>
            u.ResetTokenHash == "reset.token" &&
            u.ResetTokenExpiryTime.HasValue &&
            u.ResetTokenExpiryTime.Value > DateTime.UtcNow.AddMinutes(14) &&
            u.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);

        email.Verify(e => e.SendPasswordResetEmailAsync("ayoubdammak81@gmail.com", "reset.token"), Times.Once);

        repo.VerifyAll();
        token.VerifyAll();
        email.VerifyAll();
    }
}