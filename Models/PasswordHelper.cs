using System;
using System.Security.Cryptography;
using System.Text;

namespace MinesweeperApp.Models
{
    // This helper class creates salted password hashes
    // and verifies passwords during login.
    public static class PasswordHelper
    {
        // Creates a random salt and returns it as a Base64 string
        public static string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }

        // Combines password + salt and returns a SHA256 hash as a Base64 string
        public static string HashPassword(string password, string salt)
        {
            string saltedPassword = password + salt;
            byte[] saltedPasswordBytes = Encoding.UTF8.GetBytes(saltedPassword);

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(saltedPasswordBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Verifies whether the entered password matches the stored hash
        public static bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            string enteredHash = HashPassword(enteredPassword, storedSalt);
            return enteredHash == storedHash;
        }
    }
}
