using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Reverse
{
    [MoonSharpUserData]
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
        public List<HitObjectInfo> HitObjects { get; }

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
            var start = HitObjects[0].StartTime;
            var end = Math.Max(HitObjects[^1].StartTime, HitObjects.Max(h => h.EndTime));

            foreach (var h in HitObjects)
                if (h.IsLongNote)
                {
                    var lnStart = h.StartTime;
                    h.StartTime = end - (h.EndTime - start);
                    h.EndTime = end - (lnStart - start);
                }
                else
                    h.StartTime = end - (h.StartTime - start);

            WorkingMap.SortHitObjects();
            ActionManager.TriggerEvent(EditorActionType.ReverseHitObjects, new EditorHitObjectsReversedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => Perform();
    }
}
