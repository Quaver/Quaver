// Copyright (c)  Swan. All rights reserved.  
// See the Copyright notice in the root of the project.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using UnityEngine;
using Quaver.Utils;

namespace Quaver.Config
{
    public class ConfigParser
    {
        public static Cfg Parse(string filePath)
        {
            if (!File.Exists(filePath.Trim()))
            {
                Cfg tempCfg = new Cfg();
                tempCfg.IsValid = false;

                return tempCfg;
            }

            Cfg cfg = new Cfg();
            cfg.IsValid = true;


            foreach (string line in File.ReadAllLines(filePath))
            {
                // Parse the config values, also we need to have defaults just in case some dont exist. and update the file.
                if (line.Contains("="))
                {
                    string key = line.Substring(0, line.IndexOf('=')).Trim();
                    string value = line.Split('=').Last().Trim();

                    switch (key)
                    {
                        case "SongDirectory":
                            ParseDirectory(out cfg.SongDirectory, value, ConfigDefault.SongDirectory);
                            break;
                        case "SkinsDirectory":
                            ParseDirectory(out cfg.SkinsDirectory, value, ConfigDefault.SkinsDirectory);
                            break;
                        case "ScreenshotsDirectory":
                            ParseDirectory(out cfg.ScreenshotsDirectory, value, ConfigDefault.ScreenshotsDirectory);
                            break;
                        case "ReplaysDirectory":
                            ParseDirectory(out cfg.ReplaysDirectory, value, ConfigDefault.ReplaysDirectory);
                            break;
                        case "LogsDirectory":
                            ParseDirectory(out cfg.LogsDirectory, value, ConfigDefault.LogsDirectory);
                            break;
                        case "VolumeGlobal":
                            ParseByte(out cfg.VolumeGlobal, value, ConfigDefault.VolumeGlobal);
                            break;
                        case "VolumeEffect":
                            ParseByte(out cfg.VolumeEffect, value, ConfigDefault.VolumeEffect);
                            break;
                        case "VolumeMusic":
                            ParseByte(out cfg.VolumeMusic, value, ConfigDefault.VolumeMusic);
                            break;
                        case "BackgroundDim":
                            ParseByte(out cfg.BackgroundDim, value, ConfigDefault.BackgroundDim);
                            break;
                        case "WindowHeight":
                            ParseInt(out cfg.WindowHeight, value, ConfigDefault.WindowHeight);
                            break;
                        case "WindowWidth":
                            ParseInt(out cfg.WindowWidth, value, ConfigDefault.WindowWidth);
                            break;
                        case "WindowFullScreen":
                            ParseBool(out cfg.WindowFullScreen, value, ConfigDefault.WindowFullScreen);
                            break;
                        case "WindowLetterboxed":
                            ParseBool(out cfg.WindowLetterboxed, value, ConfigDefault.WindowLetterboxed);
                            break;
                        case "CustomFrameLimit":
                            ParseShort(out cfg.CustomFrameLimit, value, ConfigDefault.CustomFrameLimit);
                            break;
                        case "FPSCounter":
                            ParseBool(out cfg.FPSCounter, value, ConfigDefault.FPSCounter);
                            break;
                        case "FrameTimeDisplay":
                            ParseBool(out cfg.FrameTimeDisplay, value, ConfigDefault.FrameTimeDisplay);
                            break;
                        case "Language":
                            ParseString(out cfg.Language, value, ConfigDefault.Language);
                            break;
                        case "QuaverVersion":
                            ParseString(out cfg.QuaverVersion, value, ConfigDefault.QuaverVersion);
                            break;
                        case "QuaverBuildHash":
                            ParseString(out cfg.QuaverBuildHash, value, ConfigDefault.QuaverBuildHash);
                            break;
                        case "ScrollSpeed":
                            ParseByte(out cfg.ScrollSpeed, value, ConfigDefault.ScrollSpeed);
                            break;
                        case "ScrollSpeedBPMScale":
                            ParseBool(out cfg.ScrollSpeedBPMScale, value, ConfigDefault.ScrollSpeedBPMScale);
                            break;
                        case "DownScroll":
                            ParseBool(out cfg.DownScroll, value, ConfigDefault.DownScroll);
                            break;
                        case "GlobalOffset":
                            ParseByte(out cfg.GlobalOffset, value, ConfigDefault.GlobalOffset);
                            break;
                        case "LeaderboardVisible":
                            ParseBool(out cfg.LeaderboardVisible, value, ConfigDefault.LeaderboardVisible);
                            break;
                        case "Skin":
                            ParseSkin(out cfg.Skin, value, ConfigDefault.Skin);
                            break;
                        case "KeyManiaLane1":
                            ParseKeyCode(out cfg.KeyLaneMania1, value, ConfigDefault.KeyLaneMania1);
                            break;
                        case "KeyManiaLane2":
                            ParseKeyCode(out cfg.KeyLaneMania2, value, ConfigDefault.KeyLaneMania2);
                            break;
                        case "KeyManiaLane3":
                            ParseKeyCode(out cfg.KeyLaneMania3, value, ConfigDefault.KeyLaneMania3);
                            break;
                        case "KeyManiaLane4":
                            ParseKeyCode(out cfg.KeyLaneMania4, value, ConfigDefault.KeyLaneMania4);
                            break;
                        case "KeyScreenshot":
                            ParseKeyCode(out cfg.KeyScreenshot, value, ConfigDefault.KeyScreenshot);
                            break;
                        case "KeyQuickRetry":
                            ParseKeyCode(out cfg.KeyQuickRetry, value, ConfigDefault.KeyQuickRetry);
                            break;
                        case "KeyIncreaseScrollSpeed":
                            ParseKeyCode(out cfg.KeyIncreaseScrollSpeed, value, ConfigDefault.KeyIncreaseScrollSpeed);
                            break;
                        case "KeyDecreaseScrollSpeed":
                            ParseKeyCode(out cfg.KeyDecreaseScrollSpeed, value, ConfigDefault.KeyDecreaseScrollSpeed);
                            break;
                        case "KeyPause":
                            ParseKeyCode(out cfg.KeyPause, value, ConfigDefault.KeyPause);
                            break;
                        case "KeyVolumeUp":
                            ParseKeyCode(out cfg.KeyVolumeUp, value, ConfigDefault.KeyVolumeUp);
                            break;
                        case "KeyVolumeDown":
                            ParseKeyCode(out cfg.KeyVolumeDown, value, ConfigDefault.KeyVolumeDown);
                            break;
                        case "TimingBars":
                            ParseBool(out cfg.TimingBars, value, ConfigDefault.TimingBars);
                            break;
                    }
                }
            }

            ConfigUpdater.Update(cfg);
            return cfg;
        }


        // Responsible for correctly parsing a directory and setting it's default value if it isn't valid.
        // If the directory doesn't exist, then it will create it at the default value.
        private static void ParseDirectory(out string valueHolder, string valueToParse, string defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse) || !Directory.Exists(valueToParse))
            {
                valueHolder = defaultValue;
                Directory.CreateDirectory(defaultValue);
                Debug.LogWarning("[CONFIG] Invalid or non-existant directory detected. Updating value & creating directory.");
                return;
            }

            valueHolder = valueToParse;
            return;
        }

        // Responsible for taking a byte configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseByte(out byte valueHolder, string valueToParse, byte defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse))
            {
                valueHolder = defaultValue;
                Debug.LogWarning("[CONFIG] Invalid Byte value: " + valueToParse + " detected in config. Updating to default value.");
                return;
            }

            valueHolder = byte.Parse(valueToParse);
            return;
        }

        // Responsible for taking a int configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseInt(out int valueHolder, string valueToParse, int defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse))
            {
                valueHolder = defaultValue;
                Debug.LogWarning("[CONFIG] Invalid int value: " + valueToParse + " detected in config. Updating to default value.");
                return;
            }

            valueHolder = Int32.Parse(valueToParse);
            return;
        }


        // Responsible for taking a short configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseShort(out short valueHolder, string valueToParse, short defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse))
            {
                valueHolder = defaultValue;
                Debug.LogWarning("[CONFIG] Invalid short value: " + valueToParse + " detected in config. Updating to default value.");
                return;
            }

            valueHolder = short.Parse(valueToParse);
            return;
        }


        // Responsible for taking a bool configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseBool(out bool valueHolder, string valueToParse, bool defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse))
            {
                valueHolder = defaultValue;
                Debug.LogWarning("[CONFIG] Invalid bool value: " + valueToParse + " detected in config. Updating to default value.");
                return;
            }

            valueHolder = bool.Parse(valueToParse);
            return;
        }

        // Responsible for taking a string configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseString(out string valueHolder, string valueToParse, string defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse))
            {
                valueHolder = defaultValue;
                Debug.LogWarning("[CONFIG] Invalid string value: " + valueToParse + " detected in config. Updating to default value.");
                return;
            }

            valueHolder = valueToParse;
            return;
        }

        // Responsible for taking a skin string configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseSkin(out string valueHolder, string valueToParse, string defaultValue)
        {
            if (Strings.IsNullOrEmptyOrWhiteSpace(valueToParse) || !Directory.Exists(Application.dataPath + "/" + valueToParse))
            {
                valueHolder = defaultValue;
                Debug.LogWarning("[CONFIG] Invalid skin value detected in config, resorting to default skin.");
                return;
            }

            valueHolder = valueToParse;
            return;
        }

        // Responsible for taking a KeyCode string configuration value, validating it, and setting that value
        // in our Cfg instance.
        private static void ParseKeyCode(out KeyCode valueHolder, string valueToParse, KeyCode defaultValue)
        {
            try
            {
                KeyCode key = (KeyCode)Enum.Parse(typeof(KeyCode), valueToParse);
                valueHolder = key;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Debug.LogWarning("[CONFIG] Invalid KeyCode value: " + valueToParse + " detected in config. Updating to default value.");
                valueHolder = defaultValue;
            }

            return;
        }
    }
}