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

public class VerificationCodeGeneratedEventHandlerTest
{
    private readonly IUserRepository _fakeRepo;
    private readonly IProducer<SendEmailMessage> _fakeEmailProducer;
    private readonly VerificationCodeGeneratedEventHandler _sut;

    public VerificationCodeGeneratedEventHandlerTest()
    {
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeEmailProducer = A.Fake<IProducer<SendEmailMessage>>();
        var fakeLogger = A.Fake<ILogger<VerificationCodeGeneratedEventHandler>>();
        _sut = new(_fakeRepo, _fakeEmailProducer, fakeLogger);
    }

    [Fact]
    public async Task Handle_Valid_Notification_Should_Send_Email()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), false);
        user.AddVerificationCode(UserVerificationCodeType.EmailVerification);

        var notification = new VerificationCodeGeneratedEvent(user.Id, UserVerificationCodeType.EmailVerification);

        A.CallTo(() => _fakeRepo.GetByIdAsync(user.Id, A<CancellationToken>._)).Returns(user);
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, CancellationToken.None)).DoesNothing();

        // Act
        await _sut.Handle(notification, CancellationToken.None);

        // Assert
        A.CallTo(() => _fakeRepo.GetByIdAsync(user.Id, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, CancellationToken.None)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Notification_With_Invalid_Id_Should_Throw_Exception()
    {
        // Arrange
        var notification = new VerificationCodeGeneratedEvent(Guid.NewGuid(), UserVerificationCodeType.EmailVerification);
        A.CallTo(() => _fakeRepo.GetByIdAsync(notification.UserId, A<CancellationToken>._)).Returns<User?>(null);


        // Act
        var act = async () => await _sut.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        A.CallTo(() => _fakeRepo.GetByIdAsync(notification.UserId, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, CancellationToken.None)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Notification_With_Missing_Verification_Code_Should_Throw_Exception()
    {
        // Arrange
        var user = new User(Guid.NewGuid(), Email.Create("Test@mail.dk"), Password.Create("kjsKidh??923"), false);

        var notification = new VerificationCodeGeneratedEvent(user.Id, UserVerificationCodeType.EmailVerification);

        A.CallTo(() => _fakeRepo.GetByIdAsync(user.Id, A<CancellationToken>._)).Returns(user);

        // Act
        var act = async () => await _sut.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        A.CallTo(() => _fakeRepo.GetByIdAsync(user.Id, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeEmailProducer.PublishMessageAsync(A<SendEmailMessage>._, CancellationToken.None)).MustNotHaveHappened();
    }

}