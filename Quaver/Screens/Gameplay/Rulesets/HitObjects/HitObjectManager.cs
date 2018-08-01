using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Skinning;
using Wobble;

namespace Quaver.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class HitObjectManager
    {
        /// <summary>
        ///     All of the objects in the pool.
        /// </summary>
        public List<GameplayHitObject> ObjectPool { get; }

        /// <summary>
        ///     The object pool size.
        /// </summary>
        public int PoolSize { get; }

        /// <summary>
        ///     The number of objects left in the map
        ///     (Has to be implemented per game mode because pooling may be different.)
        /// </summary>
        public abstract int ObjectsLeft { get; }

        /// <summary>
        ///     If there are no more objects and the map is complete.
        /// </summary>
        public bool IsComplete => ObjectsLeft == 0;

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="size"></param>
        public HitObjectManager(int size)
        {
            PoolSize = size;
            ObjectPool = new List<GameplayHitObject>(PoolSize);
        }

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

        /// <summary>
        ///     Updates all the containing HitObjects
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);
    }
}
