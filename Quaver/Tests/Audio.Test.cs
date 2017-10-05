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
                @"C:\Users\swan\AppData\Local\osu!\Songs\516109 Aitsuki Nakuru - Monochrome Butterfly\audio.mp3";

            if (!File.Exists(filePath))
            {
                Console.WriteLine("[DEBUG] Audio File not found, so test cannot be run! Check your path!");
                return;
            }

            var stream = AudioLoader.LoadAudioStream(filePath);
            var positionInMilliseconds = 52012;
            AudioLoader.PlayAudioStream(stream, positionInMilliseconds);
            Console.WriteLine("[DEBUG] Waiting 2 seconds to pause audio.");
            Thread.Sleep(2000);
            AudioLoader.PauseAudioStream(stream);
            Console.WriteLine("[DEBUG] Waiting 5 seconds to resume audio");
            Thread.Sleep(5000);
            AudioLoader.ResumeAudioStream(stream);
        }
    }
}
