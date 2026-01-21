using AuthService.Application.Queries.GetUserByUsername;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Queries.GetUserByUsername;

public sealed class GetUserByUsernameQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserFound_ReturnsMappedUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByUsername("user")).ReturnsAsync(user);

        var mapper = MapperFactory.Create();
        var sut = new GetUserByUsernameQueryHandler(repo.Object, mapper);

        // Act
        var result = await sut.Handle(new GetUserByUsernameQuery("user"), CancellationToken.None);

        // Assert
        result.Email.Should().Be("a@b.com");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserNotFound_WrapsInApplicationException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByUsername("missing")).ReturnsAsync((User?)null);

        var mapper = MapperFactory.Create();
        var sut = new GetUserByUsernameQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetUserByUsernameQuery("missing"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération de l'utilisateur par nom d'utilisateur.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_WrapsInApplicationException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByUsername("user")).ThrowsAsync(new InvalidOperationException("boom"));

        var mapper = MapperFactory.Create();
        var sut = new GetUserByUsernameQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetUserByUsernameQuery("user"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération de l'utilisateur par nom d'utilisateur.");

        repo.VerifyAll();
    }
}