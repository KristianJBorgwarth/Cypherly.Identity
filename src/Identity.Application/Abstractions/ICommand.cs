using Cendrite.Domain.Common;
using Mediator;

namespace Identity.Application.Abstractions;

public interface ICommand : IRequest<Result> { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>> { }

