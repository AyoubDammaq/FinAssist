using AuthService.Application.Commands.Register;
using AuthService.Application.DTOs;
using AuthService.Application.Utils;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Commands.Register;

public sealed class RegisterCommandHandlerTests
{
    [Fact]
    public async Task Handle_EmailAlreadyTaken_ThrowsApplicationException()
    {
        // Arrange
        var existing = new User { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "existing", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(existing);

        var password = new Mock<IPasswordManagement>(MockBehavior.Loose);
        var mapper = MapperFactory.Create();

        var sut = new RegisterCommandHandler(repo.Object, password.Object, mapper);

        var cmd = new RegisterCommand(new RegisterRequestDto
        {
            UserName = "new",
            Email = "a@b.com",
            Password = "Secret#123",
            ConfirmPassword = "Secret#123"
        });

        // Act
        var act = () => sut.Handle(cmd, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Le nom d'utilisateur a@b.com est déjà pris.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_NewUser_CallsRegister_AndReturnsResponse()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync((User?)null);
        repo.Setup(r => r.Register(It.IsAny<User>())).ReturnsAsync(Guid.NewGuid());

        // Mock du password pour renvoyer "fort"
        var password = new Mock<IPasswordManagement>(MockBehavior.Loose);
        password.Setup(p => p.IsPasswordStrong(It.IsAny<string>()))
                .ReturnsAsync(true);

        var mapper = MapperFactory.Create();

        var sut = new RegisterCommandHandler(repo.Object, password.Object, mapper);

        var cmd = new RegisterCommand(new RegisterRequestDto
        {
            UserName = "new",
            Email = "a@b.com",
            Password = "Secret#123",
            ConfirmPassword = "Secret#123"
        });

        // Act
        var result = await sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Email.Should().Be("a@b.com");
        result.UserName.Should().Be("new");

        repo.Verify(r => r.Register(It.Is<User>(u =>
            u.Email == "a@b.com" &&
            u.UserName == "new" &&
            !string.IsNullOrWhiteSpace(u.PasswordHash)
        )), Times.Once);

        repo.VerifyAll();
    }
}