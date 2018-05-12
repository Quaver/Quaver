using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Osu;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics.Base;
using Quaver.Graphics.Overlays.Navbar;
using Quaver.Graphics.Overlays.Volume;
using Quaver.Graphics.UserInterface;
using Quaver.Input;
using Quaver.Modifiers;
using Quaver.Skinning;

namespace Quaver.Main
{
    /// <summary>
    ///     Holds all the global variables and configuration for our game.
    /// </summary>
    internal static class GameBase
    {
        /// <summary>
        ///     The global input manager. For all inputs that are the same across every state.
        /// </summary>
        public static GlobalInputManager GlobalInputManager { get; } = new GlobalInputManager();

        /// <summary>
        ///     Reference to the Game's window.
        /// </summary>
        public static GameWindow GameWindow { get; set; }

        /// <summary>
        ///     The global game state manager for the game.
        /// </summary>
        public static GameStateManager GameStateManager { get; } = new GameStateManager();

        /// <summary>
        ///     The current list of loaded mapsets
        /// </summary>
        public static List<Mapset> Mapsets { get; set; }

        /// <summary>
        ///     The current list of ***visible*** mapsets - Use these for song select!
        /// </summary>
        public static List<Mapset> VisibleMapsets { get; set; }

        /// <summary>
        ///     The currently selected map.
        /// </summary>
        public static Map SelectedMap { get; set; }

        /// <summary>
        ///     The currently loaded Skin
        /// </summary>
        public static Skin LoadedSkin { get; set; }

        /// <summary>
        ///     The current background
        /// </summary>
        public static Texture2D CurrentBackground { get; set; }

        /// <summary>
        ///     A boolean flag that controls whether or not we have queued changes in the song's directory.
        /// </summary>
        public static bool ImportQueueReady { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static RenderTarget2D MainRenderTarget { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static GraphicsDeviceManager GraphicsManager { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public static SpriteBatch SpriteBatch { get; set; }

        /// <summary>
        ///     The content manager 
        /// </summary>       
        public static ContentManager Content { get; set; }

        /// <summary>
        ///     The reference resolution for QuaverUserInterface and game elements
        /// </summary>
        public static Point ReferenceResolution => new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);

        /// <summary>
        ///     Contains the path of the previously loaded background.
        /// </summary>
        public static string LastBackgroundPath { get; set; }

        /// <summary>
        ///     The osu! Songs folder path
        /// </summary>
        public static string OsuSongsFolder { get; set; }

        /// <summary>
        ///     The Etterna parent folder. 
        ///     NOTE: The Map directory themselves have /songs/ already on it.
        ///     Thank you SM guys!
        /// </summary>
        public static string EtternaFolder { get; set; }

        /// <summary>
        ///     The rectangle this game will be rendered onto
        /// </summary>
        public static DrawRectangle WindowRectangle { get; set; } = new DrawRectangle(0, 0, ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);

        /// <summary>
        ///     WindowHeight / WindowWidth ratio
        ///     //TODO: Automatically set this rectangle as windoow size through method
        /// </summary>
        public static float WindowUIScale => WindowRectangle.Height / ReferenceResolution.Y; 

        /// <summary>
        ///     The score multiplier for the game. Controls how many points the game score will be 
        ///     multiplied by.
        /// </summary>
        public static float ScoreMultiplier { get; set; } = 1.0f;

        /// <summary>
        ///     A list of all the current game modifiers that are activated.
        /// </summary>
        public static List<IGameplayModifier> CurrentGameModifiers { get; set; } = new List<IGameplayModifier>();

        /// <summary>
        ///     The current keyboard state.
        /// </summary>
        public static KeyboardState KeyboardState { get; set; }

        /// <summary>
        ///     The keyboard state of the previous frame.
        /// </summary>
        public static KeyboardState PreviousKeyboardState { get; set; }

        /// <summary>
        ///     The current Mouse State
        /// </summary>
        public static MouseState MouseState { get; set; }

        /// <summary>
        ///     The mouse state of the previous frame.
        /// </summary>
        public static MouseState PreviousMouseState { get; set; }

        /// <summary>
        /// The mouse cursor
        /// </summary>
        public static QuaverCursor QuaverCursor { get; set; }

        /// <summary>
        ///     The current Discord Controller for RichPresence.
        /// </summary>
        public static DiscordController DiscordController { get; set; }

        /// <summary>
        ///     Keeps track of whether Discord Rich Presence has been initialized.
        /// </summary>
        public static bool DiscordRichPresencedInited { get; set; }

        /// <summary>
        ///     Contains all of the loaded embedded QuaverUserInterface .xnb resources that aren't apart of the
        ///     default skin
        /// </summary>
        public static QuaverUserInterface QuaverUserInterface { get; set; } = new QuaverUserInterface();

        /// <summary>
        ///     Create a Stopwatch object for the game, This'll hold the time since the application
        ///     was started.
        /// </summary>
        public static Stopwatch GameTime { get; set; } = Stopwatch.StartNew();

        /// <summary>
        ///     The build version of the game (The md5 hash of the exe)
        /// </summary>
        public static string BuildVersion { get; set; }

        /// <summary>
        ///     Reference to the game's audio engine
        /// </summary>
        public static AudioEngine AudioEngine { get; set; } = new AudioEngine();

        /// <summary>
        ///     Reference to the global volume controller
        /// </summary>
        public static VolumeController VolumeController { get; set; }

        /// <summary>
        ///     Reference to the global navbar.
        /// </summary>
        public static Nav Navbar { get; set; }

        /// <summary>
        ///     The current path of the selected map's audio file
        /// </summary>
        public static string CurrentAudioPath {
            get
            {
                switch (SelectedMap.Game)
                {
                    case MapGame.Osu:
                        return OsuSongsFolder + "/" + SelectedMap.Directory + "/" + SelectedMap.AudioPath;
                    case MapGame.Quaver:
                        return ConfigManager.SongDirectory + "/" + SelectedMap.Directory + "/" + SelectedMap.AudioPath;
                    case MapGame.Etterna:
                        return EtternaFolder + "/" + SelectedMap.Directory + "/" + SelectedMap.AudioPath;
                    default:
                        return "";
                }                
            }
        }

        /// <summary>
        ///     The current path of the selected map's background path.
        /// </summary>
        public static string CurrentBackgroundPath
        {
            get
            {
                switch (SelectedMap.Game)
                {
                    case MapGame.Osu:
                        // Parse the map and get the background
                        var osu = new PeppyBeatmap(OsuSongsFolder + SelectedMap.Directory + "/" + SelectedMap.Path);
                        return $@"{OsuSongsFolder}/{SelectedMap.Directory}/{osu.Background}";
                    case MapGame.Quaver:
                        return ConfigManager.SongDirectory + "/" + SelectedMap.Directory + "/" + SelectedMap.BackgroundPath;
                    case MapGame.Etterna:
                        return EtternaFolder + "/" + SelectedMap.Directory + "/" + SelectedMap.BackgroundPath;
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        ///     The current mods of the game in ModIdentifier form.
        /// </summary>
        public static ModIdentifier CurrentMods
        {
            get
            {
                var mods = 0;

                foreach (var mod in CurrentGameModifiers)
                    mods += (int)mod.ModIdentifier;

                return (ModIdentifier) mods;
            }
        }
    }
}
