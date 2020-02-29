using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Move
{
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
        private List<HitObjectInfo> HitObjects { get; }

        /// <summary>
        ///     The value in which the objects lanes will be added to.
        ///     Example: An object originally at lane 1 moved at a lane offset of +2 will be newly in lane 3.
        /// </summary>
        private int LaneOffset { get; }

        /// <summary>
        ///     The start time of the very first selected object
        /// </summary>
        private int StartTime { get; }

        /// <summary>
        ///     The offset at which the objects have been dragged
        /// </summary>
        private int DragOffset { get; }

        /// <summary>
        ///     If true, <see cref="Perform"/> will do any moving of the objects.
        ///     If false, only the event will be called
        /// </summary>
        private bool ShouldPerform { get; }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        /// <param name="laneOffset"></param>
        /// <param name="startTime"></param>
        /// <param name="dragOffset"></param>
        /// <param name="shouldPerform"></param>
        public EditorActionMoveHitObjects(EditorActionManager actionManager, Qua workingMap,
            List<HitObjectInfo> hitObjects, int laneOffset, int startTime, int dragOffset, bool shouldPerform = true)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            HitObjects = hitObjects;
            LaneOffset = laneOffset;
            StartTime = startTime;
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
                }
            }

            ActionManager.TriggerEvent(EditorActionType.MoveHitObjects, new EditorHitObjectsMovedEventArgs(HitObjects));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            new EditorActionMoveHitObjects(ActionManager, WorkingMap, HitObjects, LaneOffset,
                StartTime, -DragOffset).Perform();
        }
    }
}