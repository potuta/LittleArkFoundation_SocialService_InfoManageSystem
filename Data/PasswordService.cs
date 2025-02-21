using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace LittleArkFoundation.Data
{
    public class PasswordService
    {

        //Password hashing and verification using PBKDF2-SHA256
        public static string HashPassword(string password, byte[] salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,  // More iterations = stronger security
                numBytesRequested: 32   // 256-bit hash
            ));
        }

        public static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16]; // 16-byte salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash, byte[] salt)
        {
            string hash = HashPassword(enteredPassword, salt);
            return hash == storedHash;
        }
    }
}
