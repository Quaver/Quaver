using System;
using System.Collections.Generic;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    /// <summary>
    ///     Controls a note using SV.
    /// </summary>
    /// <seealso cref="ScrollGroupControllerKeys"/>
    public class ScrollNoteController : NoteControllerKeys
    {
        /// <summary>
        ///     Shortcut for <see cref="ScrollNoteController.TimingGroupController"/> casted to <see cref="ScrollGroupControllerKeys"/>
        /// </summary>
        private ScrollGroupControllerKeys ScrollGroupController => (ScrollGroupControllerKeys)TimingGroupController;

        public override bool ShouldFlipLongNoteEnd =>
            ScrollGroupController.IsSVNegative(HitObjectInfo.EndTime);

        /// <summary>
        ///     Changes of SV direction during this LN.
        ///
        ///     Used for computing the earliest and latest visible position of this LN.
        /// </summary>
        public List<SVDirectionChange> SVDirectionChanges { get; protected set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timingGroupControllerKeys"></param>
        /// <param name="state"></param>
        /// <param name="hitObject"></param>
        public ScrollNoteController(HitObjectInfo info, TimingGroupControllerKeys timingGroupControllerKeys,
            HitObjectState state = HitObjectState.Alive, GameplayHitObjectKeys hitObject = null)
            : base(info, timingGroupControllerKeys, hitObject)
        {
            Manager = timingGroupControllerKeys.Manager;
            State = state;
            InitializePositions();
        }

        /// <summary>
        ///     Calculates all relevant positions
        /// </summary>
        private void InitializePositions()
        {
            InitialTrackPosition = TimingGroupController.GetPositionFromTime(StartTime);

            if (!IsLongNote)
            {
                LatestTrackPosition = InitialTrackPosition;
                LatestHeldPosition = InitialTrackPosition;
            }
            else
            {
                SVDirectionChanges = ScrollGroupController.GetSVDirectionChanges(StartTime, EndTime);
                EndTrackPosition = TimingGroupController.GetPositionFromTime(EndTime);

                var earliestPosition = Math.Min(InitialTrackPosition, EndTrackPosition);
                var latestPosition = Math.Max(InitialTrackPosition, EndTrackPosition);

                foreach (var change in SVDirectionChanges)
                {
                    earliestPosition = Math.Min(earliestPosition, change.Position);
                    latestPosition = Math.Max(latestPosition, change.Position);
                }

                EarliestTrackPosition = earliestPosition;
                LatestTrackPosition = latestPosition;
            }
        }

        /// <summary>
        ///     Initializes <see cref="NoteControllerKeys.EarliestHeldPosition"/> and <see cref="NoteControllerKeys.LatestHeldPosition"/>.
        ///     Note that we cannot use <see cref="UpdateLongNoteSize"/> as it depends on curTime,
        ///     and we haven't reached there yet. 
        /// </summary>
        public override void InitializeLongNoteSize()
        {
            UpdateLongNoteSize(InitialTrackPosition, StartTime);
        }

        public override void UpdatePositions(double curTime)
        {
        }

        public override void UpdateLongNoteSize(double curTime)
        {
            UpdateLongNoteSize(TimingGroupController.CurrentTrackPosition, curTime);
        }

        /// <summary>
        ///     Updates the earliest and latest track positions as well as the current LN body size.
        /// </summary>
        private void UpdateLongNoteSize(long offset, double curTime)
        {
            // If we're past the LN start, start from the current position.
            var startPosition = curTime >= StartTime ? offset : InitialTrackPosition;
            var realTime = Math.Max(curTime, StartTime);

            var earliestPosition = Math.Min(startPosition, EndTrackPosition);
            var latestPosition = Math.Max(startPosition, EndTrackPosition);

            if (!LegacyLNRendering)
                SetEarliestAndLatestLongNotes(realTime, ref earliestPosition, ref latestPosition);


            EarliestHeldPosition = earliestPosition;
            LatestHeldPosition = latestPosition;
        }

        /// <summary>
        ///     Calculates the position of the Hit Object with a position offset.
        /// </summary>
        /// <returns></returns>
        public override float GetSpritePosition(float hitPosition, float initialPos) =>
            hitPosition + ((initialPos - TimingGroupController.CurrentTrackPosition) *
                           (ScrollDirection == ScrollDirection.Down
                               ? -ScrollGroupController.ScrollSpeed
                               : ScrollGroupController.ScrollSpeed)
                           / HitObjectManagerKeys.TrackRounding);

        private void SetEarliestAndLatestLongNotes(double curTime, ref long earliestPosition, ref long latestPosition)
        {
            if (SVDirectionChanges == null)
                return;
            foreach (var change in SVDirectionChanges)
            {
                if (curTime >= change.StartTime)
                    continue; // We're past this change already.

                earliestPosition = Math.Min(earliestPosition, change.Position);
                latestPosition = Math.Max(latestPosition, change.Position);
            }
        }
    }
}