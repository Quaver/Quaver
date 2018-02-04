using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Logging;

namespace Quaver.Config
{
    internal class Configuration
    {
        /// <summary>
        ///     Dictates whether or not this is the first write of the file for the current game session.
        ///     (Not saved in Config)
        /// </summary>
        private static bool FirstWrite { get; set; }

        /// <summary>
        ///     These are all values that should never ben
        /// </summary>
        private static string _gameDirectory;
        internal static string GameDirectory { get => _gameDirectory; set { _gameDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The skin directory
        /// </summary>
        private static string _skinDirectory;
        internal static string SkinDirectory { get => _skinDirectory; set { _skinDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The screenshot directory
        /// </summary>
        private static string _screenshotDirectory;
        internal static string ScreenshotDirectory { get => _screenshotDirectory; set { _screenshotDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The replay directory
        /// </summary>
        private static string _replayDirectory;
        internal static string ReplayDirectory { get => _replayDirectory; set { _replayDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The Logs directory
        /// </summary>
        private static string _logsDirectory;
        internal static string LogsDirectory { get => _logsDirectory; set { _logsDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The data directory
        /// </summary>
        private static string _dataDirectory;
        internal static string DataDirectory { get => _dataDirectory; set { _dataDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The song directory
        /// </summary>
        private static string _songDirectory;
        internal static string SongDirectory { get => _songDirectory; set { _songDirectory = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The username of the user.
        /// </summary>
        private static string _username = "";
        internal static string Username { get => _username; set { _username = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The skin in the Skins directory that is loaded. Default is the only exception, as it'll be overrided.
        /// </summary>
        private static string _skin = "";
        internal static string Skin { get => _skin; set { _skin = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The default skin that will be loaded if the skin property is blank
        /// </summary>
        private static DefaultSkins _defaultSkin = DefaultSkins.Arrow;
        internal static DefaultSkins DefaultSkin { get => _defaultSkin; set { _defaultSkin = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The master volume of the game.
        /// </summary>
        private static byte _volumeGlobal = 100;
        internal static byte VolumeGlobal { get => _volumeGlobal; set { _volumeGlobal = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The SFX volume of the game.
        /// </summary>
        private static byte _volumeEffect = 100;
        internal static byte VolumeEffect { get => _volumeEffect; set { _volumeEffect = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The Music volume of the gamne.
        /// </summary>
        private static byte _volumeMusic = 30;
        internal static byte VolumeMusic{ get => _volumeMusic; set { _volumeMusic = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The dim for backgrounds during gameplay
        /// </summary>
        private static byte _backgroundBrightness = 20;
        internal static byte BackgroundBrightness { get => _backgroundBrightness; set { _backgroundBrightness = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The height of the window.
        /// </summary>
        private static int _windowHeight = 900;
        internal static int WindowHeight { get => _windowHeight; set { _windowHeight = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The width of the window.
        /// </summary>
        private static int _windowWidth = 1600;
        internal static int WindowWidth { get => _windowWidth; set{ _windowWidth = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     4k Hit Position offset from receptor
        /// </summary>
        private static int _userHitPositionOffset4k = 0;
        internal static int UserHitPositionOffset4k { get => _userHitPositionOffset4k; set { _userHitPositionOffset4k = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     7k Hit Position offset from receptor
        /// </summary>
        private static int _userHitPositionOffset7k = 0;
        internal static int UserHitPositionOffset7k { get => _userHitPositionOffset7k; set { _userHitPositionOffset7k = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Is the window fullscreen?
        /// </summary>
        private static bool _windowFullScreen;
        internal static bool WindowFullScreen { get => _windowFullScreen; set { _windowFullScreen = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Is the window letterboxed?
        /// </summary>
        private static bool _windowLetterboxed;
        internal static bool WindowLetterboxed { get => _windowLetterboxed; set { _windowLetterboxed = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Should the game display the FPS Counter?
        /// </summary>
        private static bool _fpsCounter = true;
        internal static bool FpsCounter { get => _fpsCounter; set { _fpsCounter = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Determines if the health bar + multiplier is at top or bottom of the playfield
        /// </summary>
        private static bool _healthBarPositionTop = false;
        internal static bool HealthBarPositionTop { get => _healthBarPositionTop; set { _healthBarPositionTop = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The scroll speed for mania 4k
        /// </summary>
        private static byte _scrollSpeed4k = 20;
        internal static byte ScrollSpeed4k { get => _scrollSpeed4k; set { _scrollSpeed4k = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The scroll speed for mania 7k
        /// </summary>
        private static byte _scrollSpeed7k = 20;
        internal static byte ScrollSpeed7k { get => _scrollSpeed7k; set { _scrollSpeed7k = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Should 4k be played with DownScroll? If false, it's UpScroll
        /// </summary>
        private static bool _downScroll4k = true;
        internal static bool DownScroll4k { get => _downScroll4k; set { _downScroll4k = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Should 7k be played with DownScroll? If false, it's UpScroll
        /// </summary>
        private static bool _downScroll7k = true;
        internal static bool DownScroll7k { get => _downScroll7k; set { _downScroll7k = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The offset of the notes compared to the song start.
        /// </summary>
        private static sbyte _globalAudioOffset;
        internal static sbyte GlobalAudioOffset { get => _globalAudioOffset; set { _globalAudioOffset = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Dictates whether or not the song audio is pitched while using the Speed mod.
        /// </summary>
        private static bool _pitched;
        internal static bool Pitched { get => _pitched; set { _pitched = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Toggle to show the LN release counter in-game.
        /// </summary>
        private static bool _showReleaseCounter;
        internal static bool ShowReleaseCounter { get => _showReleaseCounter; set { _showReleaseCounter = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Toggle for Grade Bar relative accuracy
        /// </summary>
        private static bool _gradeBarRelative = true;
        internal static bool GradeBarRelative { get => _gradeBarRelative; set { _gradeBarRelative = value; Task.Run(async () => await WriteConfigFileAsync()); } }


        /// <summary>
        ///     The path of the osu!.db file
        /// </summary>
        private static string _osuDbPath;
        internal static string OsuDbPath { get => _osuDbPath; set { _osuDbPath = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Dictates where or not we should load osu! beatmaps from osu!.db on game start
        /// </summary>
        private static bool _autoLoadOsuBeatmaps;
        internal static bool AutoLoadOsuBeatmaps { get => _autoLoadOsuBeatmaps; set { _autoLoadOsuBeatmaps = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 1
        /// </summary>
        private static Keys _keyMania4k1 = Keys.A;
        internal static Keys KeyMania4k1 { get => _keyMania4k1; set { _keyMania4k1 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 2
        /// </summary>
        private static Keys _keyMania4k2 = Keys.S;
        internal static Keys KeyMania4k2 { get => _keyMania4k2; set { _keyMania4k2 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 3
        /// </summary>
        private static Keys _keyMania4k3 = Keys.K;
        internal static Keys KeyMania4k3 { get => _keyMania4k3; set { _keyMania4k3 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 4
        /// </summary>
        private static Keys _keyMania4k4 = Keys.L;
        internal static Keys KeyMania4k4 { get => _keyMania4k4; set { _keyMania4k4 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 1 - 7k
        /// </summary>
        private static Keys _keyMania7k1 = Keys.A;
        internal static Keys KeyMania7k1 { get => _keyMania7k1; set { _keyMania7k1 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 2 - 7k
        /// </summary>
        private static Keys _keyMania7k2 = Keys.S;
        internal static Keys KeyMania7k2 { get => _keyMania7k2; set { _keyMania7k2 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 3 - 7k
        /// </summary>
        private static Keys _keyMania7k3 = Keys.D;
        internal static Keys KeyMania7k3 { get => _keyMania7k3; set { _keyMania7k3 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 4 - 7k
        /// </summary>
        private static Keys _keyMania7k4 = Keys.Space;
        internal static Keys KeyMania7k4 { get => _keyMania7k4; set { _keyMania7k4 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 5 - 7k
        /// </summary>
        private static Keys _keyMania7k5 = Keys.J;
        internal static Keys KeyMania7k5 { get => _keyMania7k5; set { _keyMania7k5 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 6 - 7k
        /// </summary>
        private static Keys _keyMania7k6 = Keys.K;
        internal static Keys KeyMania7k6 { get => _keyMania7k6; set { _keyMania7k6 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed for lane 7 - 7k
        /// </summary>
        private static Keys _keyMania7k7 = Keys.L;
        internal static Keys KeyMania7k7 { get => _keyMania7k7; set { _keyMania7k7 = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to quickly retry the song during gameplay
        /// </summary>
        private static Keys _keyQuickRetry = Keys.OemTilde;
        internal static Keys KeyQuickRetry { get => _keyQuickRetry; set { _keyQuickRetry = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to increase the scroll speed during gameplay.
        /// </summary>
        private static Keys _keyIncreaseScrollSpeed = Keys.F4;
        internal static Keys KeyIncreaseScrollSpeed { get => _keyIncreaseScrollSpeed; set { _keyIncreaseScrollSpeed = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to decrease the scroll speed during gameplay.
        /// </summary>
        private static Keys _keyDecreaseScrollSpeed = Keys.F3;
        internal static Keys KeyDecreaseScrollSpeed { get => _keyDecreaseScrollSpeed; set { _keyDecreaseScrollSpeed = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to pause and menu-back.
        /// </summary>
        private static Keys _keyPause = Keys.Escape;
        internal static Keys KeyPause { get => _keyPause; set { _keyPause = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to turn the volume up.
        /// </summary>
        private static Keys _keyVolumeUp = Keys.Up;
        internal static Keys KeyVolumeUp { get => _keyVolumeUp; set { _keyVolumeUp = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to turn the volume down
        /// </summary>
        private static Keys _keyVolumeDown = Keys.Down;
        internal static Keys KeyVolumeDown { get => _keyVolumeDown; set { _keyVolumeDown = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key pressed to skip the song introduction
        /// </summary>
        private static Keys _keySkipIntro = Keys.Space;
        internal static Keys KeySkipIntro { get => _keySkipIntro; set { _keySkipIntro = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key to take a screenshot of the game window.
        /// </summary>
        private static Keys _keyTakeScreenshot = Keys.F12;
        internal static Keys KeyTakeScreenshot { get => _keyTakeScreenshot; set { _keyTakeScreenshot = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     The key to toggle the overlay
        /// </summary>
        private static Keys _keyToggleOverlay = Keys.F8;
        internal static Keys KeyToggleOverlay { get => _keyToggleOverlay; set { _keyToggleOverlay = value; Task.Run(async () => await WriteConfigFileAsync()); } }

        /// <summary>
        ///     Important!
        ///     Responsible for initializing directory properties,
        ///     writing a new config file if it doesn't exist and also reading config files.
        ///     This should be the one of the first things that is called upon launch.
        /// </summary>
        internal static void InitializeConfig()
        {
            // When initializing, we manually set the directory fields rather than the props,
            // because we only want to write the config file one time at this stage.
            // Usually when a property is modified, it will automatically write the config file again,
            // so that's what we're preventing here.
            _gameDirectory = Directory.GetCurrentDirectory();

            _skinDirectory = GameDirectory + "/Skins";
            Directory.CreateDirectory(SkinDirectory);

            _screenshotDirectory = GameDirectory + "/Screenshots";
            Directory.CreateDirectory(ScreenshotDirectory);

            _logsDirectory = GameDirectory + "/Logs";
            Directory.CreateDirectory(LogsDirectory);

            _replayDirectory = GameDirectory + "/Replays";
            Directory.CreateDirectory(ReplayDirectory);

            _dataDirectory = GameDirectory + "/Data";
            Directory.CreateDirectory(DataDirectory);
            // Create data directory to store all of the game's replays.
            Directory.CreateDirectory(DataDirectory + "/r/");

            _songDirectory = GameDirectory + "/Songs";
            Directory.CreateDirectory(SongDirectory);

            // Create the rintime log file.
            Logger.CreateLogFile();

            // We'll want to write a quaver.cfg file if it doesn't already exist.
            // There's no need to read the config file afterwards, since we already have 
            // all of the default values.
            if (!File.Exists(GameDirectory + "/quaver.cfg"))
            {
                File.WriteAllText(GameDirectory + "/quaver.cfg", "; Quaver Configuration File");
                FirstWrite = true;

                Task.Run(async () => await WriteConfigFileAsync()).Wait();
                return;
            }

            // If we already have a config file, we'll just want to read that.
            ReadConfigFile();
            Logger.Log("Config file has successfully been read.", LogColors.GameImportant);
        }

        /// <summary>
        ///     Reads a quaver.cfg file and sets all of the successfully read values.
        ///     At the end of reading, we write the config file, changing any invalid data/
        /// </summary>
        private static void ReadConfigFile()
        {
            var data = new FileIniDataParser().ReadFile(GameDirectory + "/quaver.cfg")["Config"];

            // Validate and set the parsed values.
            // We use the fields here because since we're reading multiple fields, 
            // we don't want to save the config file more than once whereas if we were using the props.
            _gameDirectory = ConfigHelper.ReadDirectory(GameDirectory, data["GameDirectory"]);
            _skinDirectory = ConfigHelper.ReadDirectory(SkinDirectory, data["SkinDirectory"]);
            _screenshotDirectory = ConfigHelper.ReadDirectory(ScreenshotDirectory, data["ScreenshotDirectory"]);
            _replayDirectory = ConfigHelper.ReadDirectory(ReplayDirectory, data["ReplayDirectory"]);
            _logsDirectory = ConfigHelper.ReadDirectory(LogsDirectory, data["LogsDirectory"]);
            _dataDirectory = ConfigHelper.ReadDirectory(DataDirectory, data["DataDirectory"]);
            _songDirectory = ConfigHelper.ReadDirectory(SongDirectory, data["SongDirectory"]);
            _osuDbPath = ConfigHelper.ReadPath(OsuDbPath, data["OsuDbPath"]);
            _autoLoadOsuBeatmaps = ConfigHelper.ReadBool(AutoLoadOsuBeatmaps, data["AutoLoadOsuBeatmaps"]);
            _username = ConfigHelper.ReadString(Username, data["Username"]);
            _volumeGlobal = ConfigHelper.ReadPercentage(VolumeGlobal, data["VolumeGlobal"]);
            _volumeEffect = ConfigHelper.ReadPercentage(VolumeEffect, data["VolumeEffect"]);
            _volumeMusic = ConfigHelper.ReadPercentage(VolumeMusic, data["VolumeMusic"]);
            _backgroundBrightness = ConfigHelper.ReadPercentage(BackgroundBrightness, data["BackgroundBrightness"]);
            _windowHeight = ConfigHelper.ReadInt32(WindowHeight, data["WindowHeight"]);
            _windowWidth = ConfigHelper.ReadInt32(WindowWidth, data["WindowWidth"]);
            _healthBarPositionTop = ConfigHelper.ReadBool(HealthBarPositionTop, data["HealthBarPositionTop"]);
            _userHitPositionOffset4k = ConfigHelper.ReadInt32(UserHitPositionOffset4k, data["HitPositionOffset4k"]);
            _userHitPositionOffset7k = ConfigHelper.ReadInt32(UserHitPositionOffset7k, data["HitPositionOffset7k"]);
            _windowFullScreen = ConfigHelper.ReadBool(WindowFullScreen, data["WindowFullScreen"]);
            _windowLetterboxed = ConfigHelper.ReadBool(WindowLetterboxed, data["WindowLetterboxed"]);
            _fpsCounter = ConfigHelper.ReadBool(FpsCounter, data["FpsCounter"]);
            _scrollSpeed4k = ConfigHelper.ReadPercentage(ScrollSpeed4k, data["ScrollSpeed4k"]);
            _scrollSpeed7k = ConfigHelper.ReadPercentage(ScrollSpeed7k, data["ScrollSpeed7k"]);
            _downScroll4k = ConfigHelper.ReadBool(DownScroll4k, data["DownScroll4k"]);
            _downScroll7k = ConfigHelper.ReadBool(DownScroll7k, data["DownScroll7k"]);
            _globalAudioOffset = ConfigHelper.ReadSignedByte(GlobalAudioOffset, data["GlobalAudioOffset"]);
            _skin = ConfigHelper.ReadSkin(Skin, data["Skin"]);
            _defaultSkin = ConfigHelper.ReadDefaultSkin(DefaultSkin, data["DefaultSkin"]);
            _pitched = ConfigHelper.ReadBool(Pitched, data["Pitched"]);
            _showReleaseCounter = ConfigHelper.ReadBool(_showReleaseCounter, data["ShowReleaseCounter"]);
            _gradeBarRelative = ConfigHelper.ReadBool(_gradeBarRelative, data["GradeBarRelative"]);
            _keyMania4k1 = ConfigHelper.ReadKeys(KeyMania4k1, data["KeyMania4k1"]);
            _keyMania4k2 = ConfigHelper.ReadKeys(KeyMania4k2, data["KeyMania4k2"]);
            _keyMania4k3 = ConfigHelper.ReadKeys(KeyMania4k3, data["KeyMania4k3"]);
            _keyMania4k4 = ConfigHelper.ReadKeys(KeyMania4k4, data["KeyMania4k4"]);
            _keyMania7k1 = ConfigHelper.ReadKeys(KeyMania7k1, data["KeyMania7k1"]);
            _keyMania7k2 = ConfigHelper.ReadKeys(KeyMania7k2, data["KeyMania7k2"]);
            _keyMania7k3 = ConfigHelper.ReadKeys(KeyMania7k3, data["KeyMania7k3"]);
            _keyMania7k4 = ConfigHelper.ReadKeys(KeyMania7k4, data["KeyMania7k4"]);
            _keyMania7k5 = ConfigHelper.ReadKeys(KeyMania7k5, data["KeyMania7k5"]);
            _keyMania7k6 = ConfigHelper.ReadKeys(KeyMania7k6, data["KeyMania7k6"]);
            _keyMania7k7 = ConfigHelper.ReadKeys(KeyMania7k7, data["KeyMania7k7"]);
            _keyQuickRetry = ConfigHelper.ReadKeys(KeyQuickRetry, data["KeyQuickRetry"]);
            _keyIncreaseScrollSpeed = ConfigHelper.ReadKeys(KeyIncreaseScrollSpeed, data["KeyIncreaseScrollSpeed"]);
            _keyDecreaseScrollSpeed = ConfigHelper.ReadKeys(KeyDecreaseScrollSpeed, data["KeyDecreaseScrollSpeed"]);
            _keySkipIntro = ConfigHelper.ReadKeys(KeySkipIntro, data["KeySkipIntro"]);
            _keyTakeScreenshot = ConfigHelper.ReadKeys(KeyTakeScreenshot, data["KeyTakeScreenshot"]);
            _keyToggleOverlay = ConfigHelper.ReadKeys(KeyToggleOverlay, data["KeyToggleOverlay"]);
                
            // Write the config file with all of the changed/invalidated data.
            Task.Run(async () => await WriteConfigFileAsync());
        }

        /// <summary>
        ///     Takes all of the current values from the Configuration class and creates a file with them.
        ///     This will automatically be called whenever a configuration value is changed in the code.
        /// </summary>
        internal static async Task WriteConfigFileAsync()
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
            foreach (var p in typeof(Configuration)
                .GetProperties(BindingFlags.Static | BindingFlags.NonPublic))
            {
                // Don't include the FirstWrite Property
                if (p.Name == "FirstWrite")
                    continue;

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
                    Logger.Log("Too many attempts in a short time to write the config file have been made.", Color.Aqua);
            }
        }

        /// <summary>
        ///     Checks if the file is ready to be written to.
        /// </summary>
        /// <param name="sFilename"></param>
        /// <returns></returns>
        public static bool IsFileReady(string sFilename)
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
    ///     Enum containing a number representation of the default skins we have available
    /// </summary>
    internal enum DefaultSkins
    {
        Bar,
        Arrow
    }
}