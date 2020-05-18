using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch
{
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
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        public EditorActionPlaceHitObjectBatch(EditorActionManager actionManager, Qua workingMap, List<HitObjectInfo> hitObjects)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            HitObjects = hitObjects;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            HitObjects.ForEach(x => WorkingMap.HitObjects.Add(x));
            WorkingMap.Sort();

            ActionManager.TriggerEvent(EditorActionType.PlaceHitObjectBatch, new EditorHitObjectBatchPlacedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => new EditorActionRemoveHitObjectBatch(ActionManager, WorkingMap, HitObjects)?.Perform();
    }
}