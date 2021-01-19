using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Wobble.Logging;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.Resnap
{
    [MoonSharpUserData]
    public class EditorActionResnapHitObjects : IEditorAction
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorActionType Type { get; } = EditorActionType.ResnapHitObjects;

        /// <summary>
        /// </summary>
        private EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private List<int> Snaps { get; }

        /// <summary>
        /// </summary>
        private List<HitObjectInfo> HitObjectsToResnap { get; }

        /// <summary>
        ///  The distance of note start/end to the closest snap
        /// </summary>
        /// <typeparam name="HitObjectInfo">Note</typeparam>
        /// <typeparam name="(int">Distance of StartTime to closest snap</typeparam>
        /// <typeparam name="int)">Distance of EndTime to closest snap</typeparam>
        private readonly Dictionary<HitObjectInfo, (int, int)> noteTimeAdjustments = new Dictionary<HitObjectInfo, (int, int)>();

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        [MoonSharpVisible(false)]
        public EditorActionResnapHitObjects(EditorActionManager actionManager, Qua workingMap, List<int> snaps, List<HitObjectInfo> hitObjectsToResnap)
        {
            ActionManager = actionManager;
            HitObjectsToResnap = hitObjectsToResnap;
            WorkingMap = workingMap;
            Snaps = snaps;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            foreach (var note in HitObjectsToResnap)
            {
                // Using AudioEngine.GetNearestSnapTimeFromTime is unreliable since it might not return the current snap
                var startTimeDelta = DiffToClosestSnap(note.StartTime);

                if (startTimeDelta != 0)
                    note.StartTime -= startTimeDelta;

                var endTimeDelta = 0;

                if (note.IsLongNote)
                {
                    endTimeDelta = DiffToClosestSnap(note.EndTime);

                    if (endTimeDelta != 0f)
                        note.EndTime -= endTimeDelta;
                }

                noteTimeAdjustments.Add(note, (startTimeDelta, endTimeDelta));
            }

            var offsnapCount = noteTimeAdjustments.Values.Count(x => x.Item1 != 0 || x.Item2 != 0);

            if (offsnapCount > 0)
            {
                var notifMessage = $"Resnapped {offsnapCount} note{(offsnapCount == 1 ? "" : "s")}";
                NotificationManager.Show(NotificationLevel.Info, notifMessage);

                ActionManager.TriggerEvent(EditorActionType.ResnapHitObjects, new EditorActionHitObjectsResnappedEventArgs(Snaps, HitObjectsToResnap));
            }
            else
                NotificationManager.Show(NotificationLevel.Info, $"No notes to resnap");

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int DiffToClosestSnap(int time)
        {
            var timingPoint = WorkingMap.GetTimingPointAt(time);
            var msPerSnaps = Snaps.Select(s => timingPoint.MillisecondsPerBeat / s).ToList();

            var smallestDelta = float.MaxValue;

            foreach (var msPerSnap in msPerSnaps)
            {
                var deltaForward = (time - timingPoint.StartTime) % msPerSnap;
                var deltaBackward = deltaForward - msPerSnap;
                var delta = deltaForward < -deltaBackward ? deltaForward : deltaBackward;

                if (Math.Abs(delta) < Math.Abs(smallestDelta))
                    smallestDelta = delta;
            }

            return (int)Math.Round(smallestDelta);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo()
        {
            foreach (var adjustment in noteTimeAdjustments)
            {
                var note = adjustment.Key;
                note.StartTime += adjustment.Value.Item1;
                note.EndTime += adjustment.Value.Item2;
            }

            ActionManager.TriggerEvent(EditorActionType.ResnapHitObjects, new EditorActionHitObjectsResnappedEventArgs(Snaps, HitObjectsToResnap));

            var offsnapCount = noteTimeAdjustments.Values.Count(x => x.Item1 != 0 || x.Item2 != 0);
            var notifMessage = $"Unsnapped {offsnapCount} note{(offsnapCount == 1 ? "" : "s")}";
            NotificationManager.Show(NotificationLevel.Info, notifMessage);

            noteTimeAdjustments.Clear();
        }
    }
}