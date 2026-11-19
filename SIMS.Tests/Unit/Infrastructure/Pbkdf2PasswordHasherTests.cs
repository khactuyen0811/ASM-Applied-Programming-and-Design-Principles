using SIMS.Infrastructure.Security;

namespace SIMS.Tests.Unit.Infrastructure;

public sealed class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesDifferentSaltEachCall()
    {
        var first = _hasher.Hash("12345");
        var second = _hasher.Hash("12345");

        Assert.NotEqual(first.Salt, second.Salt);
        Assert.NotEqual(first.Hash, second.Hash);
        Assert.False(string.IsNullOrWhiteSpace(first.Hash));
        Assert.False(string.IsNullOrWhiteSpace(first.Salt));
    }

    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var result = _hasher.Hash("correct-horse");

        Assert.True(_hasher.Verify("correct-horse", result.Hash, result.Salt));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var result = _hasher.Hash("correct-horse");

        Assert.False(_hasher.Verify("wrong-horse", result.Hash, result.Salt));
    }

    [Theory]
    [InlineData("", "c2FsdA==")]
    [InlineData("not-base64", "c2FsdA==")]
    [InlineData("aGFzaA==", "")]
    [InlineData("aGFzaA==", "not-base64")]
    [InlineData("YWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWFhYWE=", "c2FsdA==")]
    public void Verify_ReturnsFalse_ForTamperedOrEmptyHashOrSalt(string hash, string salt)
    {
        Assert.False(_hasher.Verify("12345", hash, salt));
    }
}
