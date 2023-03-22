using System;
using System.Collections.Generic;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
	public class GameplayHitObjectInfo : HitObjectInfo
	{
        private HitObjectState _state;
		public HitObjectState State
        {
            get => _state;
            set
            {
                // do it in setter? or have a separate kill method? or use an event model?
                if (value == HitObjectState.Dead && HitObject != null)
                    HitObject.Kill();

                _state = value;
            }
        }

		public GameplayHitObjectKeys HitObject { get; private set; }

		private HitObjectManagerKeys Manager { get; set; }

        /// <summary>
        ///     Changes of SV direction during this LN.
        ///
        ///     Used for computing the earliest and latest visible position of this LN.
        /// </summary>
        public List<SVDirectionChange> SVDirectionChanges { get; private set; }

        /// <summary>
        ///     Y-offset from the origin?
        /// </summary>
        public long InitialTrackPosition { get; private set; }

        /// <summary>
        ///     Position of the LN end sprite.
        /// </summary>
        public long EndTrackPosition { get; private set; }

        /// <summary>
        ///     Latest position of this object.
        ///
        ///     For LNs with negative SVs, this can be larger than EndTrackPosition for example.
        /// </summary>
        public long LatestTrackPosition { get; private set; }

        /// <summary>
        ///     Earliest position of this object.
        ///
        ///     For LNs with negative SVs, this can be earlier than InitialTrackPosition for example.
        /// </summary>
        public long EarliestTrackPosition { get; private set; }

		public bool CurrentlyBeingHeld => State == HitObjectState.Held;

		public GameplayHitObjectInfo(HitObjectInfo info, HitObjectManagerKeys manager, HitObjectState state = HitObjectState.Alive, GameplayHitObjectKeys hitObject = null)
		{
            StartTime = info.StartTime;
            Lane = info.Lane;
            EndTime = info.EndTime;
            HitSound = info.HitSound;
            KeySounds = info.KeySounds;
            EditorLayer = info.EditorLayer;
            IsEditableInLuaScript = info.IsEditableInLuaScript;

			Manager = manager;
			State = state;
			HitObject = hitObject;

			InitializePositions();
		}

		private void InitializePositions()
		{
			InitialTrackPosition = Manager.GetPositionFromTime(StartTime);

            // Update Hit Object State depending if its an LN or not
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
                // CurrentLongNoteBodySize = (LatestTrackPosition - EarliestTrackPosition) * HitObjectManagerKeys.ScrollSpeed / HitObjectManagerKeys.TrackRounding - LongNoteSizeDifference;
            }
		}

        public void Link(GameplayHitObjectKeys hitObject)
        {
			HitObject = hitObject;
			HitObject.InitializeObject(Manager, this);
        }

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

	public enum HitObjectState
	{
		Alive,
		Held,
		Dead,
		Removed
	}
}
