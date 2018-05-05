namespace Quaver.States.Gameplay.Keys
{
    internal class KeysHitObject : HitObject
    {
        /// <summary>
        ///     If the note is a long note.
        ///     In .qua format, long notes are defined as if the end time is greater than 0.
        /// </summary>
        internal bool IsLongNote => Info.EndTime > 0;
    }
}