using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Move
{
    [MoonSharpUserData]
    public class EditorActionMoveHitObjects : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.MoveHitObjects;

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
        ///     The value in which the objects lanes will be added to.
        ///     Example: An object originally at lane 1 moved at a lane offset of +2 will be newly in lane 3.
        /// </summary>
        public int LaneOffset { get; }

        /// <summary>
        ///     The offset at which the objects have been dragged
        /// </summary>
        public int DragOffset { get; }

        /// <summary>
        ///     If true, <see cref="Perform"/> will do any moving of the objects.
        ///     If false, only the event will be called
        /// </summary>
        public bool ShouldPerform { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        /// <param name="laneOffset"></param>
        /// <param name="dragOffset"></param>
        /// <param name="shouldPerform"></param>
        public EditorActionMoveHitObjects(EditorActionManager actionManager, Qua workingMap,
            List<HitObjectInfo> hitObjects, int laneOffset, int dragOffset, bool shouldPerform = true)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            HitObjects = hitObjects;
            LaneOffset = laneOffset;
            DragOffset = dragOffset;
            ShouldPerform = shouldPerform;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {
            if (ShouldPerform)
            {
                foreach (var h in HitObjects)
                {
                    h.StartTime += DragOffset;

                    if (h.IsLongNote)
                        h.EndTime += DragOffset;

                    h.Lane += LaneOffset;
                }
            }

            WorkingMap.HitObjects.HybridSort();
            ActionManager.TriggerEvent(EditorActionType.MoveHitObjects, new EditorHitObjectsMovedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            new EditorActionMoveHitObjects(ActionManager, WorkingMap, HitObjects, -LaneOffset, -DragOffset).Perform();

            // Set ShouldPerform back to true, because the action should be called if the user uses the "redo" function
            ShouldPerform = true;
        }
    }
}
