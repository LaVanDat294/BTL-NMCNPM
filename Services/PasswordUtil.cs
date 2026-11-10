using System.Security.Cryptography;

namespace NMCNPM.Services
{
    public static class PasswordUtil
    {
        // Format: PBKDF2$iterations$base64Salt$base64Hash
        public static string Hash(string password)
        {
            const int iterations = 100000;
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, 32);
            return $"PBKDF2${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(password)) 
                return false;

            // Plain-text fallback for backward compatibility or legacy test accounts
            if (stored == password) 
                return true;

            try
            {
                var p = stored.Split('$');
                if (p.Length != 4 || p[0] != "PBKDF2") 
                    return false;

                int iterations = int.Parse(p[1]);
                byte[] salt = Convert.FromBase64String(p[2]);
                byte[] expected = Convert.FromBase64String(p[3]);
                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch 
            { 
                return false; 
            }
        }
    }
}
