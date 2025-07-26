using Cypherly.Domain.Common;
using MediatR;

namespace Identity.Application.Abstractions;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse> { }

public interface ILimitedQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQueryLimited<TResponse> { }