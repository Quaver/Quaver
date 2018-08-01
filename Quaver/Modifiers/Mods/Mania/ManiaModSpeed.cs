using System;
using System.ComponentModel;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Audio;
using Quaver.Logging;

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
        public ModIdentifier[] IncompatibleMods { get; set; } =
        {
            ModIdentifier.Speed05X,
            ModIdentifier.Speed06X,
            ModIdentifier.Speed07X,
            ModIdentifier.Speed08X,
            ModIdentifier.Speed09X,
            ModIdentifier.Speed11X,
            ModIdentifier.Speed12X,
            ModIdentifier.Speed13X,
            ModIdentifier.Speed14X,
            ModIdentifier.Speed15X,
            ModIdentifier.Speed16X,
            ModIdentifier.Speed17X,
            ModIdentifier.Speed18X,
            ModIdentifier.Speed19X,
            ModIdentifier.Speed20X,
        };

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
                    AudioEngine.Track.Rate = 0.5f;
                    break;
                case ModIdentifier.Speed06X:
                    AudioEngine.Track.Rate = 0.6f;
                    break;
                case ModIdentifier.Speed07X:
                    AudioEngine.Track.Rate = 0.7f;
                    break;
                case ModIdentifier.Speed08X:
                    AudioEngine.Track.Rate = 0.8f;
                    break;
                case ModIdentifier.Speed09X:
                    AudioEngine.Track.Rate = 0.9f;
                    break;
                case ModIdentifier.Speed11X:
                    AudioEngine.Track.Rate = 1.1f;
                    break;
                case ModIdentifier.Speed12X:
                    AudioEngine.Track.Rate = 1.2f;
                    break;
                case ModIdentifier.Speed13X:
                    AudioEngine.Track.Rate = 1.3f;
                    break;
                case ModIdentifier.Speed14X:
                    AudioEngine.Track.Rate = 1.4f;
                    break;
                case ModIdentifier.Speed15X:
                    AudioEngine.Track.Rate = 1.5f;
                    break;
                case ModIdentifier.Speed16X:
                    AudioEngine.Track.Rate = 1.6f;
                    break;
                case ModIdentifier.Speed17X:
                    AudioEngine.Track.Rate = 1.7f;
                    break;
                case ModIdentifier.Speed18X:
                    AudioEngine.Track.Rate = 1.8f;
                    break;
                case ModIdentifier.Speed19X:
                    AudioEngine.Track.Rate = 1.9f;
                    break;
                case ModIdentifier.Speed20X:
                    AudioEngine.Track.Rate = 2.0f;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            
            // Remove the incoming mod from the list of incompatible ones.
            var im = IncompatibleMods.ToList();
            im.Remove(modIdentifier);
            IncompatibleMods = im.ToArray();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod() { }
    }
}
