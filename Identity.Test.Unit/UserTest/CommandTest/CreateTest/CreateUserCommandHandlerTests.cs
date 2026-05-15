using AutoFixture;
using Cypherly.Domain.Common;
using Cypherly.Message.Contracts.Messages.Profile;
using Cypherly.Message.Contracts.Responses.Profile;
using FakeItEasy;
using FluentAssertions;
using Identity.Application.Contracts.Repository;
using Identity.Application.Features.User.Commands.Create;
using Identity.Domain.Aggregates;
using Identity.Domain.Common;
using Identity.Domain.Services.User;
using Identity.Domain.ValueObjects;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Identity.Test.Unit.UserTest.CommandTest.CreateTest;

public class CreateUserCommandHandlerTests
{
    private readonly IUserRepository _fakeRepo;
    private readonly IUserLifeCycleService _fakeUserLifeCycleServices;
    private readonly IUnitOfWork _fakeUnitOfWork;
    private readonly IRequestClient<CreateUserProfileMessage> _fakeRequestClient;
    private readonly Fixture _fixture = new();
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _fakeRepo = A.Fake<IUserRepository>();
        _fakeUserLifeCycleServices = A.Fake<IUserLifeCycleService>();
        _fakeUnitOfWork = A.Fake<IUnitOfWork>();
        _fakeRequestClient = A.Fake<IRequestClient<CreateUserProfileMessage>>();
        var fakeLogger = A.Fake<ILogger<CreateUserCommandHandler>>();


        _sut = new CreateUserCommandHandler(_fakeRepo, _fakeUserLifeCycleServices, _fakeUnitOfWork, _fakeRequestClient, fakeLogger);
    }

    [Fact]
    public async Task Handle_Valid_Command_Should_Return_ResultOk()
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "test@mail.dk",
            Password = "password923K=?",
            Username = "validUsername"

        };

        var user = new User(Guid.NewGuid(), Email.Create(cmd.Email), Password.Create(cmd.Password), isVerified: false);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserByEmailSpec>._, A<CancellationToken>._)).Returns<User>(null);
        A.CallTo(() => _fakeUserLifeCycleServices.CreateUser(cmd.Email, cmd.Password)).Returns(Result.Ok(user));
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).DoesNothing();
        A.CallTo(() => _fakeRepo.CreateAsync(A<User>.Ignored, A<CancellationToken>._)).DoesNothing();

        // Fake the MassTransit response
        var responseMessage = _fixture.Build<CreateUserProfileResponse>()
            .With(x => x.IsSuccess, true)
            .Create();

        var fakeMassTransitResponse = A.Fake<Response<CreateUserProfileResponse>>();
        A.CallTo(() => fakeMassTransitResponse.Message).Returns(responseMessage);

        // Simulate the response from requestClient for profile creation
        A.CallTo(() => _fakeRequestClient.GetResponse<CreateUserProfileResponse>(A<CreateUserProfileMessage>.Ignored, A<CancellationToken>.Ignored, A<RequestTimeout>.Ignored))
            .Returns(Task.FromResult(fakeMassTransitResponse));

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Email.Should().Be(cmd.Email);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserByEmailSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRepo.CreateAsync(A<User>.Ignored, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).MustHaveHappenedOnceExactly();

        A.CallTo(() => _fakeRequestClient.GetResponse<CreateUserProfileResponse>(A<CreateUserProfileMessage>.Ignored, A<CancellationToken>.Ignored, A<RequestTimeout>.Ignored))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Handle_Given_Email_Exists_Should_Return_ResultFail()
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "test@mail.dk",
            Password = "password923K=?",
            Username = "validUsername"
        };


        var existingUser = new User(Guid.NewGuid(), Email.Create(cmd.Email), Password.Create(cmd.Password), isVerified: false);

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserByEmailSpec>._, A<CancellationToken>._)).Returns(existingUser);

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("An account already exists with that email");

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserByEmailSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRepo.CreateAsync(A<User>.Ignored, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => _fakeRequestClient.GetResponse<CreateUserProfileResponse>(A<CreateUserProfileMessage>.Ignored, A<CancellationToken>.Ignored, A<RequestTimeout>.Ignored))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Handle_Given_UserService_CreateUser_Fails_Should_Return_ResultFail()
    {
        // Arrange
        var cmd = new CreateUserCommand()
        {
            Email = "test@mail.dk",
            Password = "password923K=?",
            Username = "validUsername"
        };


        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserByEmailSpec>._, A<CancellationToken>._)).Returns<User>(null);
        A.CallTo(() => _fakeUserLifeCycleServices.CreateUser(cmd.Email, cmd.Password)).Returns(Result.Fail<User>(Errors.General.UnspecifiedError("error")));

        // Act
        var result = await _sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Message.Should().Be("error");

        A.CallTo(() => _fakeRepo.GetSinleAsync(A<UserByEmailSpec>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeUserLifeCycleServices.CreateUser(A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
        A.CallTo(() => _fakeRepo.CreateAsync(A<User>.Ignored, A<CancellationToken>._)).MustNotHaveHappened();
        A.CallTo(() => _fakeUnitOfWork.SaveChangesAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        A.CallTo(() => _fakeRequestClient.GetResponse<CreateUserProfileResponse>(A<CreateUserProfileMessage>.Ignored, A<CancellationToken>.Ignored, A<RequestTimeout>.Ignored))
            .MustNotHaveHappened();
    }
}
