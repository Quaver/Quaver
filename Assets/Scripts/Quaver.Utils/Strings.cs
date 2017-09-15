// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        // Checks a string if it has any null or white space
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

        // Checks if a string is null, empty, or has whitespace.
        public static bool IsNullOrEmptyOrWhiteSpace(string value)
        {
            return string.IsNullOrEmpty(value) ||
                ReferenceEquals(value, null) ||
                    string.IsNullOrEmpty(value.Trim());
        }

        // Remove the last word from a string
        public static string RemoveLastWord(string value)
        {
            if (value == null) return ""; // Only if any

            string result = value;
            int index = value.LastIndexOf(" ");
            if (index > -1) result = value.Remove(index);
            return result;
        }

        // Gets the MD5 Hash of a string.
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

