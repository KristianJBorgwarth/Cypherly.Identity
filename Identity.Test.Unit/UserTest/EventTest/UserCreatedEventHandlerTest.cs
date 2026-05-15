using Cypherly.Message.Contracts.Abstractions;
using Cypherly.Message.Contracts.Messages.Email;
using FakeItEasy;
using FluentAssertions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.User.Events;
using Identity.Domain.Aggregates;
using Identity.Domain.Enums;
using Identity.Domain.Events.User;
using Identity.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.UserTest.EventTest;

public class UserCreatedEventHandlerTest
{
    private readonly IUserRepository _fakeUserRepository;
    private readonly IProducer<SendEmailMessage> _fakeEmailProducer;
    private readonly UserCreatedEventHandler _sut;

    public UserCreatedEventHandlerTest()
    {
        _fakeUserRepository = A.Fake<IUserRepository>();
        _fakeEmailProducer = A.Fake<IProducer<SendEmailMessage>>();
        var fakeLogger = A.Fake<ILogger<UserCreatedEventHandler>>();

        _sut = new(_fakeUserRepository, _fakeEmailProducer, fakeLogger);
    }

    [Fact]
    public async Task Handle_UserCreatedEvent_UserFound_SendEmailMessage()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("Test=??8239"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, A<CancellationToken>._))
            .Returns(Task.CompletedTask);

        var userCreatedEvent = new UserCreatedEvent(user.Id);

        // Act
        await _sut.Handle(userCreatedEvent, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_UserCreatedEvent_UserNotFound_ThrowException()
    {
        // Arrange
        var userCreatedEvent = new UserCreatedEvent(Guid.NewGuid());
        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns<User?>(null);

        // Act
        var act = async () => await _sut.Handle(userCreatedEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User not found");
        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_UserCreatedEvent_VerificationCodeNotFound_ThrowException()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("fuckasdk?2329JS"), false);
        var userEvent = new UserCreatedEvent(user.Id);

        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        //Will throw since user will have no verification code
        //Act
        var act = async () => await _sut.Handle(userEvent, CancellationToken.None);


        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Verification code not found");

        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_UserCreatedEvent_SendEmailMessageFailed_ThrowException()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("test@mail.dk"), Password.Create("Test=??8239"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).Returns(user);

        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, A<CancellationToken>._))
            .Throws<Exception>();

        var userCreatedEvent = new UserCreatedEvent(user.Id);

        // Act
        var act = async () => await _sut.Handle(userCreatedEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        A.CallTo(() => _fakeUserRepository.GetSinleAsync(A<UserWithVerificationCodesSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}