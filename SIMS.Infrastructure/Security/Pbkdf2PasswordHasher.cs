using System.Security.Cryptography;
using SIMS.Domain.Abstractions;

namespace SIMS.Infrastructure.Security
{
    public sealed class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public PasswordHashResult Hash(string password)
        {
            ArgumentNullException.ThrowIfNull(password);

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return new PasswordHashResult(Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

        public bool Verify(string password, string hash, string salt)
        {
            if (password is null || string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
            {
                return false;
            }

            byte[] saltBytes;
            byte[] expected;
            try
            {
                saltBytes = Convert.FromBase64String(salt);
                expected = Convert.FromBase64String(hash);
            }
            catch (FormatException)
            {
                return false;
            }

            if (expected.Length != HashSize)
            {
                return false;
            }

            var actual = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, HashSize);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
