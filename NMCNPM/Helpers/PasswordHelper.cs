using System;
using System.Security.Cryptography;

namespace NMCNPM.Helpers
{
    public class PasswordHelper
    {
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        // Tao ma bam mat khau
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, Iterations,
                HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);

                return "PBKDF2$" + Iterations + "$" +
                    Convert.ToBase64String(salt) + "$" +
                    Convert.ToBase64String(hash);
            }
        }

        // Kiem tra mat khau
        public static bool VerifyPassword(
            string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(storedHash))
                return false;

            try
            {
                string[] parts = storedHash.Split('$');

                if (parts.Length != 4 ||
                    parts[0] != "PBKDF2")
                    return false;

                int iterations = int.Parse(parts[1]);

                if (iterations < 10000 ||
                    iterations > 1000000)
                    return false;

                byte[] salt =
                    Convert.FromBase64String(parts[2]);

                byte[] expectedHash =
                    Convert.FromBase64String(parts[3]);

                if (salt.Length != SaltSize ||
                    expectedHash.Length != HashSize)
                    return false;

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations,
                    HashAlgorithmName.SHA256))
                {
                    byte[] actualHash =
                        pbkdf2.GetBytes(HashSize);

                    return FixedTimeEquals(
                        actualHash, expectedHash);
                }
            }
            catch
            {
                return false;
            }
        }

        // So sanh hash an toan
        private static bool FixedTimeEquals(
            byte[] a, byte[] b)
        {
            if (a == null || b == null ||
                a.Length != b.Length)
                return false;

            int result = 0;

            for (int i = 0; i < a.Length; i++)
            {
                result |= a[i] ^ b[i];
            }

            return result == 0;
        }
    }
}