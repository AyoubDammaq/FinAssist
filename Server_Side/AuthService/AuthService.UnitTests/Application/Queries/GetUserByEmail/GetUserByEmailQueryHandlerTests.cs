using AuthService.Application.Queries.GetUserByEmail;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Queries.GetUserByEmail;

public sealed class GetUserByEmailQueryHandlerTests
{
    [Fact]
    public async Task Handle_UserFound_ReturnsMappedUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "user", PasswordHash = "hash" };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ReturnsAsync(user);

        var mapper = MapperFactory.Create();
        var sut = new GetUserByEmailQueryHandler(repo.Object, mapper);

        // Act
        var result = await sut.Handle(new GetUserByEmailQuery("a@b.com"), CancellationToken.None);

        // Assert
        result.UserName.Should().Be("user");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserNotFound_WrapsInApplicationException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("missing@b.com")).ReturnsAsync((User?)null);

        var mapper = MapperFactory.Create();
        var sut = new GetUserByEmailQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetUserByEmailQuery("missing@b.com"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération de l'utilisateur par email.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_WrapsInApplicationException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetByEmail("a@b.com")).ThrowsAsync(new InvalidOperationException("boom"));

        var mapper = MapperFactory.Create();
        var sut = new GetUserByEmailQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetUserByEmailQuery("a@b.com"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération de l'utilisateur par email.");

        repo.VerifyAll();
    }
}