using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
                    GameBase.GameClock = 0.5f;
                    break;
                case ModIdentifier.Speed06X:
                    GameBase.GameClock = 0.6f;
                    break;
                case ModIdentifier.Speed07X:
                    GameBase.GameClock = 0.7f;
                    break;
                case ModIdentifier.Speed08X:
                    GameBase.GameClock = 0.8f;
                    break;
                case ModIdentifier.Speed09X:
                    GameBase.GameClock = 0.9f;
                    break;
                case ModIdentifier.Speed11X:
                    GameBase.GameClock = 1.1f;
                    break;
                case ModIdentifier.Speed12X:
                    GameBase.GameClock = 1.2f;
                    break;
                case ModIdentifier.Speed13X:
                    GameBase.GameClock = 1.3f;
                    break;
                case ModIdentifier.Speed14X:
                    GameBase.GameClock = 1.4f;
                    break;
                case ModIdentifier.Speed15X:
                    GameBase.GameClock = 1.5f;
                    break;
                case ModIdentifier.Speed16X:
                    GameBase.GameClock = 1.6f;
                    break;
                case ModIdentifier.Speed17X:
                    GameBase.GameClock = 1.7f;
                    break;
                case ModIdentifier.Speed18X:
                    GameBase.GameClock = 1.8f;
                    break;
                case ModIdentifier.Speed19X:
                    GameBase.GameClock = 1.9f;
                    break;
                case ModIdentifier.Speed20X:
                    GameBase.GameClock = 2.0f;
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
            Logger.Log($"Speed is now set to {GameBase.GameClock}x", Color.Cyan);

            SongManager.ChangeSongSpeed();
        }
    }
}
