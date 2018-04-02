using System;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.Modifiers.Mods.Mania 
{
    internal class ManiaModSpeed : IGameplayModifier
    {
        /// <inheritdoc />
        /// <summary>
        ///     Name
        /// </summary>
        public string Name { get; set; } = "ManiaModSpeed";

        /// <inheritdoc />
        /// <summary>
        ///     Identifier (None. Speed is a Type and doesn't have an identifier)
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Type
        /// </summary>
        public ModType Type { get; set; } = ModType.Speed;

        /// <inheritdoc />
        /// <summary>
        ///     Desc
        /// </summary>
        public string Description { get; set; } = "Change the speed of the song!";

        /// <inheritdoc />
        /// <summary>
        ///     Ranked
        /// </summary>
        public bool Ranked { get; set; } = true;

        /// <inheritdoc />
        /// <summary>
        ///     Score x
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = 0;

        /// <inheritdoc />
        /// <summary>
        ///     Incompatible Mods
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; }

        /// <summary>
        ///     Ctor - Set speed
        /// </summary>
        /// <param name="modIdentifier"></param>
        public ManiaModSpeed(ModIdentifier modIdentifier)
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

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod()
        {
            GameBase.AudioEngine.SetPlaybackRate();
            Logger.LogImportant($"ManiaModSpeed is now set to {GameBase.AudioEngine.PlaybackRate}x", LogType.Runtime);
        }
    }
}
