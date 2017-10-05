using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ManagedBass;
using Quaver.Audio;

namespace Quaver.Tests
{
    internal static class AudioTest
    {
        internal static void PlaySongPreview(bool run)
        {
            if (!run)
                return;

            const string filePath =
                @"C:\Users\swan\Desktop\New folder\1. Camellia - WHAT THE CAT!\audio.ogg";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("[DEBUG] Audio File not found, so test cannot be run! Check your path!");
                return;
            }

            GameAudio song = new GameAudio(filePath);

            const double positionInMilliseconds = 1000;
            song.Play(positionInMilliseconds);
            Console.WriteLine("[DEBUG] Current Audio Position - Started: " + song.GetAudioPosition());
            Thread.Sleep(2000);

            /*song.Pause();
            Console.WriteLine("[DEBUG] Current Audio Position - Paused: " + song.GetAudioPosition());
            Console.WriteLine("[DEBUG] Waiting 5 seconds to resume audio");
            Thread.Sleep(5000);

            song.Resume();
            Console.WriteLine("[DEBUG] Current Audio Position - Resumed: " + song.GetAudioPosition());*/
        }
    }
}
