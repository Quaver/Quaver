using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Database.Beatmaps;
using Quaver.Discord;
using Quaver.Graphics.Sprite;
using Quaver.Logging;

namespace Quaver.Audio
{
    /// <summary>
    ///     Responsible for loading and playing random beatmaps' songs during the main menu
    /// </summary>
    internal class MenuAudioPlayer
    {
        /// <summary>
        ///     A reference to the last beatmap that was selected.
        /// </summary>
        private static Beatmap LastBeatmap { get; set; }

        /// <summary>
        ///     Initializes the main menu's audio player.
        /// </summary>
        internal static void Initialize()
        {
            if (GameBase.Mapsets.Count == 0)
                return; 

            // Load and play the randomly selected beatmap's song.
            if (GameBase.SelectedBeatmap != null)
            {
                // In the event that the song is already loaded up, we don't want to load it again
                // through this state.
                if (SongManager.Length > 1)
                    SongManager.Resume();
                else
                {
                    SongManager.Load();

                    if (SongManager.Length > 1)
                        SongManager.Play();
                }

                DiscordController.ChangeDiscordPresence($"{GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}", "Listening");
            }
            else
            {
                // Set Rich Presence
                DiscordController.ChangeDiscordPresence("Idle", "In the menus");
            }
        }

        /// <summary>
        ///     Checks if a beatmap's song has finished, and if it has it will select another random one
        ///     and play that. This is ran all throughout the main menu screen.
        /// </summary>
        internal static void PlayRandomBeatmaps()
        {
            if (GameBase.Mapsets.Count == 0 || SongManager.Position < SongManager.Length)
                return;
            
            if (SongManager.Length > 1)
                SongManager.Stop();

            // Select new map
            if (GameBase.Mapsets.Count > 1)
                BeatmapUtils.SelectRandomBeatmap();

            // Check if the newly selected map isn't the same as the last.
            if (GameBase.SelectedBeatmap == LastBeatmap)
                return;

            LastBeatmap = GameBase.SelectedBeatmap;
            
            // Load Audio
            SongManager.Load();

            // Load Background and change it
            BackgroundManager.LoadBackground();

            if (GameBase.CurrentBackground != null)
                BackgroundManager.Change(GameBase.CurrentBackground);

            // Begin to play
            if (SongManager.Length > 1 && SongManager.AudioStream != 0)
                SongManager.Play();

            // Set new Discord Rich Presence
            DiscordController.ChangeDiscordPresence($"{GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}", "Listening");
        }
    }
}
