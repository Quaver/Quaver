using System;
using System.ComponentModel;
using System.Linq;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Audio;

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
        ///     Identifier (None. Speed is a ResultsScreenType and doesn't have an identifier)
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     ResultsScreenType
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
        public ManiaModSpeed(ModIdentifier modIdentifier) => ModIdentifier = modIdentifier;

        /// <inheritdoc />
        /// <summary>
        ///     Initialize
        /// </summary>
        public void InitializeMod()
        {
            AudioEngine.Track.Rate = ModHelper.GetRateFromMods(ModIdentifier);

            // Remove the incoming mod from the list of incompatible ones.
            var im = IncompatibleMods.ToList();
            im.Remove(ModIdentifier);
            IncompatibleMods = im.ToArray();
        }
    }
}
