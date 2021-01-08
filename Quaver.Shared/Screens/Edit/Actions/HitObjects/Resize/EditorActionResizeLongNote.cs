using System;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Resize
{
    [MoonSharpUserData]
    public class EditorActionResizeLongNote : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.ResizeLongNote;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua Map { get; }

        /// <summary>
        /// </summary>
        private HitObjectInfo HitObject { get; }

        /// <summary>
        ///     The original end time of the long note
        /// </summary>
        private int OriginalTime { get; }

        /// <summary>
        ///     The new end time of the long note
        /// </summary>
        private int NewTime { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObject"></param>
        /// <param name="originalTime"></param>
        /// <param name="newTime"></param>
        [MoonSharpVisible(false)]
        public EditorActionResizeLongNote(EditorActionManager actionManager, Qua workingMap, HitObjectInfo hitObject,
            int originalTime, int newTime)
        {
            ActionManager = actionManager;
            Map = workingMap;
            HitObject = hitObject;

            OriginalTime = originalTime;
            NewTime = newTime;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            HitObject.EndTime = NewTime;
            ActionManager.TriggerEvent(Type, new EditorLongNoteResizedEventArgs(HitObject, OriginalTime, NewTime));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionResizeLongNote(ActionManager, Map, HitObject, NewTime, OriginalTime).Perform();
    }
}
