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
        public static Rectangle Window { get; set; } = new Rectangle(0, 0, 800, 480); //TODO: Automatically set this rectangle as windoow size

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
        ///     Used to select a random beatmap from our current list of visible beatmaps.
        /// </summary>
        public static void SelectRandomBeatmap()
        {
            if (Beatmaps.Count == 0)
                return;

            // Find the number of total beatmaps
            var totalMaps = 0;

            foreach (KeyValuePair<string, List<Beatmap>> mapset in Beatmaps)
            {
                totalMaps += mapset.Value.Count;
            }

            var rand = new Random();
            var randomBeatmap = rand.Next(0, totalMaps);

            // Find the totalMaps'th beatmap
            var onMap = 0;
            foreach (KeyValuePair<string, List<Beatmap>> mapset in Beatmaps)
            {
                bool foundBeatmap = false;

                foreach (var beatmap in mapset.Value)
                {
                    if (onMap == randomBeatmap)
                    {
                        SelectedBeatmap = beatmap;
                        foundBeatmap = true;
                        break;
                    }

                    onMap++;
                }

                if (foundBeatmap)
                    break;
            }

            Console.WriteLine($"[GAME BASE] Random Beatmap: {SelectedBeatmap.Artist} - {SelectedBeatmap.Title} [{SelectedBeatmap.DifficultyName}]");
        }
    }
}
