using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Swap
{
    public class EditorActionSwapLanes : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.SwapLanes;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjects { get; }

        private int SwapLane1 { get; }
        private int SwapLane2 { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        /// <param name="swapLane1"></param>
        /// <param name="swapLane2"></param>
        public EditorActionSwapLanes(EditorActionManager actionManager, Qua workingMap, List<HitObjectInfo> hitObjects,
            int swapLane1, int swapLane2)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            HitObjects = hitObjects;
            SwapLane1 = swapLane1;
            SwapLane2 = swapLane2;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            foreach (var h in HitObjects)
            {
                if (h.Lane == SwapLane1)
                    h.Lane = SwapLane2;
                else if (h.Lane == SwapLane2)
                    h.Lane = SwapLane1;
            }

            ActionManager.TriggerEvent(EditorActionType.SwapLanes, new EditorLanesSwappedEventArgs(HitObjects, SwapLane1, SwapLane2));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo() => Perform();
    }
}