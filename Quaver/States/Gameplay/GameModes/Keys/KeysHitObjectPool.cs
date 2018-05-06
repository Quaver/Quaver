using Quaver.API.Gameplay;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.GameModes.Keys
{
    internal class KeysHitObjectPool : HitObjectPool
    {
        internal KeysHitObjectPool(int size) : base(size)
        {
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
            for (var i = 0; i < Size && i < Objects.Count; i++)
            {
                if (Objects[i].Info.Lane == lane && Objects[i].Info.StartTime - songTime > -JudgeWindow.Okay)
                    return i;
            }
            
            return -1;
        }
    }
}