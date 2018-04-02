using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.Logging;

namespace Quaver.Modifiers.Mods 
{
    internal class Speed : IMod
    {
        public string Name { get; set; } = "Speed";

        public ModIdentifier ModIdentifier { get; set; }

        public ModType Type { get; set; } = ModType.Speed;

        public string Description { get; set; } = "Change the speed of the song!";

        public bool Ranked { get; set; } = true;

        public float ScoreMultiplierAddition { get; set; } = 0;

        public ModIdentifier[] IncompatibleMods { get; set; }

        public Speed(ModIdentifier modIdentifier)
        {
            ModIdentifier = modIdentifier;

            switch (modIdentifier)
            {
                case ModIdentifier.Speed05X:
                    GameBase.AudioEngine.PlaybackRate = 0.5f;
                    break;
                case ModIdentifier.Speed06X:
                    GameBase.AudioEngine.PlaybackRate = 0.6f;
                    break;
                case ModIdentifier.Speed07X:
                    GameBase.AudioEngine.PlaybackRate = 0.7f;
                    break;
                case ModIdentifier.Speed08X:
                    GameBase.AudioEngine.PlaybackRate = 0.8f;
                    break;
                case ModIdentifier.Speed09X:
                    GameBase.AudioEngine.PlaybackRate = 0.9f;
                    break;
                case ModIdentifier.Speed11X:
                    GameBase.AudioEngine.PlaybackRate = 1.1f;
                    break;
                case ModIdentifier.Speed12X:
                    GameBase.AudioEngine.PlaybackRate = 1.2f;
                    break;
                case ModIdentifier.Speed13X:
                    GameBase.AudioEngine.PlaybackRate = 1.3f;
                    break;
                case ModIdentifier.Speed14X:
                    GameBase.AudioEngine.PlaybackRate = 1.4f;
                    break;
                case ModIdentifier.Speed15X:
                    GameBase.AudioEngine.PlaybackRate = 1.5f;
                    break;
                case ModIdentifier.Speed16X:
                    GameBase.AudioEngine.PlaybackRate = 1.6f;
                    break;
                case ModIdentifier.Speed17X:
                    GameBase.AudioEngine.PlaybackRate = 1.7f;
                    break;
                case ModIdentifier.Speed18X:
                    GameBase.AudioEngine.PlaybackRate = 1.8f;
                    break;
                case ModIdentifier.Speed19X:
                    GameBase.AudioEngine.PlaybackRate = 1.9f;
                    break;
                case ModIdentifier.Speed20X:
                    GameBase.AudioEngine.PlaybackRate = 2.0f;
                    break;
            }

            IncompatibleMods = Enum
                .GetValues(typeof(ModIdentifier))
                .Cast<ModIdentifier>()
                .Where(w => w != modIdentifier)
                .ToArray();
        }

        public void InitializeMod()
        {
            GameBase.AudioEngine.SetPlaybackRate();
            Logger.LogImportant($"Speed is now set to {GameBase.AudioEngine.PlaybackRate}x", LogType.Runtime);
        }
    }
}
