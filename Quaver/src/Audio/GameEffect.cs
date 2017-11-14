using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Config;
using ManagedBass;
using Microsoft.Xna.Framework;
using Quaver.Logging;

namespace Quaver.Audio
{
    internal class GameEffect : GameAudio
    {
        /// <summary>
        ///     Constructor - Turns an audio stream into a temp file, then proceeds to load it. 
        ///     ManagedBass doesn't seem to have to ability to create a stream directly from 
        ///     a Streaam instance, so this seems to be a better option.
        ///     TODO: Explore this more, it's a bit hacky.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="effect"></param>
        public GameEffect(Stream stream)
        {
            try
            {
                byte[] buffer = new byte[8 * 1024];

                // Create a random name for the file
                var random = new Random();
                var chars = "abcdefghijklmnopqrstuvwxyz0123456789";
                var randFileName = new string(Enumerable.Repeat(chars, 25).Select(s => s[random.Next(s.Length)]).ToArray());

                // Create file with the audio stream
                var path = Config.Configuration.DataDirectory + $"/{randFileName}.mp3";
                var file = File.Create(path);
                int len;
                while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    file.Write(buffer, 0, len);
                }

                file.Close();

                Path = path;

                // Proceed to load the audio stream as normal.
                LoadAudioStream(path);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Color.Red);
            }
        }

        /// <summary>
        ///     Plays the track, but loads another stream since this is an effect.
        /// </summary>
        /// <param name="previewTime"></param>
        /// <param name="playbackRate"></param>
        /// <param name="pitch"></param>
        internal override void Play(double previewTime = 0, float playbackRate = 1.0f, bool pitch = false)
        {
            base.Play();
            LoadAudioStream(Path);
        }

        /// <summary>
        ///     Sets the audio volume to that of what is in the config file, depending on if it is an effect or not.
        /// </summary>
        internal override void ChangeAudioVolume()
        {
            var volume = (float)Config.Configuration.VolumeEffect / 100;

            Bass.ChannelSetAttribute(Stream, ChannelAttribute.Volume, volume);
        }
    }
}
