using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Main;

namespace Quaver.Modifiers
{
    internal class Speed : IMod
    {
        /// <summary>
        ///     The name of the mod.
        /// </summary>
        public string Name { get; set; } = "Speed Modifier";

        /// <summary>
        ///     The identifier of the mod.
        /// </summary>
        public ModIdentifier ModIdentifier { get; set; } = ModIdentifier.Speed;

        /// <summary>
        ///     The type of mod as defined in the enum
        /// </summary>
        public ModType Type { get; set; } = ModType.Special;

        /// <summary>
        ///     The description of the Mod
        /// </summary>
        public string Description { get; set; } = "Sets the song's speed.";

        /// <summary>
        ///     Is the mod ranked?
        /// </summary>
        public bool Ranked { get; set; } = false;

        /// <summary>
        ///     The addition to the score multiplier this mod will have
        /// </summary>
        public float ScoreMultiplierAddition { get; set; } = 0;

        /// <summary>
        ///     The identifier of mods that are incompatible with this one.
        /// </summary>
        public ModIdentifier[] IncompatibleMods { get; set; } = { ModIdentifier.Speed };

        /// <summary>
        ///     The speed alteration rate the game's clock will be set to.
        /// </summary>
        private float SpeedAlterationRate { get; set; }

        /// <summary>
        ///     Sets the speed alteration rate. This is so we can have only one mod dedicated to speed.
        /// </summary>
        /// <param name="speedRate"></param>
        public Speed(float speedRate)
        {
            SpeedAlterationRate = speedRate;
        }

        /// <summary>
        ///     All the mod logic should go here, setting unique variables. NEVER call this directly. Always use
        ///     ModManager.AddMod();
        /// </summary>
        public void InitializeMod()
        {
            // Set the GameClock (Audio Speed) to the SpeedAlterationRate.
            GameBase.GameClock = SpeedAlterationRate;
            Console.WriteLine($"[MOD MANAGER] Speed is not set to {GameBase.GameClock}x");
        }
    }
}
