using System.Collections;
using System.Collections.Generic;
using System.IO;
using Quaver.Config;
using Quaver.Cache;
using Quaver.Qua;
using UnityEngine;

namespace Quaver.Audio
{
	public class AudioPlayer
	{
		// This is purely responsible for loading a beatmap's audio file.
		// It will take in a cached map, parse it, and load it's audio from the file system.
		public static void LoadSong(CachedBeatmap map, AudioSource gameAudio)
		{
			// Parse the cached beatmap, and find the audio file from it
			if (!File.Exists(map.AudioPath))
			{
				Debug.LogError("[AUDIO PLAYER] Error: File cannot be played because it was not found!");
				return;
			}

            string url = "file:///" + map.AudioPath;

            WWW audioLoader = new WWW(url);

            while (!audioLoader.isDone)
            {
                Debug.Log("[AUDIO PLAYER] Loading Audio Track: " + url);
            }

            if (audioLoader.isDone)
            {
                gameAudio.clip = audioLoader.GetAudioClip(false, false, AudioType.OGGVORBIS);

                if (!gameAudio.isPlaying && gameAudio.clip.isReadyToPlay)
                {
                    Debug.Log("[AUDIO PLAYER] Audio Track Loaded! Starting Song.");
                    gameAudio.Play();
                }
            }
			
		}
	}
}