using Cypherly.Domain.Common;
using Mediator;

namespace Identity.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
