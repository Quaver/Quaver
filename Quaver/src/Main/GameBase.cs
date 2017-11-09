using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Beatmaps;
using Quaver.Database;
using Quaver.Skins;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Quaver.Config;
using Quaver.Discord;
using Quaver.Graphics;
using Quaver.Modifiers;

namespace Quaver.Main
{
    /// <summary>
    ///     Holds all the global variables and configuration for our game.
    /// </summary>
    internal static class GameBase
    {
        /// <summary>
        ///     The current list of loaded beatmaps
        /// </summary>
        public static Dictionary<string, List<Beatmap>> Beatmaps { get; set; }

        /// <summary>
        ///     The current list of ***visible*** beatmaps - Use these for song select!
        /// </summary>
        public static Dictionary<string, List<Beatmap>> VisibleBeatmaps { get; set; }

        /// <summary>
        ///     The currently selected beatmap.
        /// </summary>
        public static Beatmap SelectedBeatmap { get; set; }

        /// <summary>
        ///     The currently loaded Skin
        /// </summary>
        public static Skin LoadedSkin { get; set; }

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
        /// 
        /// </summary>
        public static Rectangle Window { get; set; } = new Rectangle(0, 0, Configuration.WindowWidth, Configuration.WindowHeight); //TODO: Automatically set this rectangle as windoow size

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
        ///     Responsible for loading and setting our global beatmaps variable.
        /// </summary>
        public static async Task LoadAndSetBeatmaps()
        {
            Beatmaps = BeatmapUtils.OrderBeatmapsByArtist(await BeatmapCache.LoadBeatmapDatabaseAsync());
            VisibleBeatmaps = Beatmaps;
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
    }
}
