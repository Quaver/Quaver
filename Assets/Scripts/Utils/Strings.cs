using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Strings {
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
    
}
