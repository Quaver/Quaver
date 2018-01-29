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
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Discord;
using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.Utility;
using Quaver.Input;
using Quaver.Graphics.GameOverlay;
using Quaver.Steam;

namespace Quaver
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

        public static GameOverlay GameOverlay { get; set; } = new GameOverlay();

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
        ///     The reference resolution for UI and game elements
        /// </summary>
        public static Point ReferenceResolution { get; } = new Point(1280, 720);

        /*
        /// <summary>
        ///     The rectangle this game will be rendered onto
        /// </summary>
        public static DrawRectangle Window { get; private set; } = new DrawRectangle(0, 0, Configuration.WindowWidth, Configuration.WindowHeight); //TODO: Automatically set this rectangle as windoow size through method*/

        public static DrawRectangle WindowRectangle { get; set; } = new DrawRectangle(0, 0, Configuration.WindowWidth, Configuration.WindowHeight);

        /// <summary>
        ///     WindowHeight / WindowWidth ratio
        /// </summary>
        public static float WindowUIScale { get; set; } = WindowRectangle.Height / ReferenceResolution.Y; //TODO: Automatically set this rectangle as windoow size through method

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
        public static Cursor Cursor { get; set; }

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
        public static Stopwatch GameTime { get; set; } = Stopwatch.StartNew();

        /// <summary>
        ///     The build version of the game (The md5 hash of the exe)
        /// </summary>
        public static string BuildVersion { get; set; }

#if STEAM
        /// <summary>
        ///     Class that handles everything to do with Steam API calls
        /// </summary>
        public static SteamAPIHelper SteamAPIHelper { get; set; }
#endif

        /// <summary>
        ///     This method changes the window to match configuration settings
        /// </summary>
        /// <param name="resolution"></param>
        /// <param name="fullscreen"></param>
        /// <param name="letterbox"></param>
        public static void ChangeWindow(bool fullscreen, bool letterbox, Point? resolution = null)
        {
            // Change Resolution
            if (resolution != null)
            {
                Configuration.WindowWidth = resolution.Value.X;
                Configuration.WindowHeight = resolution.Value.Y;
                GraphicsManager.PreferredBackBufferWidth = resolution.Value.X;
                GraphicsManager.PreferredBackBufferHeight = resolution.Value.Y;
                WindowRectangle = new DrawRectangle(0, 0, resolution.Value.X, resolution.Value.Y);
                WindowUIScale = WindowRectangle.Height / ReferenceResolution.Y;
            }

            // Update Fullscreen
            if (fullscreen != GraphicsManager.IsFullScreen)
                GraphicsManager.IsFullScreen = fullscreen;

            // Update letter boxing
            if (letterbox)
            {
                //do stuff
            }

            // Log this event
            Logger.Log("Window settings changed:", LogColors.GameImportant);
            Logger.Log("Window Resolution: "+ GraphicsManager.PreferredBackBufferWidth + "x" + GraphicsManager.PreferredBackBufferHeight, LogColors.GameInfo);
            Logger.Log("Window Letterboxing: " + letterbox, LogColors.GameInfo);
            Logger.Log("Window Fullscreen: "+ GraphicsManager.IsFullScreen, LogColors.GameInfo);

            // Apply changes to graphics manager
            GraphicsManager.ApplyChanges();
        }
    }
}
