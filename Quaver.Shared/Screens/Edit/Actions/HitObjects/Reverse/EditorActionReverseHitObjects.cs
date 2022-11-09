using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Reverse
{
    public class EditorActionReverseHitObjects : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.ReverseHitObjects;

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
        public EditorActionReverseHitObjects(EditorActionManager actionManager, Qua workingMap, List<HitObjectInfo> hitObjects)
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
            var start = HitObjects.Min(h => h.StartTime);
            var end = HitObjects.Max(h => Math.Max(h.StartTime, h.EndTime));

            foreach (var h in HitObjects)
            {
                if (h.IsLongNote)
                {
                    var lnStart = h.StartTime;
                    h.StartTime = end - (h.EndTime - start);
                    h.EndTime = end - (lnStart - start);
                }
                else
                {
                    h.StartTime = end - (h.StartTime - start);
                }
            }

            WorkingMap.Sort();
            ActionManager.TriggerEvent(EditorActionType.ReverseHitObjects, new EditorHitObjectsReversedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => Perform();
    }
}