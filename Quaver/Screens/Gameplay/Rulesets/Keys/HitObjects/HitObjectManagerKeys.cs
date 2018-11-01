using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Maps.Structures;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Modifiers;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    public class HitObjectManagerKeys : HitObjectManager
    {
        /// <summary>
        ///     Used to Round TrackPosition from Long to Float
        /// </summary>
        public static float TrackRounding { get; } = 100;

        /// <summary>
        ///     The speed at which objects fall down from the screen.
        /// </summary>
        public static float ScrollSpeed
        {
            get
            {
                var speed = MapManager.Selected.Value.Qua.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K : ConfigManager.ScrollSpeed7K;
                return speed.Value / (20f * AudioEngine.Track.Rate);
            }
        }

        /// <summary>
        ///     Reference to the ruleset this HitObject manager is for.
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; private set; }

        /// <summary>
        ///     Hit Object info used for object pool and gameplay
        /// </summary>
        public List<Queue<HitObjectInfo>> HitObjectInfo { get; set; }

        /// <summary>
        ///     Object pool for every hit object.
        ///     Every hit object in the pool is split by the hit object's lane
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> ActiveNotes { get; set; }

        /// <summary>
        ///     The list of dead notes (grayed out LN's)
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> DeadNotes { get; private set; }

        /// <summary>
        ///     The list of currently held long notes.
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> HeldLongNotes { get; private set; }

        /// <summary>
        ///     List of slider velocities used for the current map
        /// </summary>
        public List<SliderVelocityInfo> ScrollVelocities { get; set; } = new List<SliderVelocityInfo>();

        /// <summary>
        ///     List of added hit object positions calculated from SV. Used for optimization
        /// </summary>
        public List<long> VelocityPositionMarkers { get; set; } = new List<long>();

        /// <summary>
        ///     The object pool size.
        /// </summary>
        public int InitialPoolSizePerLane { get; } = 2;

        /// <summary>
        ///     The position at which the next Hit Object must be at in order to add a new Hit Object to the pool.
        /// </summary>
        public float CreateObjectPosition { get; } = 150000;

        /// <summary>
        ///     The position at which the earliest Hit Object must be at before its recycled.
        /// </summary>
        public float RecycleObjectPosition { get; } = 150000;

        /// <summary>
        ///     Current position for Hit Objects
        /// </summary>
        public long CurrentTrackPosition { get; private set; }

        /// <summary>
        ///     Current SV index used for optimization when using UpdateCurrentPosition()
        ///     Default value is 0. "0" means that Current time has not passed first SV point yet.
        /// </summary>
        private int CurrentSvIndex { get; set; } = 0;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int ObjectsLeft
        {
            get
            {
                var total = 0;
                foreach (var lane in ActiveNotes) total += lane.Count;
                foreach (var lane in HeldLongNotes) total += lane.Count;
                foreach (var lane in DeadNotes) total += lane.Count;
                return total;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override GameplayHitObject EarliestHitObject
        {
            get
            {
                var earliest = int.MaxValue;
                GameplayHitObject hitOb = null;

                // Check Main Object Pool
                foreach (var lane in ActiveNotes)
                {
                    if (lane.Count > 0 && lane.Peek().Info.StartTime < earliest)
                    {
                        hitOb = lane.Peek();
                        earliest = hitOb.Info.StartTime;
                    }
                }

                // Check LN Object Pool
                foreach (var lane in HeldLongNotes)
                {
                    if (lane.Count > 0 && lane.Peek().Info.StartTime < earliest)
                    {
                        hitOb = lane.Peek();
                        earliest = hitOb.Info.StartTime;
                    }
                }

                return hitOb;
            }
        }

        /// <summary>
        ///     The offset from the edge of the screen of the hit position.
        /// </summary>
        public float HitPositionOffset
        {
            get
            {
                var playfield = (GameplayPlayfieldKeys) Ruleset.Playfield;
                var skin = SkinManager.Skin.Keys[Ruleset.Mode];

                if (GameplayRulesetKeys.IsDownscroll)
                    return playfield.ReceptorPositionY + (ConfigManager.UserHitPositionOffset4K.Value + skin.HitPosOffsetY);

                // Up Scroll
                return playfield.ReceptorPositionY - (ConfigManager.UserHitPositionOffset4K.Value + skin.HitPosOffsetY);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="size"></param>
        public HitObjectManagerKeys(GameplayRulesetKeys ruleset, Qua map) : base(map)
        {
            Ruleset = ruleset;
            GameplayHitObjectKeys.HitPositionOffset = HitPositionOffset;

            // Initialize SV
            InitializeScrollVelocities(map);
            InitializePositionMarkers();
            UpdateCurrentTrackPosition();

            // Initialize Object Pool
            InitializeInfoPool(map);
            InitializeObjectPool();
        }

        /// <summary>
        ///     Initialize Info Pool. Info pool is used to pass info around to Hit Objects.
        /// </summary>
        /// <param name="map"></param>
        private void InitializeInfoPool(Qua map)
        {
            // Initialize collections
            var keyCount = Ruleset.Map.GetKeyCount();
            HitObjectInfo = new List<Queue<HitObjectInfo>>(keyCount);
            ActiveNotes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            DeadNotes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            HeldLongNotes = new List<Queue<GameplayHitObjectKeys>>(keyCount);

            // Add HitObject Info to Info pool
            for (var i = 0; i < Ruleset.Map.GetKeyCount(); i++)
            {
                HitObjectInfo.Add(new Queue<HitObjectInfo>());
                ActiveNotes.Add(new Queue<GameplayHitObjectKeys>(InitialPoolSizePerLane));
                DeadNotes.Add(new Queue<GameplayHitObjectKeys>());
                HeldLongNotes.Add(new Queue<GameplayHitObjectKeys>());
            }

            // Sort Hit Object Info into their respective lanes
            foreach (var info in map.HitObjects)
                HitObjectInfo[info.Lane - 1].Enqueue(info);
        }

        /// <summary>
        ///     Create the initial objects in the object pool
        /// </summary>
        private void InitializeObjectPool()
        {
            foreach (var lane in HitObjectInfo)
            {
                for (var i = 0; i < InitialPoolSizePerLane && lane.Count > 0; i++)
                {
                    CreatePoolObject(lane.Dequeue());
                }
            }
        }

        /// <summary>
        ///     Create new Hit Object and add it into the pool with respect to its lane
        /// </summary>
        /// <param name="info"></param>
        private void CreatePoolObject(HitObjectInfo info) => ActiveNotes[info.Lane - 1].Enqueue(new GameplayHitObjectKeys(info, Ruleset, this));

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateCurrentTrackPosition();
            UpdateAndScoreActiveObjects();
            UpdateAndScoreHeldObjects();
            UpdateDeadObjects();
        }

        /// <summary>
        ///     Returns the earliest un-tapped Hit Object
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <returns></returns>
        public GameplayHitObjectKeys GetClosestTap(int lane) => ActiveNotes[lane].Count > 0 ? ActiveNotes[lane].Peek() : null;

        /// <summary>
        ///     Returns the earliest active Long Note
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <returns></returns>
        public GameplayHitObjectKeys GetClosestRelease(int lane) => HeldLongNotes[lane].Count > 0 ? HeldLongNotes[lane].Peek() : null;

        /// <summary>
        ///     Updates the active objects in the pool + adds to score when applicable.
        /// </summary>
        private void UpdateAndScoreActiveObjects()
        {
            // Add more hit objects to the pool if necessary
            foreach (var lane in HitObjectInfo)
            {
                while (lane.Count > 0 && GetPositionFromTime(lane.Peek().StartTime) - CurrentTrackPosition < CreateObjectPosition)
                {
                    CreatePoolObject(lane.Dequeue());
                }
            }

            // Check to see if the player missed any active notes
            foreach (var lane in ActiveNotes)
            {
                while (lane.Count > 0 && (int)Ruleset.Screen.Timing.Time > lane.Peek().Info.StartTime + Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay])
                {
                    // Current hit object
                    var hitObject = lane.Dequeue();

                    // Update scoreboard for simulated plays
                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();

                    // Add new hit stat data and update score
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, hitObject.Info, (int)Ruleset.Screen.Timing.Time, Judgement.Miss,
                                            int.MinValue, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                    Ruleset.ScoreProcessor.Stats.Add(stat);
                    Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);

                    // Perform Playfield animations
                    var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
                    playfield.Stage.ComboDisplay.MakeVisible();
                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

                    // If ManiaHitObject is an LN, kill it and count it as another miss because of the tail.
                    // - missing an LN counts as two misses
                    if (hitObject.IsLongNote)
                    {
                        KillPoolObject(hitObject);
                        Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);
                        Ruleset.ScoreProcessor.Stats.Add(stat);
                        screenView.UpdateScoreboardUsers();
                    }
                    // Otherwise recycle the object.
                    else
                    {
                        KillPoolObject(hitObject);
                    }
                }
            }

            // Update active objects.
            foreach (var lane in ActiveNotes)
            {
                foreach (var hitObject in lane)
                    hitObject.UpdateSpritePositions(CurrentTrackPosition);
            }
        }

        /// <summary>
        ///     Updates the held long note objects in the pool + adds to score when applicable.
        /// </summary>
        private void UpdateAndScoreHeldObjects()
        {
            // The release window. (Window * Multiplier)
            var window = Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[Judgement.Okay];

            // Check to see if any LN releases were missed (Counts as an okay instead of a miss.)
            foreach (var lane in HeldLongNotes)
            {
                while (lane.Count > 0 && (int)Ruleset.Screen.Timing.Time > lane.Peek().Info.EndTime + window)
                {
                    // Current hit object
                    var hitObject = lane.Dequeue();

                    // The judgement that is given when a user completely fails to release.
                    const Judgement missedJudgement = Judgement.Okay;

                    // Add new hit stat data and update score
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, hitObject.Info, (int)Ruleset.Screen.Timing.Time, Judgement.Okay,
                                                int.MinValue, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                    Ruleset.ScoreProcessor.Stats.Add(stat);
                    Ruleset.ScoreProcessor.CalculateScore(missedJudgement);

                    // Update scoreboard for simulated plays
                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();

                    // Perform Playfield animations
                    var stage = ((GameplayPlayfieldKeys)Ruleset.Playfield).Stage;
                    stage.ComboDisplay.MakeVisible();
                    stage.JudgementHitBurst.PerformJudgementAnimation(missedJudgement);
                    stage.HitLightingObjects[hitObject.Info.Lane - 1].StopHolding();

                    // Update Pooling
                    KillHoldPoolObject(hitObject);
                }
            }

            // Update the currently held long notes.
            foreach (var lane in HeldLongNotes)
            {
                foreach (var hitObject in lane)
                    hitObject.UpdateSpritePositions(CurrentTrackPosition);
            }
        }

        /// <summary>
        ///     Updates all of the dead objects in the pool.
        /// </summary>
        private void UpdateDeadObjects()
        {
            // Check to see if dead object is ready for recycle
            foreach (var lane in DeadNotes)
            {
                while (lane.Count > 0 &&
                    (CurrentTrackPosition > lane.Peek().LongNoteTrackPosition + RecycleObjectPosition))
                {
                    RecyclePoolObject(lane.Dequeue());
                }
            }

            // Update dead objects.
            foreach (var lane in DeadNotes)
            {
                foreach (var hitObject in lane)
                {
                    // Update position
                    hitObject.UpdateSpritePositions(CurrentTrackPosition);
                }
            }
        }

        /// <summary>
        ///     Kills a note at a specific index of the object pool.
        /// </summary>
        /// <param name="index"></param>
        public void KillPoolObject(GameplayHitObjectKeys hitObject)
        {
            // Change the sprite color to dead.
            hitObject.Kill();

            // Add to dead notes pool
            DeadNotes[hitObject.Info.Lane - 1].Enqueue(hitObject);
        }

        /// <summary>
        ///     Recycles a pool object.
        /// </summary>
        /// <param name="index"></param>
        public void RecyclePoolObject(GameplayHitObjectKeys hitObject)
        {
            var lane = HitObjectInfo[hitObject.Info.Lane - 1];
            if (lane.Count > 0)
            {
                var info = lane.Dequeue();
                hitObject.InitializeObject(info, this);
                ActiveNotes[info.Lane - 1].Enqueue(hitObject);
            }
            else
            {
                hitObject.Destroy();
            }
        }

        /// <summary>
        ///     Changes a pool object to a long note that is held at the receptors.
        /// </summary>
        /// <param name="index"></param>
        public void ChangePoolObjectStatusToHeld(GameplayHitObjectKeys hitObject)
        {
            // Add to the held long notes.
            HeldLongNotes[hitObject.Info.Lane - 1].Enqueue(hitObject);
            hitObject.CurrentlyBeingHeld = true;
        }

        /// <summary>
        ///     Kills a hold pool object.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="destroy"></param>
        public void KillHoldPoolObject(GameplayHitObjectKeys hitObject)
        {
            // Change start time and LN size.
            var time = Ruleset.Screen.Timing.Time;
            hitObject.TrackPosition = GetPositionFromTime(time);
            hitObject.Info.StartTime = (int)time;
            hitObject.CurrentlyBeingHeld = false;
            hitObject.UpdateLongNoteSize(hitObject.TrackPosition);
            hitObject.Kill();

            // Add to dead notes pool
            DeadNotes[hitObject.Info.Lane - 1].Enqueue(hitObject);
        }

        /// <summary>
        ///     Generate Scroll Velocity points.
        /// </summary>
        /// <param name="qua"></param>
        private void InitializeScrollVelocities(Qua qua)
        {
            // Find average bpm
            var commonBpm = qua.GetCommonBpm();

            // Create SV multiplier timing points
            var index = 0;
            for (var i = 0; i < qua.TimingPoints.Count; i++)
            {
                var svFound = false;

                // SV starts after the last timing point
                if (i == qua.TimingPoints.Count - 1)
                {
                    for (var j = index; j < qua.SliderVelocities.Count; j++)
                    {
                        var sv = new SliderVelocityInfo()
                        {
                            StartTime = qua.SliderVelocities[j].StartTime,
                            Multiplier = qua.SliderVelocities[j].Multiplier * (float)(qua.TimingPoints[i].Bpm / commonBpm)
                        };
                        ScrollVelocities.Add(sv);

                        // Toggle SvFound if inheriting point is overlapping timing point
                        if (Math.Abs(sv.StartTime - qua.TimingPoints[i].StartTime) < 1)
                            svFound = true;
                    }
                }

                // SV does not start after the last timing point
                else
                {
                    for (var j = index; j < qua.SliderVelocities.Count; j++)
                    {
                        // SV starts before the first timing point
                        if (qua.SliderVelocities[j].StartTime < qua.TimingPoints[0].StartTime)
                        {
                            var sv = new SliderVelocityInfo()
                            {
                                StartTime = qua.SliderVelocities[j].StartTime,
                                Multiplier = qua.SliderVelocities[j].Multiplier * (float)(qua.TimingPoints[0].Bpm / commonBpm)
                            };
                            ScrollVelocities.Add(sv);

                            // Toggle SvFound if inheriting point is overlapping timing point
                            if (Math.Abs(sv.StartTime - qua.TimingPoints[0].StartTime) < 1)
                                svFound = true;
                        }

                        // SV start is in between two timing points
                        else if (qua.SliderVelocities[j].StartTime >= qua.TimingPoints[i].StartTime
                            && qua.SliderVelocities[j].StartTime < qua.TimingPoints[i + 1].StartTime)
                        {
                            var sv = new SliderVelocityInfo()
                            {
                                StartTime = qua.SliderVelocities[j].StartTime,
                                Multiplier = qua.SliderVelocities[j].Multiplier * (float)(qua.TimingPoints[i].Bpm / commonBpm)
                            };
                            ScrollVelocities.Add(sv);

                            // Toggle SvFound if inheriting point is overlapping timing point
                            if (Math.Abs(sv.StartTime - qua.TimingPoints[i].StartTime) < 1)
                                svFound = true;
                        }

                        // Update current index if SV falls out of range for optimization
                        else
                        {
                            index = j;
                            break;
                        }
                    }
                }

                // Create BPM SV if no inheriting point is overlapping the current timing point
                if (!svFound)
                {
                    var sv = new SliderVelocityInfo()
                    {
                        StartTime = qua.TimingPoints[i].StartTime,
                        Multiplier = (float)(qua.TimingPoints[i].Bpm / commonBpm)
                    };
                    ScrollVelocities.Add(sv);
                }
            }

            // Sort SV points by start time
            ScrollVelocities = ScrollVelocities.OrderBy(o => o.StartTime).ToList();
        }

        /// <summary>
        ///     Create SV-position points for computation optimization
        /// </summary>
        private void InitializePositionMarkers()
        {
            // Compute for Change Points
            var position = (long)(ScrollVelocities[0].StartTime * ScrollVelocities[0].Multiplier * TrackRounding);
            VelocityPositionMarkers.Add(position);

            for (var i = 1; i < ScrollVelocities.Count; i++)
            {
                position += (long)((ScrollVelocities[i].StartTime - ScrollVelocities[i - 1].StartTime) * TrackRounding * ScrollVelocities[i - 1].Multiplier);
                VelocityPositionMarkers.Add(position);
            }
        }

        /// <summary>
        ///     Get Hit Object (End/Start) position from audio time (Unoptimized.)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public long GetPositionFromTime(double time)
        {
            long curPos = 0;

            if (time < ScrollVelocities[0].StartTime)
            {
                curPos = GetPositionFromTime(time, 0);
            }
            else if (time >= ScrollVelocities[ScrollVelocities.Count - 1].StartTime)
            {
                curPos = GetPositionFromTime(time, ScrollVelocities.Count);
            }
            else
            {
                // Get index
                for (var i = 0; i < ScrollVelocities.Count; i++)
                {
                    if (time < ScrollVelocities[i].StartTime)
                    {
                        curPos = GetPositionFromTime(time, i);
                        break;
                    }
                }
            }

            return curPos;
        }

        /// <summary>
        ///     Get Hit Object (End/Start) position from audio time and SV Index.
        ///     Index used for optimization
        /// </summary>
        /// <param name="time"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public long GetPositionFromTime(double time, int index)
        {
            // NoSV Modifier is toggled on
            if (ModManager.IsActivated(ModIdentifier.NoSliderVelocity))
                return (long)(time * TrackRounding);

            // Continue if SV is enabled
            long curPos = 0;

            // Time starts before the first SV point
            if (index == 0)
                curPos = (long)(time * ScrollVelocities[0].Multiplier * TrackRounding);

            // Time starts after the first SV point and before the last SV point
            else if (index < VelocityPositionMarkers.Count)
            {
                // Reference the correct ScrollVelocities index by subracting 1
                index--;

                // Get position
                curPos = VelocityPositionMarkers[index];
                curPos += (long)((time - ScrollVelocities[index].StartTime) * ScrollVelocities[index].Multiplier * TrackRounding);
            }

            // Time starts after the last SV point
            else
            {
                // Throw exception if index exceeds list size for some reason
                if (index > VelocityPositionMarkers.Count)
                    throw new Exception("index exceeds Velocity Position Marker List Size");

                // Reference the correct ScrollVelocities index by subracting 1
                index--;

                // Get position
                curPos = VelocityPositionMarkers[index];
                curPos += (long)((time - ScrollVelocities[index].StartTime) * ScrollVelocities[index].Multiplier * TrackRounding);
            }

            return curPos;
        }

        /// <summary>
        ///     Update Current position of the hit objects
        /// </summary>
        /// <param name="audioTime"></param>
        public void UpdateCurrentTrackPosition()
        {
            // Use necessary visual offset
            var audioTime = Ruleset.Screen.Timing.Time - ConfigManager.GlobalAudioOffset.Value - MapManager.Selected.Value.LocalOffset;

            // Update SV index if necessary. Afterwards update Position.
            while (CurrentSvIndex < ScrollVelocities.Count && audioTime >= ScrollVelocities[CurrentSvIndex].StartTime)
            {
                CurrentSvIndex++;
            }
            CurrentTrackPosition = GetPositionFromTime(audioTime, CurrentSvIndex);
        }
    }
}
