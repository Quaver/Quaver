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
using Quaver.Utility;

namespace Quaver.Audio
{
    internal class GameEffect : GameAudio
    {
        /// <summary>
        ///     The raw file data containing the SFX
        /// </summary>
        private byte[] Data { get; set; }

        /// <summary>
        ///     The path of the file, if there is one.
        /// </summary>
        private string Path { get; set; }

        /// <summary>
        ///     Constructor - Takes in a Stream and converts it into a byte array containing
        ///     the raw file data. This is then used to create a BASS stream to be used as a game effect.
        ///     * Used for embedded resource effects
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="effect"></param>
        public GameEffect(Stream stream)
        {
            IsEffect = true;

            try
            {
                // Convert the stream to a byte array
                Data = Util.ConvertStreamToByteArray(stream);

                if (Data.Length == 0)
                {
                    Stream = 0;
                    return;
                }

                LoadAudioStream();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Ctor - 
        ///     Loads a GameEffect from a file path.
        ///     Used for skinned SFX.
        /// </summary>
        /// <param name="path"></param>
        public GameEffect(string path)
        {
            IsEffect = true;
            Path = path;

            if (!File.Exists(Path))
            {
                Stream = 0;
                return;
            }

            Console.WriteLine(Path);
            LoadAudioStream();
        }

        /// <summary>
        ///     Loads the audio stream again
        /// </summary>
        private void LoadAudioStream()
        {
            // Make sure BASS only gets initialized one time.
            if (!GameBase.BassInitialized)
            {
                Bass.Init();
                GameBase.BassInitialized = true;
            }

            // Create the stream from the raw file data
            var stream = 0;

            if (Data != null && Data.Length != 0)
                stream = Bass.CreateStream(Data, 0, Data.Length, BassFlags.Decode);

            // In the event that the stream data is non-existant, we must have been given a file path
            // so we'll run a check for that and end up loading the stream via that method.
            else if (Path != "")
            {
                stream = Bass.CreateStream(Path, Flags: BassFlags.Decode);
            }
            else
                return;

            if (stream != 0)
            {
                AudioHandler.GlobalAudioStreams.Add(Stream);
                Stream = stream;
            }

            Bass.ChannelAddFlag(Stream, BassFlags.AutoFree);
        }

        /// <summary>
        ///     Plays the track, but loads another stream since this is an effect.
        /// </summary>
        /// <param name="previewTime"></param>
        /// <param name="playbackRate"></param>
        /// <param name="pitch"></param>
        internal override void Play(double previewTime = 0, float playbackRate = 1.0f)
        {
            base.Play();
            LoadAudioStream();
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
