using System.Security.Cryptography;
using System.Text;

namespace Quaver.Helpers
{
    internal static class CryptoHelper
    {
        /// <summary>
        ///     MD5 hash a string.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string StringToMd5(string input)
        {
            // Use input string to calculate MD5 hash
            StringBuilder sb = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString().ToLower();
            }
        }
    }
}