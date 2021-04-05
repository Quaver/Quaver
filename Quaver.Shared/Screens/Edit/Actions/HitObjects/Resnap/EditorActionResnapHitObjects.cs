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
using Quaver.Shared.Audio;
using Wobble.Graphics;

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
        ///     The original and new time(s) for each note
        /// </summary>
        private readonly Dictionary<HitObjectInfo, NoteAdjustment> noteTimeAdjustments =
            new Dictionary<HitObjectInfo, NoteAdjustment>();

        /// <summary>
        /// </summary>
        public bool ShowNotif { get; }

        private struct NoteAdjustment
        {
            public int OriginalStartTime;
            public int OriginalEndTime;
            public int NewStartTime;
            public int NewEndTime;
            public bool IsLongNote;

            public NoteAdjustment(int originalStartTime, int originalEndTime, HitObjectInfo note)
            {
                OriginalStartTime = originalStartTime;
                OriginalEndTime = originalEndTime;
                NewStartTime = note.StartTime;
                NewEndTime = note.EndTime;
                IsLongNote = note.IsLongNote;
            }

            /// <summary>
            ///  Whether the start time was changed
            /// </summary>
            public bool StartTimeWasChanged => OriginalStartTime != NewStartTime;

            /// <summary>
            /// Whether the end time was changed
            /// </summary>
            public bool EndTimeWasChanged => IsLongNote && OriginalEndTime != NewEndTime;

            /// <summary>
            /// Whether the note was moved at all
            /// </summary>
            public bool NoteWasMoved => StartTimeWasChanged || EndTimeWasChanged;
        }

        /// <summary>
        /// </summary>
        /// <param name="actionManager"></param>
        /// <param name="workingMap"></param>
        /// <param name="snaps"></param>
        /// <param name="hitObjectsToResnap"></param>
        /// <param name="showNotif"></param>
        [MoonSharpVisible(false)]
        public EditorActionResnapHitObjects(EditorActionManager actionManager, Qua workingMap, List<int> snaps,
            List<HitObjectInfo> hitObjectsToResnap, bool showNotif)
        {
            ActionManager = actionManager;
            HitObjectsToResnap = hitObjectsToResnap;
            WorkingMap = workingMap;
            Snaps = snaps;
            ShowNotif = showNotif;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Perform()
        {
            var resnapCount = 0;

            foreach (var note in HitObjectsToResnap)
            {
                var originalStartTime = note.StartTime;
                var originalEndTime = note.EndTime;

                note.StartTime = ClosestTickOverall(note.StartTime);
                if (note.IsLongNote)
                    note.EndTime = ClosestTickOverall(note.EndTime);

                var adjustment = new NoteAdjustment(originalStartTime, originalEndTime, note);
                if (adjustment.NoteWasMoved)
                {
                    noteTimeAdjustments.Add(note, adjustment);
                    resnapCount++;
                }
            }

            if (resnapCount > 0)
            {
                if (ShowNotif)
                {
                    var notifMessage = $"Resnapped {resnapCount} note{(resnapCount == 1 ? "" : "s")}";
                    NotificationManager.Show(NotificationLevel.Info, notifMessage);
                }

                ActionManager.TriggerEvent(EditorActionType.ResnapHitObjects,
                    new EditorActionHitObjectsResnappedEventArgs(Snaps, HitObjectsToResnap, ShowNotif));
            }
            else if (ShowNotif)
            {
                NotificationManager.Show(NotificationLevel.Info, $"No notes resnapped");
            }
        }

        /// <summary>
        ///    Gets the closest time overall
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int ClosestTickOverall(int time)
        {
            var closestTime = int.MaxValue;
            foreach (var newTime in Snaps.Select(snap => ClosestTickToSnap(time, snap)))
            {
                if (Math.Abs(time - newTime) < Math.Abs(time - closestTime))
                    closestTime = newTime;
            }

            return closestTime;
        }

        /// <summary>
        ///     Gets the closest time for a given snap, uses the same algorithm as EditorPlayfield.GetNearestTickFromTime()
        /// </summary>
        /// <param name="time"></param>
        /// <param name="snap"></param>
        /// <returns></returns>
        private int ClosestTickToSnap(int time, int snap)
        {
            var timingPoint = WorkingMap.GetTimingPointAt(time);
            if (timingPoint == null)
                return time;

            var timeFwd = (int) AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, Direction.Forward, snap, time);
            var timeBwd = (int) AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, Direction.Backward, snap, time);

            var fwdDiff = Math.Abs(time - timeFwd);
            var bwdDiff = Math.Abs(time - timeBwd);

            // When the forward and backwards differences are around the same (the user places directly on a line or in the middle)
            // always go to the nearest backwards tick
            if (Math.Abs(fwdDiff - bwdDiff) <= 2)
            {
                var snapTimePerBeat = 60000f / timingPoint.Bpm / snap;
                return (int) AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, Direction.Backward, snap,
                    time + snapTimePerBeat);
            }

            var closestTime = time;

            if (bwdDiff < fwdDiff)
                closestTime = timeBwd;
            else if (fwdDiff < bwdDiff)
                closestTime = timeFwd;

            return closestTime;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        [MoonSharpVisible(false)]
        public void Undo()
        {
            foreach (var change in noteTimeAdjustments)
            {
                var note = change.Key;
                var adjustment = change.Value;
                note.StartTime = adjustment.OriginalStartTime;
                note.EndTime = adjustment.OriginalEndTime;
            }

            ActionManager.TriggerEvent(EditorActionType.ResnapHitObjects,
                new EditorActionHitObjectsResnappedEventArgs(Snaps, HitObjectsToResnap, ShowNotif));

            if (ShowNotif)
            {
                var offsnapCount = noteTimeAdjustments.Values.Count(x => x.StartTimeWasChanged || x.EndTimeWasChanged);
                var notifMessage = $"Unsnapped {offsnapCount} note{(offsnapCount == 1 ? "" : "s")}";
                NotificationManager.Show(NotificationLevel.Info, notifMessage);
            }

            noteTimeAdjustments.Clear();
        }
    }
}