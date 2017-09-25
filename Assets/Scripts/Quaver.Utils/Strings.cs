using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;

namespace Quaver.Utils
{
    public static class Strings
    {
        /// <summary>
        /// Checks if a string is null or has any whitespace
        /// </summary>
        /// <param name="value">The value you're checking</param>
        /// <returns>Boolean</returns>
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if a string is null, empty or has any whitespace
        /// </summary>
        /// <param name="value">The value you're checking</param>
        /// <returns>Boolean</returns>   
        public static bool IsNullOrEmptyOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) ||
                ReferenceEquals(value, null) ||
                    string.IsNullOrEmpty(value.Trim());
        }

        /// <summary>
        /// Removes the last word in a string
        /// </summary>
        /// <param name="value">The string you want to move the last word from/</param>
        /// <returns>The new string w/ the last word taken out.</returns>
        public static string RemoveLastWord(string value)
        {
            if (value == null) return ""; // Only if any

            string result = value;
            int index = value.LastIndexOf(" ");
            if (index > -1) result = value.Remove(index);
            return result;
        }

        /// <summary>
        /// Converts a string to an MD5 Hash
        /// </summary>
        /// <param name="TextToHash">The string</param>
        /// <returns>MD5 Hashed version of the string.</returns>
        public static String GetMD5Hash(String TextToHash)
        {
            //Check wether data was passed
            if ((TextToHash == null) || (TextToHash.Length == 0))
            {
                return String.Empty;
            }

            //Calculate MD5 hash. This requires that the string is splitted into a byte[].
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] textToHash = Encoding.Default.GetBytes(TextToHash);
            byte[] result = md5.ComputeHash(textToHash);

            //Convert result back to string.
            return System.BitConverter.ToString(result);
        }
    }
}

