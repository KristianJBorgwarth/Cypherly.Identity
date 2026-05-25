using FluentAssertions;
using Identity.Application.Caching;
using Identity.Application.Features.Authentication.Commands.VerifyNonce;
using Identity.Test.Unit.Setup.Helpers;

namespace Identity.Test.Unit.AuthenticationTest.CommandTest.VerifyNonce;

public class VerifyNonceServiceTest
{
    private readonly IVerifyNonceService _verifyNonceService = new VerifyNonceService();
    private const string PublicKey = "VlmK9Smh3RVtT7CHaHW5rbrYAWeM9ImVdP6WhmnMqK0=";
    private const string PrivateKey = "mR6AP1dNY1eEp7Z7bn6q0gPiOvcDl3FX4th65LY3Zwg=";

    [Fact]
    public void VerifyNonce_ValidNonceAndSignature_ReturnsTrue()
    {
        // Arrange

        var nonce = Nonce.Create(Guid.NewGuid(), Guid.NewGuid());

        var signature = Ed25519Helper.SignNonce(nonce.NonceValue, PrivateKey);

        // Act
        var result = _verifyNonceService.VerifyNonce(nonce.NonceValue, signature, PublicKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyNonce_InvalidNonce_ReturnsFalse()
    {
        // Arrange
        const string incorrectPrivateKey = "mR6AP1dNY1eEp7Z7bn6q0gPiOvcDl3FX4th65LY8Zwg=";
        var nonce = Nonce.Create(Guid.NewGuid(), Guid.NewGuid());

        var signature = Ed25519Helper.SignNonce(nonce.NonceValue, incorrectPrivateKey);

        // Act
        var result = _verifyNonceService.VerifyNonce(nonce.NonceValue, signature, PublicKey);

        // Assert
        result.Should().BeFalse();
    }
}