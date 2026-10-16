namespace Identity.Application.Dtos;

public sealed record AccessToken(string Token, DateTime ExpiresAt);
