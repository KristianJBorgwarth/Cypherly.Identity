using Identity.Domain.Common;
using MediatR;

namespace Identity.Application.Abstractions;

public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

