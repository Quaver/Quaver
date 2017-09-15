// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
        public static void LoadSong(CachedBeatmap map, AudioSource gameAudio, bool usePreviewTime = false, float playDelay = 0f)
        {
            // Parse the cached beatmap, and find the audio file from it
            if (!File.Exists(map.AudioPath))
            {
                Debug.LogError("[AUDIO PLAYER] Error: File cannot be played because it was not found!");
                return;
            }
            else
            {
                GameObject obj = new GameObject("Audio Buffer");
                obj.AddComponent<AudioBufferer>().init(map, gameAudio, usePreviewTime, playDelay);
            }
        }
    }
}