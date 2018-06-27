using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Main;

namespace Quaver.States.Gameplay.HitObjects
{
    internal abstract class HitObjectManager
    {
        /// <summary>
        ///     All of the objects in the pool.
        /// </summary>
        internal List<HitObject> ObjectPool { get; }

        /// <summary>
        ///     The object pool size.
        /// </summary>
        internal int PoolSize { get; }

        /// <summary>
        ///     The number of objects left in the map
        ///     (Has to be implemented per game mode because pooling may be different.)
        /// </summary>
        internal abstract int ObjectsLeft { get; }

        /// <summary>
        ///     If there are no more objects and the map is complete.
        /// </summary>
        internal bool IsComplete => ObjectsLeft == 0;
        
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="size"></param>
        internal HitObjectManager(int size)
        {
            PoolSize = size;
            ObjectPool = new List<HitObject>(PoolSize);
        }
        
        /// <summary>
        ///     Plays the correct hitsounds based on the note index of the HitObjectPool.
        /// </summary>
        internal static void PlayObjectHitSounds(HitObjectInfo hitObject)
        {
            // Normal
            if (hitObject.HitSound == 0 || (HitSounds.Normal & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHit);

            // Clap
            if ((HitSounds.Clap & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHitClap);

            // Whistle
            if ((HitSounds.Whistle & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHitWhistle);

            // Finish
            if ((HitSounds.Finish & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHitFinish);
        }

        /// <summary>
        ///     Updates all the containing HitObjects
        /// </summary>
        /// <param name="dt"></param>
        internal abstract void Update(double dt);
    }
}