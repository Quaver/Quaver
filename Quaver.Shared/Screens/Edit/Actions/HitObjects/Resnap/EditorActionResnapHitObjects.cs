using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch
{
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
        ///     Unmodified notes, kept for undoing
        /// </summary>
        private List<HitObjectInfo> OldNotes { get; } = new List<HitObjectInfo>();

        /// <summary>
        ///     Modified notes, kept for undoing
        /// </summary>
        private List<HitObjectInfo> NewNotes { get; } = new List<HitObjectInfo>();

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="hitObjects"></param>
        public EditorActionResnapHitObjects(EditorActionManager actionManager, Qua workingMap, List<int> snaps)
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            Snaps = snaps;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Perform()
        {

            // Using AudioEngine.GetNearestSnapTimeFromTime is unreliable since it might not return the current snap
            foreach (var note in WorkingMap.HitObjects)
            {
                var startTimeDelta = DiffToClosestSnap(note.StartTime);
                var endTimeDelta = DiffToClosestSnap(note.EndTime);
                var startTimeNotSnapped = Math.Abs(startTimeDelta) >= 1;
                var endTimeNotSnapped = Math.Abs(endTimeDelta) >= 1 && note.IsLongNote;

                if (!startTimeNotSnapped && !endTimeNotSnapped)
                    continue;

                var resnappedNote = Helpers.ObjectHelper.DeepClone(note);

                if (startTimeNotSnapped)
                {
                    resnappedNote.StartTime = (int)(resnappedNote.StartTime - startTimeDelta);
                    Logger.Debug($"Resnapped {note.StartTime}|{note.Lane} to {resnappedNote.StartTime}", LogType.Runtime, false);
                }

                if (endTimeNotSnapped)
                {
                    resnappedNote.EndTime = (int)(resnappedNote.EndTime - endTimeDelta);
                    Logger.Debug($"Resnapped {note.EndTime}|{note.Lane} to {resnappedNote.EndTime} (Long Note End)", LogType.Runtime, false);
                }

                OldNotes.Add(note);
                NewNotes.Add(resnappedNote);
            }

            if (OldNotes.Count() > 0)
            {
                OldNotes.ForEach(n => WorkingMap.HitObjects.Remove(n));
                WorkingMap.HitObjects.AddRange(NewNotes);
                WorkingMap.Sort();
                NotificationManager.Show(NotificationLevel.Info, $"Resnapped {NewNotes.Count} notes");
                ActionManager.TriggerEvent(EditorActionType.ResnapHitObjects, new EditorActionHitObjectsResnappedEventArgs(Snaps));
            }
            else
                NotificationManager.Show(NotificationLevel.Info, $"No notes to resnap");

        }

        private float DiffToClosestSnap(int time) {
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

            return smallestDelta;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Undo()
        {
            NewNotes.ForEach(n => WorkingMap.HitObjects.Remove(n));
            WorkingMap.HitObjects.AddRange(OldNotes);
            WorkingMap.Sort();
        }
    }
}
