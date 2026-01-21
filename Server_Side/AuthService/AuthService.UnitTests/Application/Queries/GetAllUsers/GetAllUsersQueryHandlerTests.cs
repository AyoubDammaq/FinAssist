using AuthService.Application.Queries.GetAllUsers;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using Moq;

namespace AuthService.UnitTests.Application.Queries.GetAllUsers;

public sealed class GetAllUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsMappedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "a@b.com", UserName = "user1", PasswordHash = "hash1" },
            new() { Id = Guid.NewGuid(), Email = "c@d.com", UserName = "user2", PasswordHash = "hash2" }
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetAll()).ReturnsAsync(users);

        var mapper = MapperFactory.Create();
        var sut = new GetAllUsersQueryHandler(repo.Object, mapper);

        // Act
        var result = await sut.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].Email.Should().Be("a@b.com");
        result[0].UserName.Should().Be("user1");
        result[1].Email.Should().Be("c@d.com");
        result[1].UserName.Should().Be("user2");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrows_WrapsInApplicationException()
    {
        // Arrange
        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetAll()).ThrowsAsync(new InvalidOperationException("boom"));

        var mapper = MapperFactory.Create();
        var sut = new GetAllUsersQueryHandler(repo.Object, mapper);

        // Act
        var act = () => sut.Handle(new GetAllUsersQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ApplicationException>()
            .WithMessage("Une erreur est survenue lors de la récupération des utilisateurs.");

        repo.VerifyAll();
    }
}