using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Main;
using SQLitePCL;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Config
{
    internal static class ConfigManager
    {        
        /// <summary>
        ///     These are all values that should never ben
        /// </summary>
         private static string _gameDirectory;
        internal static BindedValue<string> GameDirectory { get; set; }

        /// <summary>
        ///     The skin directory
        /// </summary>
        private static string _skinDirectory;
        internal static BindedValue<string> SkinDirectory { get; set; }

        /// <summary>
        ///     The screenshot directory
        /// </summary>
        private static string _screenshotDirectory;
        internal static BindedValue<string> ScreenshotDirectory { get; set; }

        /// <summary>
        ///     The replay directory
        /// </summary>
        private static string _replayDirectory;
        internal static BindedValue<string> ReplayDirectory { get; set; }

        /// <summary>
        ///     The Logs directory
        /// </summary>
        private static string _logsDirectory;
        internal static BindedValue<string> LogsDirectory { get; set; }

        /// <summary>
        ///     The data directory
        /// </summary>
        private static string _dataDirectory;
        internal static BindedValue<string> DataDirectory { get; set; }

        /// <summary>
        ///     The song directory
        /// </summary>
        private static string _songDirectory;
        internal static BindedValue<string> SongDirectory { get; set; }

        /// <summary>
        ///     The username of the user.
        /// </summary>
        internal static BindedValue<string> Username { get; set; }

        /// <summary>
        ///     The skin in the Skins directory that is loaded. Default is the only exception, as it'll be overrided.
        /// </summary>
        internal static BindedValue<string> Skin { get; set; }

        /// <summary>
        ///     The default skin that will be loaded if the skin property is blank
        /// </summary>
        internal static BindedValue<DefaultSkins> DefaultSkin { get; set; }

        /// <summary>
        ///     The master volume of the game.
        /// </summary>
        internal static BindedInt VolumeGlobal { get; set; }

        /// <summary>
        ///     The SFX volume of the game.
        /// </summary>
        internal static BindedInt VolumeEffect { get; set; }

        /// <summary>
        ///     The Music volume of the gamne.
        /// </summary>
        internal static BindedInt VolumeMusic { get; set; }

        /// <summary>
        ///     The dim for backgrounds during gameplay
        /// </summary>
        internal static BindedInt BackgroundBrightness { get; set; }


        /// <summary>
        ///     The height of the window.
        /// </summary>
        internal static BindedInt WindowHeight { get; set; }

        /// <summary>
        ///     The width of the window.
        /// </summary>
        internal static BindedInt WindowWidth { get; set; }

        /// <summary>
        ///     4k Hit Position offset from receptor
        /// </summary>
        internal static BindedInt UserHitPositionOffset4k { get; set; }

        /// <summary>
        ///     7k Hit Position offset from receptor
        /// </summary>
        internal static BindedInt UserHitPositionOffset7k { get; set; }

        /// <summary>
        ///     Is the window fullscreen?
        /// </summary>
        internal static BindedValue<bool> WindowFullScreen { get; set; }

        /// <summary>
        ///     Is the window letterboxed?
        /// </summary>
        internal static BindedValue<bool> WindowLetterboxed { get; set; }

        /// <summary>
        ///     Should the game display the FPS Counter?
        /// </summary>
        internal static BindedValue<bool> FpsCounter { get; set; }

        /// <summary>
        ///     Determines if the health bar + multiplier is at top or bottom of the playfield
        /// </summary>
        internal static BindedValue<bool> HealthBarPositionTop { get; set; }

        /// <summary>
        ///     The scroll speed for mania 4k
        /// </summary>
        internal static BindedInt ScrollSpeed4k { get; set; }

        /// <summary>
        ///     The scroll speed for mania 7k
        /// </summary>
        internal static BindedInt ScrollSpeed7k { get; set; }

        /// <summary>
        ///     Should 4k be played with DownScroll? If false, it's UpScroll
        /// </summary>
        internal static BindedValue<bool> DownScroll4k { get; set; }

        /// <summary>
        ///     Should 7k be played with DownScroll? If false, it's UpScroll
        /// </summary>
        internal static BindedValue<bool> DownScroll7k { get; set; }

        /// <summary>
        ///     The offset of the notes compared to the song start.
        /// </summary>
        internal static BindedInt GlobalAudioOffset { get; set; }

        /// <summary>
        ///     Dictates whether or not the song audio is pitched while using the ManiaModSpeed gameplayModifier.
        /// </summary>
        internal static BindedValue<bool> Pitched { get; set; }

        /// <summary>
        ///     The path of the osu!.db file
        /// </summary>
        internal static BindedValue<string> OsuDbPath { get; set; } 

        /// <summary>
        ///     Dictates where or not we should load osu! maps from osu!.db on game start
        /// </summary>
        internal static BindedValue<bool> AutoLoadOsuBeatmaps { get; set; }

        /// <summary>
        ///     The path of the Etterna cache folder
        ///     NOTE: Usually located at C:\Games\Etterna\Cache\Songs
        /// </summary>
        internal static BindedValue<string> EtternaCacheFolderPath { get; set; }

        /// <summary>
        ///     Dictates whether or not the game will be loaded with all of the Etterna maps
        /// </summary>
        internal static BindedValue<bool> AutoLoadEtternaCharts { get; set; }

        /// <summary>
        ///     Keybindings for 4K
        /// </summary>
        internal static BindedValue<Keys> KeyMania4k1 { get; set; }
        internal static BindedValue<Keys> KeyMania4k2 { get; set; }
        internal static BindedValue<Keys> KeyMania4k3 { get; set; }
        internal static BindedValue<Keys> KeyMania4k4 { get; set; }

        /// <summary>
        ///     Keybindings for 7K
        /// </summary>
        internal static BindedValue<Keys> KeyMania7k1 { get; set; }
        internal static BindedValue<Keys> KeyMania7k2 { get; set; }
        internal static BindedValue<Keys> KeyMania7k3 { get; set; }
        internal static BindedValue<Keys> KeyMania7k4 { get; set; }
        internal static BindedValue<Keys> KeyMania7k5 { get; set; }
        internal static BindedValue<Keys> KeyMania7k6 { get; set; }
        internal static BindedValue<Keys> KeyMania7k7 { get; set; }


        /// <summary>
        ///     The key pressed to pause and menu-back.
        /// </summary>
        internal static BindedValue<Keys> KeyPause { get; set; }

        /// <summary>
        ///     The key pressed to skip the song introduction
        /// </summary>
        internal static BindedValue<Keys> KeySkipIntro { get; set; }

        /// <summary>
        ///     The key to take a screenshot of the game window.
        /// </summary>
        internal static BindedValue<Keys> KeyTakeScreenshot { get; set; }

        /// <summary>
        ///     The key to toggle the overlay
        /// </summary>
        internal static BindedValue<Keys> KeyToggleOverlay { get; set; }

        /// <summary>
        ///     Dictates whether or not this is the first write of the file for the current game session.
        ///     (Not saved in Config)
        /// </summary>
        private static bool FirstWrite { get; set; }

        /// <summary>
        ///     Important!
        ///     Responsible for initializing directory properties,
        ///     writing a new config file if it doesn't exist and also reading config files.
        ///     This should be the one of the first things that is called upon game launch.
        /// </summary>
        internal static void InitializeConfig()
        {
            // When initializing, we manually set the directory fields rather than the props,
            // because we only want to write the config file one time at this stage.
            // Usually when a property is modified, it will automatically write the config file again,
            // so that's what we're preventing here.
            _gameDirectory = Directory.GetCurrentDirectory();

            _skinDirectory = _gameDirectory + "/Skins";
            Directory.CreateDirectory(_skinDirectory);

            _screenshotDirectory = _gameDirectory + "/Screenshots";
            Directory.CreateDirectory(_screenshotDirectory);

            _logsDirectory = _gameDirectory + "/Logs";
            Directory.CreateDirectory(_logsDirectory);

            _replayDirectory = _gameDirectory + "/Replays";
            Directory.CreateDirectory(_replayDirectory);

            _dataDirectory = _gameDirectory + "/Data";
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_dataDirectory + "/r/");

            _songDirectory = _gameDirectory + "/Songs";
            Directory.CreateDirectory(_songDirectory);

            // If we already have a config file, we'll just want to read that.
            ReadConfigFile();
    
            // Create log files after reading config.
            Logger.CreateLogFile();
            
            Logger.LogSuccess("Config file has successfully been read.", LogType.Runtime);
        }

        /// <summary>
        ///     Reads a quaver.cfg file and sets all of the successfully read values.
        ///     At the end of reading, we write the config file, changing any invalid data/
        /// </summary>
        private static void ReadConfigFile()
        {
            // We'll want to write a quaver.cfg file if it doesn't already exist.
            // There's no need to read the config file afterwards, since we already have 
            // all of the default values.
            if (!File.Exists(_gameDirectory + "/quaver.cfg"))
                File.WriteAllText(_gameDirectory + "/quaver.cfg", "; Quaver Configiration File");
            
            var data = new FileIniDataParser().ReadFile(_gameDirectory + "/quaver.cfg")["Config"];

            // Read / Set Config Values
            GameDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"GameDirectory", _gameDirectory, data);
            SkinDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SkinDirectory", _skinDirectory, data);
            ScreenshotDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"ScreenshotDirectory", _screenshotDirectory, data);
            ReplayDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"ReplayDirectory", _replayDirectory, data);
            LogsDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"LogsDirectory", _logsDirectory, data);
            DataDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"DataDirectory", _dataDirectory, data);
            SongDirectory = ReadSpecialConfigType(SpecialConfigType.Directory, @"SongDirectory", _songDirectory, data);
            OsuDbPath = ReadSpecialConfigType(SpecialConfigType.Path, @"OsuDbPath", "", data);
            AutoLoadOsuBeatmaps = ReadValue(@"AutoLoadOsuBeatmaps", false, data);
            EtternaCacheFolderPath = ReadSpecialConfigType(SpecialConfigType.Path, @"EtternaCacheFolderPath", "", data);
            AutoLoadEtternaCharts = ReadValue(@"AutoLoadEtternaCharts", false, data);
            Username = ReadValue(@"Username", "", data);
            VolumeGlobal = ReadInt(@"VolumeGlobal", 50, 0, 100, data);
            VolumeEffect = ReadInt(@"VolumeEffect", 80, 0, 100, data);
            VolumeMusic = ReadInt(@"VolumeMusic", 50, 0, 100, data);
            BackgroundBrightness = ReadInt(@"BackgroundBrightness", 30, 0, 100, data);
            WindowHeight = ReadInt(@"WindowHeight", 720, 600, short.MaxValue, data);
            WindowWidth = ReadInt(@"WindowWidth", 1280, 800, short.MaxValue, data);
            HealthBarPositionTop = ReadValue(@"HealthBarPositionTop", false, data);
            UserHitPositionOffset4k = ReadInt(@"UserHitPositionOffset4k", 0, 0, byte.MaxValue, data);
            UserHitPositionOffset7k = ReadInt(@"UserHitPositionOffset7k", 0, 0, byte.MaxValue, data);
            WindowFullScreen = ReadValue(@"WindowFullScreen", false, data);
            WindowLetterboxed = ReadValue(@"WindowLetterboxed", false, data);
            FpsCounter = ReadValue(@"FpsCounter", true, data);
            ScrollSpeed4k = ReadInt(@"ScrollSpeed4k", 15, 1, 60, data);
            ScrollSpeed7k = ReadInt(@"ScrollSpeed7k", 15, 1, 60, data);
            DownScroll4k = ReadValue(@"DownScroll4k", true, data);
            DownScroll7k = ReadValue(@"DownScroll7k", true, data);
            GlobalAudioOffset = ReadInt(@"GlobalAudioOffset", 0, 0, byte.MaxValue, data);
            Skin = ReadSpecialConfigType(SpecialConfigType.Skin, @"Skin", "", data);
            DefaultSkin = ReadValue(@"DefaultSkin", DefaultSkins.Arrow, data);
            Pitched = ReadValue(@"Pitched", false, data);
            KeyMania4k1 = ReadValue(@"KeyMania4k1", Keys.A, data);
            KeyMania4k2 = ReadValue(@"KeyMania4k2", Keys.S, data);
            KeyMania4k3 = ReadValue(@"KeyMania4k3", Keys.K, data);
            KeyMania4k4 = ReadValue(@"KeyMania4k4", Keys.L, data);
            KeyMania7k1 = ReadValue(@"KeyMania7k1", Keys.A, data);
            KeyMania7k2 = ReadValue(@"KeyMania7k2", Keys.S, data);
            KeyMania7k3 = ReadValue(@"KeyMania7k3", Keys.D, data);
            KeyMania7k4 = ReadValue(@"KeyMania7k4", Keys.Space, data);
            KeyMania7k5 = ReadValue(@"KeyMania7k5", Keys.J, data);
            KeyMania7k6 = ReadValue(@"KeyMania7k6", Keys.K, data);
            KeyMania7k7 = ReadValue(@"KeyMania7k7", Keys.L, data);
            KeySkipIntro = ReadValue(@"KeySkipIntro", Keys.RightAlt, data);
            KeyPause = ReadValue(@"KeyPause", Keys.Escape, data);
            KeyTakeScreenshot = ReadValue(@"KeyTakeScreenshot", Keys.F12, data);
            KeyToggleOverlay = ReadValue(@"KeyToggleOverlay", Keys.F8, data);

            // Set Master and Sound Effect Volume
            SoundEffect.MasterVolume = VolumeGlobal.Value / 100f;
            GameBase.AudioEngine.MasterVolume = VolumeGlobal.Value;
            GameBase.AudioEngine.MusicVolume = VolumeMusic.Value;
            
            // Write the config file with all of the changed/invalidated data.
            Task.Run(async () => await WriteConfigFileAsync());
        }

        /// <summary>
        ///     Reads a BindedValue<T>. Works on all types.
        /// </summary>
        /// <returns></returns>
        private static BindedValue<T> ReadValue<T>(string name, T defaultVal, KeyDataCollection ini)
        {
            var binded = new BindedValue<T>(name, defaultVal, AutoSaveConfiguration);
            var converter = TypeDescriptor.GetConverter(typeof(T));

            // Attempt to parse the value and default it if it can't.
            try
            {
                binded.Value = (T) converter.ConvertFromString(null, CultureInfo.InvariantCulture, ini[name]);
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }
            
            return binded;
        }

        /// <summary>
        ///     Reads an Int32 to a BindedInt
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultVal"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="ini"></param>
        /// <returns></returns>
        private static BindedInt ReadInt(string name, int defaultVal, int min, int max, KeyDataCollection ini)
        {
            var binded = new BindedInt(name, defaultVal, min, max, AutoSaveConfiguration);

            // Try to read the int.
            try
            {
                binded.Value = int.Parse(ini[name]);
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }

            return binded;
        }
        
        /// <summary>
        ///     Reads a special configuration string type. These values need to be read and written in a
        ///     certain way.
        /// </summary>
        /// <returns></returns>
        private static BindedValue<string> ReadSpecialConfigType(SpecialConfigType type, string name, string defaultVal, KeyDataCollection ini)
        {
             var binded = new BindedValue<string>(name, defaultVal, AutoSaveConfiguration);

            try
            {
                // Get parsed config value.
                var parsedVal = ini[name];
                
                switch (type)
                {
                    case SpecialConfigType.Directory:
                        if (Directory.Exists(parsedVal))
                            binded.Value = parsedVal;
                        else
                        {
                            // Make sure the default directory is created.
                            Directory.CreateDirectory(defaultVal);
                            throw new ArgumentException();
                        }
                        break;
                    case SpecialConfigType.Path:
                        if (File.Exists(parsedVal))
                            binded.Value = parsedVal;
                        else
                            throw new ArgumentException();
                        break;
                    case SpecialConfigType.Skin:
                        if (Directory.Exists(SkinDirectory + "/" + parsedVal))
                            binded.Value = parsedVal;
                        else
                            throw new ArgumentException();
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }
            catch (Exception e)
            {
                binded.Value = defaultVal;
            }

            return binded;
        }
        
        /// <summary>
        ///     Autosaves configuration for bindable bools.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="d"></param>
        private static void AutoSaveConfiguration<T>(object sender, BindedValueEventArgs<T> d)
        {
            Task.Run(async () => await WriteConfigFileAsync());
        }
        
        /// <summary>
        ///     Takes all of the current values from the ConfigManager class and creates a file with them.
        ///     This will automatically be called whenever a configuration value is changed in the code.
        /// </summary>
        private static async Task WriteConfigFileAsync()
        {
            // Tracks the number of attempts to write the file it has made. 
            var attempts = 0;

            // Don't do anything if the file isn't ready.
            while (!IsFileReady(GameDirectory + "/quaver.cfg") && !FirstWrite){}

            var sb = new StringBuilder();

            // Top file information
            sb.AppendLine("; Quaver Configuration File");
            sb.AppendLine("; Last Updated On: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine();
            sb.AppendLine("[Config]");
            sb.AppendLine("; Quaver Configuration Values");
            
            // For every line we want to append "PropName = PropValue" to the string
            foreach (var p in typeof(ConfigManager)
                .GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            {
                // Don't include the FirstWrite Property
                if (p.Name == "FirstWrite" || p.Name == "BindedBoolStore")
                    continue;

                if (p.GetType() == typeof(BindedInt))
                {
                    Console.WriteLine("BINDED INT!!! ");
                }

                sb.AppendLine(p.Name + " = " + p.GetValue(null));
            }
               
            try
            {
                // Create a new stream 
                var sw = new StreamWriter(GameDirectory + "/quaver.cfg")
                {
                    AutoFlush = true
                };

                // Write to file and close it.
                await sw.WriteLineAsync(sb.ToString());
                sw.Close();

                FirstWrite = false;
            }
            catch (Exception e)
            {
                // Try to write the file again 3 times.
                while (attempts != 2)
                {
                    attempts++;

                    // Create a new stream 
                    var sw = new StreamWriter(GameDirectory + "/quaver.cfg")
                    {
                        AutoFlush = true
                    };

                    // Write to file and close it.
                    await sw.WriteLineAsync(sb.ToString());
                    sw.Close();
                }

                // If too many attempts were made.
                if (attempts == 2)
                    Logger.LogError("Too many attempts in a short time to write the config file have been made.", LogType.Runtime);
            }
        }

        /// <summary>
        ///     Checks if the file is ready to be written to.
        /// </summary>
        /// <param name="sFilename"></param>
        /// <returns></returns>
        private static bool IsFileReady(string sFilename)
        {
            // If the file can be opened for exclusive access it means that the file
            // is no longer locked by another process.
            try
            {
                using (var inputStream = File.Open(sFilename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return (inputStream.Length > 0);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    /// <summary>
    ///     Enum containing special config types. We want to read and default these in
    ///     a very particular way.
    /// </summary>
    internal enum SpecialConfigType
    {
        Directory,
        Path,
        Skin
    }
    
    /// <summary>
    ///     Enum containing a number representation of the default skins we have available
    /// </summary>
    internal enum DefaultSkins
    {
        Bar,
        Arrow
    }
}