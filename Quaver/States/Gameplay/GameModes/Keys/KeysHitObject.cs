using Quaver.API.Maps;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.GameModes.Keys
{
    internal class KeysHitObject : HitObject
    {
        /// <summary>
        ///     If the note is a long note.
        ///     In .qua format, long notes are defined as if the end time is greater than 0.
        /// </summary>
        internal bool IsLongNote => Info.EndTime > 0;

        /// <summary>
       ///     The Y position of the HitObject.
       /// </summary>
        internal float PositionY { get; set; }

        /// <summary>
        ///     The width of the object.
        /// </summary>
        internal float Width { get; set; }

        /// <summary>
        ///     The Y-Offset from the receptor.
        /// </summary>
        internal float OffsetYFromReceptor { get; set; }

        /// <summary>
        ///     The beat snap for this object.
        /// </summary>
        internal BeatSnap Snap { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="info"></param>
        public KeysHitObject(HitObjectInfo info) : base(info)
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="playfield"></param>
        internal override void Initialize(IGameplayPlayfield playfield)
        {
        }
    }
}