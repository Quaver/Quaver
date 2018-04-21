﻿using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.Config
{
    /// <summary>
    /// Helper class for any methods that are used for reading config files.
    /// </summary>
    internal static class ConfigHelper
    {
        /// <summary>
        ///     Reads a string and checks if it's a valid directory.
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static string ReadDirectory(string defaultDir, string newDir)
        {
            // If the config specified directory already exists, then we'll return 
            // the new directory's value, otherwise we'll make sure a default directory 
            // is created and return the original default value.
            if (Directory.Exists(newDir))
                return newDir;

            Directory.CreateDirectory(defaultDir);
            return defaultDir;
        }

        /// <summary>
        ///     Purely responsible for reading percentage data. We don't want these values
        ///     to be greater than 100.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static byte ReadPercentage(byte defaultVal, string newVal)
        {
            // Try to parse the byte value, if the value is greater than 100 (%),
            // return the default percentage.
            try
            {
                var newPercentage = byte.Parse(newVal);
                return newPercentage > 100 ? defaultVal : newPercentage;
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }

        /// <summary>
        ///     Responsible for reading any Int32
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static int ReadInt32(int defaultVal, string newVal)
        {
            try
            {
                return int.Parse(newVal);
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }

        /// <summary>
        ///     Responsible for reading float values
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static float ReadFloat(float defaultVal, string newVal)
        {
            try
            {
                return float.Parse(newVal);
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }
    
        /// <summary>
        ///     Responsible for reading boolean values from the config file.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static bool ReadBool(bool defaultVal, string newVal)
        {
            try
            {
                return bool.Parse(newVal);
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }

        /// <summary>
        ///     Reads string values from the config file
        ///     TODO: Change for Language!, Do validation here.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static string ReadString(string defaultVal, string newVal)
        {
            return string.IsNullOrWhiteSpace(newVal) ? defaultVal : newVal;
        }

        /// <summary>
        ///     Reads Int16 values from the config file.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static short ReadInt16(short defaultVal, string newVal)
        {
            try
            {
                return short.Parse(newVal);
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }

        /// <summary>
        ///     Reads a byte from the config file
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static byte ReadByte(byte defaultVal, string newVal)
        {
            try
            {
                return byte.Parse(newVal);
            }
            catch (Exception e)
            {
                return defaultVal;
            }
        }

        /// <summary>
        ///     Reads a signed byte from the config file
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static sbyte ReadSignedByte(sbyte defaultVal, string newVal)
        {
            try
            {
                return sbyte.Parse(newVal);
            }
            catch (Exception e)
            {
                return defaultVal;
            }   
        }

        /// <summary>
        ///     Reads the skin value from the config file.
        /// </summary>
        /// <param name="defaultSkin"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static string ReadSkin(string defaultSkin, string newVal)
        {
            return Directory.Exists(ConfigManager.SkinDirectory + "/" + newVal) ? newVal : defaultSkin;
        }

        /// <summary>
        ///     Responsible for reading a path from config
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static string ReadPath(string defaultPath, string newVal)
        {
            return File.Exists(newVal) ? newVal : defaultPath;
        }

        /// <summary>
        ///     Reads an XNA Key value from a string.
        /// </summary>
        /// <param name="defaultKey"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static Keys ReadKeys(Keys defaultKey, string newVal)
        {
            return Enum.TryParse(newVal, out Keys newKey) ? newKey : defaultKey;
        }

        /// <summary>
        ///     Reads an XNA Color value from a string
        /// </summary>
        /// <param name="defaultColor"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static Color ReadColor(Color defaultColor, string newVal)
        {
            try
            {
                string[] colorSplit = newVal.Split(',');
                return new Color(byte.Parse(colorSplit[0]), byte.Parse(colorSplit[1]), byte.Parse(colorSplit[2]));
            }
            catch (Exception e)
            {
                return defaultColor;
            }
        }

        /// <summary>
        ///     Reads a DefaultSkin 
        /// </summary>
        /// <param name="defaultSkins"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        internal static DefaultSkins ReadDefaultSkin(DefaultSkins defaultSkins, string newVal)
        {
            return Enum.TryParse(newVal, out DefaultSkins newDefault) ? newDefault : defaultSkins;
        }
    }
}
