using System;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize
{
    public class EditorLongNoteResizedEventArgs : EventArgs
    {
        /// <summary>
        /// </summary>
        public HitObjectInfo HitObject { get; }

        /// <summary>
        /// </summary>
        public int OriginalTime { get; }

        /// <summary>
        /// </summary>
        public int NewTime { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="originalTime"></param>
        /// <param name="newTime"></param>
        public EditorLongNoteResizedEventArgs(HitObjectInfo hitObject, int originalTime, int newTime)
        {
            HitObject = hitObject;
            OriginalTime = originalTime;
            NewTime = newTime;
        }
    }
}