using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace QuickDesk.Infrastructure.Services
{
    public interface IPasswordHasherService
    {
        public string HashPassword(string password);
        public bool VerifyPassword(string password, string hashedPassword);
    }

    public class PasswordHasherService : IPasswordHasherService
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 10000; // Number of iterations for PBKDF2
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}";
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
            {
                throw new FormatException("Invalid hashed password format.");
            }
            var salt = Convert.FromBase64String(parts[0]);
            var key = Convert.FromBase64String(parts[1]);
            var keyToCheck = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);
            return CryptographicOperations.FixedTimeEquals(keyToCheck, key);
        }
    }
}
