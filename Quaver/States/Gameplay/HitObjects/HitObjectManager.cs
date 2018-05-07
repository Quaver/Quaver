using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Main;

namespace Quaver.States.Gameplay.HitObjects
{
    internal class HitObjectManager
    {
        /// <summary>
        ///     All of the objects in the pool.
        /// </summary>
        internal List<HitObject> ObjectPool { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="size"></param>
        internal HitObjectManager(int size)
        {
            ObjectPool = new List<HitObject>(size);
        }
        
        /// <summary>
        ///     Plays the correct hitsounds based on the note index of the HitObjectPool.
        /// </summary>
        internal void PlayObjectHitSounds(int index)
        {
            var hitObject = ObjectPool[index].Info;

            // Normal
            if (hitObject.HitSound == 0 || (HitSounds.Normal & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundHit);

            // Clap
            if ((HitSounds.Clap & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundHitClap);

            // Whistle
            if ((HitSounds.Whistle & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundHitWhistle);

            // Finish
            if ((HitSounds.Finish & hitObject.HitSound) != 0)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundHitFinish);
        }
    }
}