using Quaver.API.Maps;

namespace Quaver.States.Gameplay.HitObjects
{
    internal abstract class HitObject
    {
        /// <summary>
        ///     The info of this particular HitObject from the map file.
        /// </summary>
        internal HitObjectInfo Info { get; set; }

        /// <summary>
        ///     Initializes the HitObject.
        /// </summary>
        /// <param name="playfield"></param>
        internal abstract void Initialize(IGameplayPlayfield playfield);
    }
}