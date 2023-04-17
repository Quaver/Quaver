using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
	public class GameplayHitObjectKeysInfo
	{
        /// <summary>
        ///     Whether a hit object has been hit, held, missed, or not yet hit
        /// </summary>
        private HitObjectState _state;
		public HitObjectState State
        {
            get => _state;
            set
            {
                if (value == HitObjectState.Dead && HitObject != null)
                    HitObject.Kill();

                _state = value;
            }
        }

        /// <summary>
        ///     Original hitobject info
        /// </summary>
        public HitObjectInfo HitObjectInfo { get; private set;  }

        public int StartTime => HitObjectInfo.StartTime;
        public int Lane => HitObjectInfo.Lane;
        public int EndTime => HitObjectInfo.EndTime;
        public bool IsLongNote => HitObjectInfo.IsLongNote;

        /// <summary>
        ///     Represents the hitobject on screen
        /// </summary>
		public GameplayHitObjectKeys HitObject { get; private set; }

        /// <summary>
        ///     Reference to the hitobject manager that this belongs to
        /// </summary>
        /// <value></value>
		private HitObjectManagerKeys Manager { get; set; }

        /// <summary>
        ///     Changes of SV direction during this LN.
        ///
        ///     Used for computing the earliest and latest visible position of this LN.
        /// </summary>
        public List<SVDirectionChange> SVDirectionChanges { get; private set; }

        /// <summary>
        ///     Y-offset from the origin
        /// </summary>
        public long InitialTrackPosition { get; private set; }

        /// <summary>
        ///     Position of the LN end sprite.
        /// </summary>
        public long EndTrackPosition { get; private set; }

        /// <summary>
        ///     Latest position of this object.
        ///
        ///     For LNs with negative SVs, this can be larger than EndTrackPosition.
        /// </summary>
        public long LatestTrackPosition { get; private set; }

        /// <summary>
        ///     Earliest position of this object.
        ///
        ///     For LNs with negative SVs, this can be earlier than InitialTrackPosition.
        /// </summary>
        public long EarliestTrackPosition { get; private set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="info"></param>
        /// <param name="manager"></param>
        /// <param name="state"></param>
        /// <param name="hitObject"></param>
		public GameplayHitObjectKeysInfo(HitObjectInfo info, HitObjectManagerKeys manager, HitObjectState state = HitObjectState.Alive, GameplayHitObjectKeys hitObject = null)
		{
            HitObjectInfo = info;
            Manager = manager;
			State = state;
			HitObject = hitObject;

			InitializePositions();
		}

        /// <summary>
        ///     Calculates all relevant positions
        /// </summary>
		private void InitializePositions()
		{
			InitialTrackPosition = Manager.GetPositionFromTime(StartTime);

            if (!IsLongNote)
            {
                LatestTrackPosition = InitialTrackPosition;
            }
            else
            {
                SVDirectionChanges = Manager.GetSVDirectionChanges(StartTime, EndTime);
                EndTrackPosition = Manager.GetPositionFromTime(EndTime);

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
        ///     Associate a GameplayHitObjectKeys to start drawing this hitobject.
        /// </summary>
        /// <param name="hitObject"></param>
        public void Link(GameplayHitObjectKeys hitObject)
        {
			HitObject = hitObject;
			HitObject.InitializeObject(Manager, this);
        }

        /// <summary>
        ///     Dissaociate the currently linked GameplayHitObjectKeys to stop drawing this hitobject.
        /// </summary>
        /// <returns>The linked GameplayHitObjectKeys to add back to the pool.</returns>
        /// <exception cref="InvalidOperationException"></exception>
		public GameplayHitObjectKeys Unlink()
		{
			if (HitObject is null)
				throw new InvalidOperationException("GameplayHitObjectInfo is not linked to a GameplayHitObjectKeys");

			HitObject.Hide();
			var temp = HitObject;
			HitObject = null;
			return temp;
		}
	}

    /// <summary>
    ///     Hitobjects can be alive, held, dead, or removed.
    /// </summary>
	public enum HitObjectState
	{
        /// <summary>
        ///     Hitobject has not been hit yet.
        ///     Sprite should appear as normal.
        /// </summary>
		Alive,
        /// <summary>
        ///     Hitobject is a long note and is currently being held.
        ///     Long note length should be changing over time.
        /// </summary>
		Held,
        /// <summary>
        ///     Note has been late-missed,
        ///     or long note has been early or late-missed,
        ///     or long note was released very early.
        ///     Sprite should be tinted to show that it was missed.
        /// </summary>
		Dead,
        /// <summary>
        ///     The note has been hit, or the long note has been properly released.
        ///     There should no longer be a visible sprite representing the hitobject.
        /// </summary>
		Removed
	}
}
