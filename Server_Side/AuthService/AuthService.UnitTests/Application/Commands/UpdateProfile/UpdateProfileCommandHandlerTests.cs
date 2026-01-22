using AuthService.Application.Commands.UpdateProfile;
using AuthService.Application.DTOs;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.UnitTests.TestUtils;
using FluentAssertions;
using MediatR;
using Moq;

namespace AuthService.UnitTests.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandlerTests
{
    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        var userId = Guid.NewGuid();

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync((User?)null);

        var mapper = MapperFactory.Create();
        var sut = new UpdateProfileCommandHandler(repo.Object, mapper);

        var cmd = new UpdateProfileCommand(new UpdateProfileDto
        {
            Id = userId,
            UserName = "new",
            FirstName = "First",
            LastName = "Last",
            PhoneNumber = "0600000000"
        });

        var act = () => sut.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"User with ID {userId} not found.");

        repo.VerifyAll();
    }

    [Fact]
    public async Task Handle_UserFound_MapsAndUpdates_AndReturnsUnit()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "a@b.com",
            UserName = "old",
            FirstName = "Old",
            LastName = "Name",
            PhoneNumber = "0700000000"
        };

        var repo = new Mock<IUserRepository>(MockBehavior.Strict);
        repo.Setup(r => r.GetById(userId)).ReturnsAsync(user);
        repo.Setup(r => r.Update(It.IsAny<User>())).Returns(Task.CompletedTask);

        var mapper = MapperFactory.Create();
        var sut = new UpdateProfileCommandHandler(repo.Object, mapper);

        var cmd = new UpdateProfileCommand(new UpdateProfileDto
        {
            Id = userId,
            UserName = "new",
            FirstName = "First",
            LastName = "Last",
            PhoneNumber = "0600000000"
        });

        var result = await sut.Handle(cmd, CancellationToken.None);

        result.Should().Be(Unit.Value);

        user.UserName.Should().Be("new");
        user.FirstName.Should().Be("First");
        user.LastName.Should().Be("Last");
        user.PhoneNumber.Should().Be("0600000000");

        repo.Verify(r => r.Update(It.Is<User>(u => u.Id == userId)), Times.Once);
        repo.VerifyAll();
    }
}