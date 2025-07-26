using Cypherly.Domain.Common;
using MediatR;

namespace Cypherly.Identity.Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

public interface IQueryLimited<TResponse> : IRequest<Result<TResponse>>
{
    public Guid UserProfileId { get; init; }
}