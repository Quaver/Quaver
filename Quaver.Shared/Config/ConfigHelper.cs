/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Config
{
    public static class ConfigHelper
    {
        /// <summary>
        ///     Reads a string and checks if it's a valid directory.
        /// </summary>
        /// <param name="defaultDir"></param>
        /// <param name="newDir"></param>
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
        internal static byte ReadPercentage(byte defaultVal, string newVal) =>
            byte.TryParse(newVal, out var newOne) && newOne <= 100 ? newOne : defaultVal;

        /// <summary>
        ///     Responsible for reading any Int32
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static int ReadInt32(int defaultVal, string newVal) =>
            int.TryParse(newVal, out var newOne) ? newOne : defaultVal;

        /// <summary>
        ///     Responsible for reading float values
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static float ReadFloat(float defaultVal, string newVal) =>
            float.TryParse(newVal, out var newOne) ? newOne : defaultVal;

        /// <summary>
        ///     Responsible for reading boolean values from the config file.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static bool ReadBool(bool defaultVal, string newVal) =>
            bool.TryParse(newVal, out var newOne) ? newOne : defaultVal;

        /// <summary>
        ///     Reads string values from the config file
        ///     TODO: Change for Language!, Do validation here.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static string ReadString(string defaultVal, string newVal) =>
            string.IsNullOrWhiteSpace(newVal) ? defaultVal : newVal;

        /// <summary>
        ///     Reads Int16 values from the config file.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static short ReadInt16(short defaultVal, string newVal) =>
            short.TryParse(newVal, out var newOne) ? newOne : defaultVal;

        /// <summary>
        ///     Reads a byte from the config file
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static byte ReadByte(byte defaultVal, string newVal) =>
            byte.TryParse(newVal, out var newOne) ? newOne : defaultVal;

        /// <summary>
        ///     Reads a signed byte from the config file
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static sbyte ReadSignedByte(sbyte defaultVal, string newVal) =>
            sbyte.TryParse(newVal, out var newOne) ? newOne : defaultVal;

        /// <summary>
        ///     Reads the skin value from the config file.
        /// </summary>
        /// <param name="defaultSkin"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static string ReadSkin(string defaultSkin, string newVal) =>
            Directory.Exists(ConfigManager.SkinDirectory + "/" + newVal) ? newVal : defaultSkin;

        /// <summary>
        ///     Responsible for reading a path from config
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static string ReadPath(string defaultPath, string newVal) =>
            File.Exists(newVal) ? newVal : defaultPath;

        /// <summary>
        ///     Reads an XNA Key value from a string.
        /// </summary>
        /// <param name="defaultKey"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static Keys ReadKeys(Keys defaultKey, string newVal) => Enum.TryParse(newVal, out Keys newKey) ? newKey : defaultKey;

        /// <summary>
        ///     Reads an XNA Color value from a string
        /// </summary>
        /// <param name="defaultColor"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static Color ReadColor(Color defaultColor, string newVal) =>
            new Drain<char>(newVal, ',') is var (tr, (tg, (tb, (ta, _)))) &&
            byte.TryParse(tr, out var r) &&
            byte.TryParse(tg, out var g) &&
            byte.TryParse(tb, out var b)
                ? new(r, g, b, byte.TryParse(ta, out var a) ? a : 255)
                : defaultColor;

        /// <summary>
        ///     Reads an XNA Vector2 from a string
        /// </summary>
        /// <param name="defaultVector2"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static Vector2 ReadVector2(Vector2 defaultVector2, string newVal) =>
            new Drain<char>(newVal, ',') is var (tx, (ty, _)) &&
            int.TryParse(tx, out var x) &&
            int.TryParse(ty, out var y)
                ? new(x, y)
                : defaultVector2;

        /// <summary>
        ///     Reads a ScalableVector2 from a string
        /// </summary>
        /// <param name="defaultVector2"></param>
        /// <param name="newVal"></param>
        /// <returns></returns>
        internal static ScalableVector2? ReadVector2(ScalableVector2 defaultVector2, string newVal) =>
            new Drain<char>(newVal, ',') is var (tx, (ty, _)) &&
            int.TryParse(tx, out var x) &&
            int.TryParse(ty, out var y)
                ? new(x, y)
                : defaultVector2;

        /// <summary>
        ///     Reads an enum.
        /// </summary>
        /// <param name="defaultVal"></param>
        /// <param name="newVal"></param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns></returns>
        internal static TEnum ReadEnum<TEnum>(TEnum defaultVal, string newVal)
            where TEnum : struct =>
            Enum.TryParse(newVal, out TEnum newOne) ? newOne : defaultVal;
    }
}
