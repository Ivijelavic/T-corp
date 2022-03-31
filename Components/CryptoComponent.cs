using System;
using System.Security.Cryptography;
using System.Text;
using TCorp.EntityFramework;

namespace TCorp.Components {
    /// <summary>
    /// Handles crypto logic. Best not mess here.
    /// </summary>
    public class CryptoComponent {
        private const int SALT_LENGTH = 32;

        /// <summary>
        /// Returns a hash of the provided string using SHA-512 algorithm. 
        /// </summary>
        /// <param name="value">The string being hashed</param>
        /// <returns>The hash</returns>
        private string SHA512(string value) {
            var hash = System.Security.Cryptography.SHA512.Create();
            var encoder = new System.Text.ASCIIEncoding();
            var combined = encoder.GetBytes(value ?? "");
            return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
        }

        /// <summary>
        /// Performs a SHA-512 hash
        /// </summary>
        /// <param name="input">The string to be hased</param>
        /// <returns>Returns a string of length 128 characters</returns>
        public string Hash(string input) {
            return SHA512(input);
        }

        /// <summary>
        /// Applies a salt to the string and
        /// performs a SHA-512 hash
        /// </summary>
        /// <param name="input">The string to be hased</param>
        /// <param name="salt">The salt</param>
        /// <returns>Returns a string of length 128 characters</returns>
        public string Hash(string input, string salt) {
            return SHA512(salt + input);
        }

        /// <summary>
        /// Generates a random access token based on the current time,
        /// user credentials and a random secure string.
        /// </summary>
        /// <param name="user">The user requesting the token</param>
        /// <returns></returns>
        public string GenerateAccessToken(User user) {
            if (user == null) throw new ArgumentException("User is null");
            string randomString = SecureString(128);

            DateTime now = DateTime.Now;
            string timestamp = now.ToString("dd.MM.yyyy HH:mm:ss");

            string token = Hash(randomString + user.Username + timestamp);
            return token;
        }

        /// <summary>
        /// Private function that generates a random string of variable length
        /// using the RNGCryptoServiceProvider class.
        /// </summary>
        /// <param name="length">The length of the random string</param>
        /// <returns>A random string of the given length</returns>
        private string SecureString(int length) {
            string charsToUse = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            char[] chars = charsToUse.ToCharArray();
            int size = length;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = length;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data) {
                result.Append(chars[b % (chars.Length - 1)]);
            }
            return result.ToString();
        }

        public string GenerateSalt() {
            string salt = SecureString(SALT_LENGTH);
            return salt;
        }
    }
}