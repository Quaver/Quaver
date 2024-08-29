using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch
{
    [MoonSharpUserData]
    public class EditorActionPlaceHitObjectBatch : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.PlaceHitObjectBatch;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        [MoonSharpVisible(false)]
        public EditorActionPlaceHitObjectBatch(EditorActionManager actionManager, Qua workingMap, List<HitObjectInfo> hitObjects)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            HitObjects = hitObjects;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            WorkingMap.HitObjects.InsertSorted(HitObjects);
            ActionManager.TriggerEvent(EditorActionType.PlaceHitObjectBatch, new EditorHitObjectBatchPlacedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionRemoveHitObjectBatch(ActionManager, WorkingMap, HitObjects)?.Perform();
    }
}
