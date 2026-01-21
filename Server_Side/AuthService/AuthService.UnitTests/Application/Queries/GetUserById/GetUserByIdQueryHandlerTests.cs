using AuthService.Application.Queries.GetUserById;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Queries.GetUserById;

public sealed class GetUserByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserFound_ReturnsMappedUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);

        var mapper = MapperFactory.Create();
        var sut = new GetUserByIdQueryHandler(repo.Object, mapper);

        // Act
        var result = await sut.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        // Assert
        result.Id.Should().Be(userId);
        result.Email.Should().Be("a@b.com");
        result.UserName.Should().Be("user");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserNotFound_WrapsInApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        var mapper = MapperFactory.Create();
        var sut = new GetUserByIdQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération de l'utilisateur par ID.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_WrapsInApplicationException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ThrowsAsync(new InvalidOperationException("boom"));

        var mapper = MapperFactory.Create();
        var sut = new GetUserByIdQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération de l'utilisateur par ID.");

        repo.VerifyAll();
    }
}