using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Config;
using Quaver.Skinning;
using Wobble;

namespace Quaver.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class HitObjectManager
    {
        /// <summary>
        ///     The number of objects left in the map
        ///     (Has to be implemented per game mode because pooling may be different.)
        /// </summary>
        public abstract int ObjectsLeft { get; }

        /// <summary>
        ///     The start time of the current earliest Hit Object in the Object Pool
        /// </summary>
        public abstract GameplayHitObject EarliestHitObject { get; }

        /// <summary>
        ///     If there are no more objects and the map is complete.
        /// </summary>
        public bool IsComplete => ObjectsLeft == 0;

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="size"></param>
        public HitObjectManager(Qua map) { }

        /// <summary>
        ///     Updates all the containing HitObjects
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        ///     Plays the correct hitsounds based on the note index of the HitObjectPool.
        /// </summary>
        public static void PlayObjectHitSounds(HitObjectInfo hitObject)
        {
            // Normal
            if (hitObject.HitSound == 0 || (HitSounds.Normal & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHit.CreateChannel().Play();

            // Clap
            if ((HitSounds.Clap & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHitClap.CreateChannel().Play();

            // Whistle
            if ((HitSounds.Whistle & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHitWhistle.CreateChannel().Play();

            // Finish
            if ((HitSounds.Finish & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHitFinish.CreateChannel().Play();
        }
    }
}
