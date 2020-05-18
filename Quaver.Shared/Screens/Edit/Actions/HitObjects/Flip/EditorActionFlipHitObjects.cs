using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip
{
    public class EditorActionFlipHitObjects : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.FlipHitObjects;

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
        public EditorActionFlipHitObjects(EditorActionManager actionManager, Qua workingMap, List<HitObjectInfo> hitObjects)
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
            foreach (var h in HitObjects)
                h.Lane = WorkingMap.GetKeyCount() - h.Lane + 1;

            ActionManager.TriggerEvent(EditorActionType.FlipHitObjects, new EditorHitObjectsFlippedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => Perform();
    }
}