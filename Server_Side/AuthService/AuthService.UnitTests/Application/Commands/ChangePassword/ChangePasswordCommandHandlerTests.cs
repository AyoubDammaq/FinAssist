using AuthService.Application.Commands.ChangePassword;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using FluentAssertions;
using MediatR;
using Moq;

namespace AuthService.UnitTests.Application.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandlerTests
{
    [Fact]
    public async Task Handle_MissingRequiredFields_ThrowsArgumentException()
    {
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);

        var sut = new ChangePasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ChangePasswordCommand(
            Guid.NewGuid(),
            new ChangePasswordRequestDto
            {
                CurrentPassword = "",
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

        var sut = new ChangePasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ChangePasswordCommand(
            Guid.NewGuid(),
            new ChangePasswordRequestDto
            {
                CurrentPassword = "Old#12345",
                NewPassword = "New#12345",
                ConfirmNewPassword = "Different#12345"
            });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Les mots de passe ne correspondent pas.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);

        var sut = new ChangePasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ChangePasswordCommand(
            userId,
            new ChangePasswordRequestDto
            {
                CurrentPassword = "Old#12345",
                NewPassword = "New#12345",
                ConfirmNewPassword = "New#12345"
            });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Utilisateur introuvable.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_CurrentPasswordInvalid_ThrowsUnauthorizedAccessException()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);
        password.Setup(p => p.VerifyPassword("bad", "hash", user)).ReturnsAsync(false);

        var sut = new ChangePasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ChangePasswordCommand(
            userId,
            new ChangePasswordRequestDto
            {
                CurrentPassword = "bad",
                NewPassword = "New#12345",
                ConfirmNewPassword = "New#12345"
            });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Mot de passe actuel incorrect.");

        repo.VerifyAll();
        password.VerifyAll();
    }

    [Fact]
    public async Task Handle_ValidRequest_UpdatesPassword_AndReturnsUnit()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "a@b.com",
            UserName = "user",
            PasswordHash = "hash",
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var password = new Mock<IPasswordManagement>(MockBehavior.Strict);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>())).ReturnsAsync(true);
        password.Setup(p => p.VerifyPassword("Old#12345", "hash", user)).ReturnsAsync(true);
        password.Setup(p => p.HashPassword("New#12345")).ReturnsAsync(("newhash", "salt"));

        var sut = new ChangePasswordCommandHandler(repo.Object, password.Object);

        var cmd = new ChangePasswordCommand(
            userId,
            new ChangePasswordRequestDto
            {
                CurrentPassword = "Old#12345",
                NewPassword = "New#12345",
                ConfirmNewPassword = "New#12345"
            });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        repo.Verify(r => r.Update(It.Is<User>(u =>
            u.Id == userId &&
            u.PasswordHash == "newhash" &&
            u.UpdatedAt > DateTime.UtcNow.AddMinutes(-1)
        )), Times.Once);

        repo.VerifyAll();
        password.VerifyAll();
    }
}