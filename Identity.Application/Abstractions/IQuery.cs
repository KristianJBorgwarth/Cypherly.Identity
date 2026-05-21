using Cypherly.Domain.Common;
using MediatR;

namespace Identity.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }
