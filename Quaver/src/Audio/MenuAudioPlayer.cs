using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Beatmaps;
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
        ///     Initializes the main menu's audio player.
        /// </summary>
        internal static void Initialize()
        {
            if (GameBase.Beatmaps.Count == 0)
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

                GameBase.ChangeDiscordPresence($"In the main menu listening to: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}");
            }
            else
            {
                // Set Rich Presence
                GameBase.ChangeDiscordPresence("Idle");
            }
        }

        /// <summary>
        ///     Checks if a beatmap's song has finished, and if it has it will select another random one
        ///     and play that. This is ran all throughout the main menu screen.
        /// </summary>
        internal static void PlayRandomBeatmaps()
        {
            if (GameBase.Beatmaps.Count == 0)
                return;

            if (SongManager.Position < SongManager.Length)
                return;

            if (SongManager.Length > 1)
                SongManager.Stop();

            // Select new map
            BeatmapUtils.SelectRandomBeatmap();

            // Load Audio
            SongManager.Load();

            // Load Background and change it
            GameBase.SelectedBeatmap.LoadBackground();

            if (GameBase.SelectedBeatmap.Background != null)
                BackgroundManager.Change(GameBase.SelectedBeatmap.Background);

            // Begin to play
            if (SongManager.Length > 1)
                SongManager.Play();

            // Set new Discord Rich Presence
            GameBase.ChangeDiscordPresence($"In the main menu listening to: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}");
        }
    }
}
