using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Quaver.Database.Beatmaps;
using Quaver.Database;
using Quaver.Skins;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Discord;
using Quaver.Enums;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Maps;
using Quaver.Utility;

namespace Quaver
{
    /// <summary>
    ///     Holds all the global variables and configuration for our game.
    /// </summary>
    internal static class GameBase
    {
        /// <summary>
        ///     Reference to the Game's window.
        /// </summary>
        public static GameWindow GameWindow { get; set; }

        /// <summary>
        ///     The global game state manager for the game.
        /// </summary>
        public static GameStateManager GameStateManager { get; } = new GameStateManager();

        /// <summary>
        ///     The current list of loaded beatmaps
        /// </summary>
        public static List<Mapset> Mapsets { get; set; }

        /// <summary>
        ///     The current list of ***visible*** beatmaps - Use these for song select!
        /// </summary>
        public static List<Mapset> VisibleMapsets { get; set; }

        /// <summary>
        ///     The currently selected beatmap.
        /// </summary>
        public static Beatmap SelectedBeatmap { get; set; }

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
        ///     The reference resolution for UI and game elements
        /// </summary>
        public static Vector2 ReferenceResolution { get; } = new Vector2(1280, 720);

        /// <summary>
        ///     The rectangle this game will be rendered onto
        /// </summary>
        public static DrawRectangle Window { get; private set; } = new DrawRectangle(0, 0, Configuration.WindowWidth, Configuration.WindowHeight); //TODO: Automatically set this rectangle as windoow size through method

        /// <summary>
        ///     WindowHeight / WindowWidth ratio
        /// </summary>
        public static float WindowUIScale { get; private set; } = Window.Height / ReferenceResolution.Y; //TODO: Automatically set this rectangle as windoow size through method

        /// <summary>
        ///     The game's clock. Essentially it controls which speed songs are played at.
        /// </summary>
        public static float GameClock { get; set; } = 1.0f;

        /// <summary>
        ///     The score multiplier for the game. Controls how many points the game score will be 
        ///     multiplied by.
        /// </summary>
        public static float ScoreMultiplier { get; set; } = 1.0f;

        /// <summary>
        ///     A list of all the current game modifiers that are activated.
        /// </summary>
        public static List<IMod> CurrentGameModifiers { get; set; } = new List<IMod>();

        /// <summary>
        ///     Keeps track of if the bass library is already initialized on the default output device
        /// </summary>
        public static bool BassInitialized { get; set; } 

        /// <summary>
        ///     The current keyboard state.
        /// </summary>
        public static KeyboardState KeyboardState { get; set; }

        /// <summary>
        ///     The current Mouse State
        /// </summary>
        public static MouseState MouseState { get; set; }

        /// <summary>
        /// The mouse cursor
        /// </summary>
        public static Cursor Cursor { get; private set; }

        /// <summary>
        ///     The current Discord Controller for RichPresence.
        /// </summary>
        public static DiscordController DiscordController { get; set; }

        /// <summary>
        ///     Keeps track of whether Discord Rich Presence has been initialized.
        /// </summary>
        public static bool DiscordRichPresencedInited { get; set; }

        /// <summary>
        ///     Contains all of the loaded embedded UI .xnb resources that aren't apart of the
        ///     default skin
        /// </summary>
        public static UI UI { get; set; } = new UI();

        /// <summary>
        ///     Create a Stopwatch object for the game, This'll hold the time since the application
        ///     was started.
        /// </summary>
        public static Stopwatch GameTime { get; set;} = Stopwatch.StartNew();

        /// <summary>
        ///     The build version of the game (The md5 hash of the exe)
        /// </summary>
        public static string BuildVersion { get; set; }

        /// <summary>
        ///     Whenever the settings for window size is changed, call this method to update the window.
        /// </summary>
        /// <param name="newSize"></param>
        public static void UpdateWindow(Point newSize)
        {
            //TODO: unfinished
            Window = new DrawRectangle(0, 0, Configuration.WindowWidth, Configuration.WindowHeight);
            Rectangle mainWindow = GraphicsDevice.PresentationParameters.Bounds;

            //Align letterboxed window
            //Window = Util.DrawRect(Alignment.MidCenter, Window, mainWindow);
            WindowUIScale = Window.Height / ReferenceResolution.Y;
        }

        /// <summary>
        ///     Responsible for loading and setting our global beatmaps variable.
        /// </summary>
        public static async Task LoadAndSetBeatmaps()
        {
            Mapsets = BeatmapUtils.OrderBeatmapsByArtist(await BeatmapCache.LoadBeatmapDatabaseAsync());
            VisibleMapsets = Mapsets;
        }

        /// <summary>
        ///     Loads the skin defined in the config file. 
        /// </summary>
        public static void LoadSkin()
        {
            LoadedSkin = new Skin(Configuration.Skin);
        }

        /// <summary>
        ///     Initialize Cursor. Only called once per game.
        /// </summary>
        public static void LoadCursor()
        {
            Cursor = new Cursor();
        }

        /// <summary>
        ///     Changes the map.
        /// </summary>
        public static void ChangeBeatmap(Beatmap map)
        {
            SelectedBeatmap = map;
        }

        /// <summary>
        ///     Loads a beatmap's background
        /// </summary>
        public static void LoadBackground()
        {
            if (CurrentBackground != null)
                CurrentBackground.Dispose();

            var bgPath = Configuration.SongDirectory + "/" + SelectedBeatmap.Directory + "/" + SelectedBeatmap.BackgroundPath;

            if (!File.Exists(bgPath))
                return;

            CurrentBackground = ImageLoader.Load(bgPath);
        }

        /// <summary>
        ///     Responsible for changing the discord rich presence.
        /// </summary>
        /// <param name="details"></param>
        public static void ChangeDiscordPresence(string details, string state, double timeLeft = 0)
        {
            if (!DiscordRichPresencedInited)
                return;

            try
            {
                DiscordController.presence.details = details;

                if (timeLeft != 0)
                {
                    // Get Current Unix Time
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var unixDateTime = (DateTime.Now.ToLocalTime().ToUniversalTime() - epoch).TotalSeconds;

                    // Set Discord presence to the "time left" specified.
                    DiscordController.presence.endTimestamp = (long)(unixDateTime + (timeLeft / 1000));
                }
                else
                {
                    DiscordController.presence.endTimestamp = 0;
                }

                DiscordController.presence.smallImageKey = "4k";

                // Set presence based Mode
                switch (SelectedBeatmap.Mode)
                {
                    case GameModes.Keys4:
                        DiscordController.presence.smallImageText = "4 Keys";
                        break;
                    case GameModes.Keys7:
                        DiscordController.presence.smallImageText = "7 Keys";
                        break;
                    default:
                        break;
                }

                DiscordController.presence.state = state;
                DiscordRPC.UpdatePresence(ref DiscordController.presence);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogColors.GameError);
                throw;
            }
        }

        /// <summary>
        ///     Responsible for handling discord presence w/ mods if any exist.
        /// </summary>
        public static void ChangeDiscordPresenceGameplay(bool skippedSong)
        {
            if (!DiscordRichPresencedInited)
                return;

            try
            {
                var mapString = $"{SelectedBeatmap.Qua.Artist} - {SelectedBeatmap.Qua.Title} [{SelectedBeatmap.Qua.DifficultyName}]";

                // Get the original map length. 
                double mapLength = Qua.FindSongLength(SelectedBeatmap.Qua) / GameClock;

                // Get the new map length if it was skipped.
                if (skippedSong)
                    mapLength = (Qua.FindSongLength(SelectedBeatmap.Qua) - SongManager.Position) / GameClock;

                var sb = new StringBuilder();
                sb.Append("Playing");

                // Add mods to the string if mods exist
                if (CurrentGameModifiers.Count > 0)
                {
                    sb.Append(" + ");

                    if (CurrentGameModifiers.Exists(x => x.Type == ModType.Speed))
                        sb.Append($"Speed {GameClock}x");
                }

                ChangeDiscordPresence(mapString, sb.ToString(), mapLength);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, LogColors.GameError);
            }
        }
    }
}
