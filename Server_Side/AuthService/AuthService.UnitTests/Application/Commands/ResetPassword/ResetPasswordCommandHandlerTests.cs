using AuthService.Application.Commands.ResetPassword;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace AuthService.UnitTests.Application.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_MissingRequiredFields_ThrowsArgumentException()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = " ",
            ResetToken = "token",
            NewPassword = "New#12345",
            ConfirmNewPassword = "New#12345"
        });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Champs requis manquants.");
    }

    [Fact]
    public async Task Handle_PasswordMismatch_ThrowsArgumentException()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = "a@b.com",
            ResetToken = "token",
            NewPassword = "New#12345",
            ConfirmNewPassword = "Different#12345"
        });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Les mots de passe ne correspondent pas.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsUnit()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync((User?)null);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = "a@b.com",
            ResetToken = "token",
            NewPassword = "New#12345",
            ConfirmNewPassword = "New#12345"
        });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);
        repo.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_TokenMissingOnUser_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            ResetToken = null,
            ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(10)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = "a@b.com",
            ResetToken = "token",
            NewPassword = "New#12345",
            ConfirmNewPassword = "New#12345"
        });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Reset token invalide ou expiré.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_TokenMismatch_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            ResetToken = "token.db",
            ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(10)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = "a@b.com",
            ResetToken = "token.input",
            NewPassword = "New#12345",
            ConfirmNewPassword = "New#12345"
        });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Reset token invalide ou expiré.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_TokenExpired_ThrowsUnauthorizedAccessException()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            ResetToken = "token",
            ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(-1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = "a@b.com",
            ResetToken = "token",
            NewPassword = "New#12345",
            ConfirmNewPassword = "New#12345"
        });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Reset token invalide ou expiré.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesPassword_AndInvalidatesToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash.old",
            ResetToken = "token",
            ResetTokenExpiryTime = DateTime.UtcNow.AddMinutes(10),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);
        password.Setup(p => p.HashPassword("New#12345")).ReturnsAsync(("hash.new", "salt"));

        var sut = new ResetPasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ResetPasswordCommand(new ResetPasswordRequestDto
        {
            Email = "a@b.com",
            ResetToken = "token",
            NewPassword = "New#12345",
            ConfirmNewPassword = "New#12345"
        });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.Is<User>(u =>
            u.PasswordHash == "hash.new" &&
            u.ResetToken == null &&
            u.ResetTokenExpiryTime == null &&
            u.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);

        repo.VerifyAll();
        password.VerifyAll();
    }
}