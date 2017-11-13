﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Beatmaps;
using Quaver.Discord;
using Quaver.Graphics.Sprite;

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
                if (GameBase.SelectedBeatmap.Song != null)
                    GameBase.SelectedBeatmap.Song.Resume();
                else
                {
                    // Here we assume that the song hasn't been loaded since its length is 0,
                    // so we'll attempt to load it up and play it.
                    GameBase.SelectedBeatmap.LoadAudio();
                    GameBase.SelectedBeatmap.Song.Play();
                }

                // Set Rich Presence
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

            // Run a check if the selected map or song is currently null.
            if (GameBase.SelectedBeatmap == null || GameBase.SelectedBeatmap.Song == null)
                return;

            if (GameBase.SelectedBeatmap.Song.GetAudioPosition() < GameBase.SelectedBeatmap.Song.GetAudioLength())
                return;

            // Stop the current track
            GameBase.SelectedBeatmap.Song.Stop();

            // Select new map
            BeatmapUtils.SelectRandomBeatmap();

            // Load Audio
            GameBase.SelectedBeatmap.LoadAudio();

            // Load Background and change it
            GameBase.SelectedBeatmap.LoadBackground();
            BackgroundManager.Change(GameBase.SelectedBeatmap.Background);

            // Begin to play
            GameBase.SelectedBeatmap.Song.Play();

            // Set new Discord Rich Presence
            GameBase.ChangeDiscordPresence($"In the main menu listening to: {GameBase.SelectedBeatmap.Artist} - {GameBase.SelectedBeatmap.Title}");
        }
    }
}
