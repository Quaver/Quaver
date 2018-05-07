using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Gameplay;
using Quaver.Config;
using Quaver.Main;
using Quaver.States.Gameplay.GameModes.Keys.Playfield;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.GameModes.Keys
{
    internal class KeysHitObjectManager : HitObjectManager
    {
        /// <summary>
        ///     Reference to the entire ruleset.
        /// </summary>
        private GameModeKeys Ruleset { get; }

        /// <summary>
        ///     The list of currently dead notes
        /// </summary>
        internal List<HitObject> DeadNotes { get; }

        /// <summary>
        ///     The list of long notes in the pool.
        /// </summary>
        internal List<HitObject> LongNotes { get; }
 
        /// <summary>
        ///     The speed at which objects fall down from the screen.
        /// </summary>
        internal float ScrollSpeed => ConfigManager.ScrollSpeed4K.Value / (20f * GameBase.AudioEngine.PlaybackRate);

        /// <summary>
        ///     The offset of the hit position.
        /// </summary>
        internal float HitPositionOffset
        {
            get
            {
                var playfield = (KeysPlayfield) Ruleset.Playfield;
                
                switch (Ruleset.Mode)
                {
                    case GameMode.Keys4:
                        if (ConfigManager.DownScroll4K.Value)
                            return playfield.ReceptorPositionY + (ConfigManager.UserHitPositionOffset4K.Value + GameBase.LoadedSkin.HitPositionOffset4K);
                        else
                            return playfield.ReceptorPositionY - (ConfigManager.UserHitPositionOffset4K.Value + GameBase.LoadedSkin.HitPositionOffset4K) + GameBase.LoadedSkin.ColumnSize4K;
                    case GameMode.Keys7:
                        if (ConfigManager.DownScroll7K.Value)
                            return playfield.ReceptorPositionY + (ConfigManager.UserHitPositionOffset7K.Value + GameBase.LoadedSkin.HitPositionOffset7K);
                        else
                            return playfield.ReceptorPositionY - (ConfigManager.UserHitPositionOffset7K.Value + GameBase.LoadedSkin.HitPositionOffset7K) + GameBase.LoadedSkin.ColumnSize7K;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        internal KeysHitObjectManager(GameModeKeys ruleset, int size) : base(size)
        {
            Ruleset = ruleset;
            
            DeadNotes = new List<HitObject>();
            LongNotes = new List<HitObject>();
        }

        /// <summary>
        ///     Gets the index of the nearest object in a given lane.
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="songTime"></param>
        /// <returns></returns>
        public int GetIndexOfNearestLaneObject(int lane, double songTime)
        {
            // Search for closest ManiaHitObject that is inside the HitTiming Window
            for (var i = 0; i < ObjectPool.Capacity && i < ObjectPool.Count; i++)
            {
                if (ObjectPool[i].Info.Lane == lane && ObjectPool[i].Info.StartTime - songTime > -JudgeWindow.Okay)
                    return i;
            }
            
            return -1;
        }
    }
}