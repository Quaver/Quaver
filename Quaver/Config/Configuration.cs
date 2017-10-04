using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using Microsoft.Xna.Framework.Input;

namespace Quaver.Config
{
    internal class Configuration
    {
        /// <summary>
        ///     The master volume of the game.
        /// </summary>
        private static byte _volumeGlobal = 100;

        /// <summary>
        ///     The SFX volume of the game.
        /// </summary>
        private static byte _volumeEffect = 50;

        /// <summary>
        ///     The Music volume of the gamne.
        /// </summary>
        private static byte _volumeMusic = 50;

        /// <summary>
        ///     The dim for backgrounds during gameplay
        /// </summary>
        private static byte _backgroundDim;

        /// <summary>
        ///     The height of the window.
        /// </summary>
        private static int _windowHeight = 900;

        /// <summary>
        ///     The width of the window.
        /// </summary>
        private static int _windowWidth = 1600;

        /// <summary>
        ///     Is the window fullscreen?
        /// </summary>
        private static bool _windowFullScreen;

        /// <summary>
        ///     Is the window letterboxed?
        /// </summary>
        private static bool _windowLetterboxed;

        /// <summary>
        ///     The max custom frame limit set by the user.
        /// </summary>
        private static int _customFrameLimit = 240;

        /// <summary>
        ///     Should the game display the FPS Counter?
        /// </summary>
        private static bool _fpsCounter = true;

        /// <summary>
        ///     Should the game display a frame times graph?
        /// </summary>
        private static bool _showFrameTimeDisplay;

        /// <summary>
        ///     The language the game is in.
        /// </summary>
        private static string _language = "en";

        /// <summary>
        ///     The scroll speed during gameplay.
        /// </summary>
        private static byte _scrollSpeed = 34;

        /// <summary>
        ///     Should the scroll speed be scaled to align with the song's BPM?
        /// </summary>
        private static bool _scaleScrollSpeedWithBpm;

        /// <summary>
        ///     Should the game be played with DownScroll? If false, it's UpScroll
        /// </summary>
        private static bool _downScroll = true;

        /// <summary>
        ///     The offset of the notes compared to the song start.
        /// </summary>
        private static short _globalOffset;

        /// <summary>
        ///     Should Timing Bars be displayed during gameplay?
        /// </summary>
        private static bool _displayTimingBars = true;

        /// <summary>
        ///     Should the leaderboard be visible during gameplay?
        /// </summary>
        private static bool _leaderboardVisible = true;

        /// <summary>
        ///     The skin in the Skins directory that is loaded. Default is the only exception, as it'll be overrided.
        /// </summary>
        private static string _skin = "Default";

        /// <summary>
        ///     The key pressed for lane 1
        /// </summary>
        private static Keys _keyMania1 = Keys.D;

        /// <summary>
        ///     The key pressed for lane 2
        /// </summary>
        private static Keys _keyMania2 = Keys.F;

        /// <summary>
        ///     The key pressed for lane 3
        /// </summary>
        private static Keys _keyMania3 = Keys.J;

        /// <summary>
        ///     The key pressed for lane 4
        /// </summary>
        private static Keys _keyMania4 = Keys.K;

        /// <summary>
        ///     The key pressed to quickly retry the song during gameplay
        /// </summary>
        private static Keys _keyQuickRetry = Keys.OemTilde;

        /// <summary>
        ///     The key pressed to increase the scroll speed during gameplay.
        /// </summary>
        private static Keys _keyIncreaseScrollSpeed = Keys.F4;

        /// <summary>
        ///     The key pressed to decrease the scroll speed during gameplay.
        /// </summary>
        private static Keys _keyDecreaseScrollSpeed = Keys.F3;

        /// <summary>
        ///     The key pressed to pause and menu-back.
        /// </summary>
        private static Keys _keyPause = Keys.Escape;

        /// <summary>
        ///     The key pressed to turn the volume up.
        /// </summary>
        private static Keys _keyVolumeUp = Keys.Up;

        /// <summary>
        ///     The key pressed to turn the volume down
        /// </summary>
        private static Keys _keyVolumeDown = Keys.Down;

        /// <summary>
        ///     These are all values that should never ben
        /// </summary>
        private static string _gameDirectory;

        /// <summary>
        ///     The skin directory
        /// </summary>
        private static string _skinDirectory;

        /// <summary>
        ///     The screenshot directory
        /// </summary>
        private static string _screenshotDirectory;

        /// <summary>
        ///     The replay directory
        /// </summary>
        private static string _replayDirectory;

        /// <summary>
        ///     The Logs directory
        /// </summary>
        private static string _logsDirectory;

        /// <summary>
        ///     The data directory
        /// </summary>
        private static string _dataDirectory;

        /// <summary>
        ///     The song directory
        /// </summary>
        private static string _songDirectory;

        internal static string GameDirectory
        {
            get => _gameDirectory;
            set
            {
                _gameDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string Songdirectory
        {
            get => _songDirectory;
            set
            {
                _songDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string SkinDirectory
        {
            get => _skinDirectory;
            set
            {
                _skinDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string ScreenshotDirectory
        {
            get => _screenshotDirectory;
            set
            {
                _screenshotDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string ReplayDirectory
        {
            get => _replayDirectory;
            set
            {
                _replayDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string LogsDirectory
        {
            get => _logsDirectory;
            set
            {
                _logsDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string DataDirectory
        {
            get => _dataDirectory;
            set
            {
                _dataDirectory = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static byte VolumeGlobal
        {
            get => _volumeGlobal;
            set
            {
                _volumeGlobal = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static byte VolumeEffect
        {
            get => _volumeEffect;
            set
            {
                _volumeEffect = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static byte VolumeMusic
        {
            get => _volumeMusic;
            set
            {
                _volumeMusic = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static byte BackgroundDim
        {
            get => _backgroundDim;
            set
            {
                _backgroundDim = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static int WindowHeight
        {
            get => _windowHeight;
            set
            {
                _windowHeight = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static int WindowWidth
        {
            get => _windowWidth;
            set
            {
                _windowWidth = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool WindowFullScreen
        {
            get => _windowFullScreen;
            set
            {
                _windowFullScreen = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool WindowLetterboxed
        {
            get => _windowLetterboxed;
            set
            {
                _windowLetterboxed = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static int CustomFrameLimit
        {
            get => _customFrameLimit;
            set
            {
                _customFrameLimit = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool FpsCounter
        {
            get => _fpsCounter;
            set
            {
                _fpsCounter = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool ShowFrameTimeDisplay
        {
            get => _showFrameTimeDisplay;
            set
            {
                _showFrameTimeDisplay = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string Language
        {
            get => _language;
            set
            {
                _language = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static byte ScrollSpeed
        {
            get => _scrollSpeed;
            set
            {
                _scrollSpeed = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool ScaleScrollSpeedWithBpm
        {
            get => _scaleScrollSpeedWithBpm;
            set
            {
                _scaleScrollSpeedWithBpm = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool DownScroll
        {
            get => _downScroll;
            set
            {
                _downScroll = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static short GlobalOffset
        {
            get => _globalOffset;
            set
            {
                _globalOffset = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool DisplayTimingBars
        {
            get => _displayTimingBars;
            set
            {
                _displayTimingBars = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static bool LeaderboardVisible
        {
            get => _leaderboardVisible;
            set
            {
                _leaderboardVisible = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static string Skin
        {
            get => _skin;
            set
            {
                _skin = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyMania1
        {
            get => _keyMania1;
            set
            {
                _keyMania1 = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyMania2
        {
            get => _keyMania2;
            set
            {
                _keyMania2 = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyMania3
        {
            get => _keyMania3;
            set
            {
                _keyMania3 = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyMania4
        {
            get => _keyMania4;
            set
            {
                _keyMania4 = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyQuickRetry
        {
            get => _keyQuickRetry;
            set
            {
                _keyQuickRetry = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyIncreaseScrollSpeed
        {
            get => _keyIncreaseScrollSpeed;
            set
            {
                _keyIncreaseScrollSpeed = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyDecreaseScrollSpeed
        {
            get => _keyDecreaseScrollSpeed;
            set
            {
                _keyDecreaseScrollSpeed = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyPause
        {
            get => _keyPause;
            set
            {
                _keyPause = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyVolumeUp
        {
            get => _keyVolumeUp;
            set
            {
                _keyVolumeUp = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        internal static Keys KeyVolumeDown
        {
            get => _keyVolumeDown;
            set
            {
                _keyVolumeDown = value;
                Task.Run(async () => await WriteConfigFileAsync());
            }
        }

        /// <summary>
        ///     Important!
        ///     Responsible for initializing directory properties,
        ///     writing a new config file if it doesn't exist and also reading config files.
        ///     This should be the one of the first things that is called upon launch.
        /// </summary>
        internal static void InitializeConfig()
        {
            Console.WriteLine("[CONFIG] Initializing Config...");

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

            _songDirectory = GameDirectory + "/Songs";
            Directory.CreateDirectory(Songdirectory);

            // We'll want to write a quaver.cfg file if it doesn't already exist.
            // There's no need to read the config file afterwards, since we already have 
            // all of the default values.
            if (!File.Exists(GameDirectory + "/quaver.cfg"))
            {
                Task.Run(async () => await WriteConfigFileAsync()).Wait();
                return;
            }

            // If we already have a config file, we'll just want to read that.
            ReadConfigFile();
            Console.WriteLine("[CONFIG] Config file successfully read/created!");
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
            _songDirectory = ConfigHelper.ReadDirectory(Songdirectory, data["SongDirectory"]);
            _volumeGlobal = ConfigHelper.ReadPercentage(VolumeGlobal, data["VolumeGlobal"]);
            _volumeEffect = ConfigHelper.ReadPercentage(VolumeEffect, data["VolumeEffect"]);
            _volumeMusic = ConfigHelper.ReadPercentage(VolumeMusic, data["VolumeMusic"]);
            _backgroundDim = ConfigHelper.ReadPercentage(BackgroundDim, data["BackgroundDim"]);
            _windowHeight = ConfigHelper.ReadInt32(WindowHeight, data["WindowHeight"]);
            _windowWidth = ConfigHelper.ReadInt32(WindowWidth, data["WindowWidth"]);
            _windowFullScreen = ConfigHelper.ReadBool(WindowFullScreen, data["WindowFullScreen"]);
            _windowLetterboxed = ConfigHelper.ReadBool(WindowLetterboxed, data["WindowLetterboxed"]);
            _customFrameLimit = ConfigHelper.ReadInt32(CustomFrameLimit, data["CustomFrameLimit"]);
            _fpsCounter = ConfigHelper.ReadBool(FpsCounter, data["FpsCounter"]);
            _showFrameTimeDisplay = ConfigHelper.ReadBool(ShowFrameTimeDisplay, data["ShowFrameTimeDisplay"]);
            _language = ConfigHelper.ReadString(Language, data["Language"]);
            _scrollSpeed = ConfigHelper.ReadPercentage(ScrollSpeed, data["ScrollSpeed"]);
            _scaleScrollSpeedWithBpm = ConfigHelper.ReadBool(ScaleScrollSpeedWithBpm, data["ScaleScrollSpeedWithBpm"]);
            _downScroll = ConfigHelper.ReadBool(DownScroll, data["DownScroll"]);
            _globalOffset = ConfigHelper.ReadInt16(GlobalOffset, data["GlobalOffset"]);
            _displayTimingBars = ConfigHelper.ReadBool(DisplayTimingBars, data["DisplayTimingBars"]);
            _leaderboardVisible = ConfigHelper.ReadBool(LeaderboardVisible, data["LeaderboardVisible"]);
            _skin = ConfigHelper.ReadSkin(Skin, data["Skin"]);
            _keyMania1 = ConfigHelper.ReadKeys(KeyMania1, data["KeyMania1"]);
            _keyMania2 = ConfigHelper.ReadKeys(KeyMania2, data["KeyMania2"]);
            _keyMania3 = ConfigHelper.ReadKeys(KeyMania3, data["KeyMania3"]);
            _keyMania4 = ConfigHelper.ReadKeys(KeyMania4, data["KeyMania4"]);
            _keyQuickRetry = ConfigHelper.ReadKeys(KeyQuickRetry, data["KeyQuickRetry"]);
            _keyIncreaseScrollSpeed = ConfigHelper.ReadKeys(KeyIncreaseScrollSpeed, data["KeyIncreaseScrollSpeed"]);
            _keyDecreaseScrollSpeed = ConfigHelper.ReadKeys(KeyDecreaseScrollSpeed, data["KeyDecreaseScrollSpeed"]);

            // Write the config file with all of the changed/invalidated data.
            Task.Run(async () => await WriteConfigFileAsync());
        }

        /// <summary>
        ///     Takes all of the current values from the Configuration class and creates a file with them.
        ///     This will automatically be called whenever a configuration value is changed in the code.
        /// </summary>
        internal static async Task WriteConfigFileAsync()
        {
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
                sb.AppendLine(p.Name + " = " + p.GetValue(null));

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
            }
            catch (Exception e)
            {
                // Log File could not be generated to handle this, we'll generally just want to 
                // crash the program. This shouldn't ever happen, unless the filestream is already open.
                Console.WriteLine(e);
            }
        }
    }
}