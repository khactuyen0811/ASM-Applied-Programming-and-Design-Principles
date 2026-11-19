namespace SIMS.Domain.Abstractions
{
    public sealed record PasswordHashResult(string Hash, string Salt);

    public interface IPasswordHasher
    {
        PasswordHashResult Hash(string password);

        bool Verify(string password, string hash, string salt);
    }
}
