using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch
{
    [MoonSharpUserData]
    public class EditorActionRemoveHitObjectBatch : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.RemoveHitObjectBatch;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        [MoonSharpVisible(false)]
        public EditorActionRemoveHitObjectBatch(EditorActionManager actionManager, Qua workingMap, List<HitObjectInfo> hitObjects)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;

            // create a copy since it is highly likely the original list is
            // the list of selected hitobjects that will be modified when this action is performed
            HitObjects = new List<HitObjectInfo>(hitObjects);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            HitObjects.ForEach(x => WorkingMap.HitObjects.Remove(x));
            WorkingMap.Sort();

            HitObjects.ForEach(x => ActionManager.EditScreen.SelectedHitObjects.Remove(x));

            ActionManager.TriggerEvent(EditorActionType.RemoveHitObjectBatch, new EditorHitObjectBatchRemovedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo() => new EditorActionPlaceHitObjectBatch(ActionManager, WorkingMap, HitObjects)?.Perform();
    }
}
