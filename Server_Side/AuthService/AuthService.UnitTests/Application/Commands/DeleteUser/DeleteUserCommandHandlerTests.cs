using AuthService.Application.Commands.DeleteUser;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Commands.DeleteUser;

public sealed class DeleteUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsApplicationException()
    {
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        var mapper = MapperFactory.Create();
        var sut = new DeleteUserCommandHandler(repo.Object, mapper);

        var cmd = new DeleteUserCommand(userId);

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la suppression de l'utilisateur.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserFound_DeletesUser_AndReturnsUnit()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);
        repo.Setup(r => r.Delete(user)).Returns(Task.CompletedTask);

        var mapper = MapperFactory.Create();
        var sut = new DeleteUserCommandHandler(repo.Object, mapper);

        var cmd = new DeleteUserCommand(userId);

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(MediatR.Unit.Value);

        repo.VerifyAll();
    }
}