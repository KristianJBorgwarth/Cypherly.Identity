using Cypherly.Authentication.Test.Integration.Setup;
using Cypherly.Identity.Application.Contracts.Repository;
using Cypherly.Identity.Application.Features.User.Events;
using Cypherly.Identity.Domain.Aggregates;
using Cypherly.Identity.Domain.Enums;
using Cypherly.Identity.Domain.Events.User;
using Cypherly.Identity.Domain.ValueObjects;
using Cypherly.Identity.Infrastructure.Persistence.Context;
using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Email;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cypherly.Authentication.Test.Integration.UserTest.EventTest;

public class UserCreatedEventHandlerTest : IntegrationTestBase
{
    private readonly UserCreatedEventHandler _sut;
    public UserCreatedEventHandlerTest(IntegrationTestFactory<Program, IdentityDbContext> factory) : base(factory)
    {
        var scope = factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var emailProducer = scope.ServiceProvider.GetRequiredService<IProducer<SendEmailMessage>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<UserCreatedEventHandler>>();
        _sut = new(userRepository, emailProducer, logger);
    }

    [Fact]
    public async Task Handle_Given_Valid_Event_Should_Send_Email()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("validPassword=?23"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);
        await Db.User.AddAsync(user);
        await Db.SaveChangesAsync();

        var @event = new UserCreatedEvent(user.Id);

        // Act
        await _sut.Handle(@event, CancellationToken.None);

        // Assert
        Harness.Published.Select<SendEmailMessage>().FirstOrDefault(uc => uc.Context.Message.To == user.Email.Address).Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_Given_Invalid_Event_Should_Throw_Exception()
    {
        // Arrange
        var @event = new UserCreatedEvent(Guid.NewGuid());

        // Act
        var act = async () => await _sut.Handle(@event, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}