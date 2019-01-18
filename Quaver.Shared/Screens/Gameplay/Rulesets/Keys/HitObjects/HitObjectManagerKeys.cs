/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Skinning;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
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
        ///     Every hit object in the pool is split by the hit object's lane
        /// </summary>
        public List<Queue<HitObjectInfo>> HitObjectQueueLanes { get; set; }

        /// <summary>
        ///     Object pool for every hit object.
        ///     Every hit object in the pool is split by the hit object's lane
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> ActiveNoteLanes { get; set; }

        /// <summary>
        ///     The list of dead notes (grayed out LN's)
        ///     Every hit object in the pool is split by the hit object's lane
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> DeadNoteLanes { get; private set; }

        /// <summary>
        ///     The list of currently held long notes.
        ///     Every hit object in the pool is split by the hit object's lane
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> HeldLongNoteLanes { get; private set; }

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
        ///     Used to determine the max position for object pooling recycling/creation.
        /// </summary>
        private float ObjectPositionMagnitude { get; } = 300000;

        /// <summary>
        ///     The position at which the next Hit Object must be at in order to add a new Hit Object to the pool.
        ///     TODO: Update upon scroll speed changes
        /// </summary>
        public float CreateObjectPosition { get; private set; }

        /// <summary>
        ///     The position at which the earliest Hit Object must be at before its recycled.
        ///     TODO: Update upon scroll speed changes
        /// </summary>
        public float RecycleObjectPosition { get; private set; }

        /// <summary>
        ///     Current position for Hit Objects.
        /// </summary>
        public long CurrentTrackPosition { get; private set; }

        /// <summary>
        ///     Current SV index used for optimization when using UpdateCurrentPosition()
        ///     Default value is 0. "0" means that Current time has not passed first SV point yet.
        /// </summary>
        private int CurrentSvIndex { get; set; } = 0;

        /// <summary>
        ///     Current audio position with song and user offset values applied.
        /// </summary>
        public double CurrentAudioPosition { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int ObjectsLeft
        {
            get
            {
                var total = 0;
                ActiveNoteLanes.ForEach(x => total += x.Count);
                HeldLongNoteLanes.ForEach(x => total += x.Count);
                DeadNoteLanes.ForEach(x => total += x.Count);
                return total;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override HitObjectInfo? NextHitObject
        {
            get
            {
                HitObjectInfo? nextObject = null;

                var earliestObjectTime = int.MaxValue;

                foreach (var objectsInLane in HitObjectQueueLanes)
                {
                    if (objectsInLane.Count == 0)
                        continue;

                    var hitObject = objectsInLane.Peek();

                    if (hitObject.StartTime >= earliestObjectTime)
                        continue;

                    earliestObjectTime = hitObject.StartTime;
                    nextObject = hitObject;
                }

                return nextObject;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override bool OnBreak
        {
            get
            {
                var nextObject = NextHitObject;

                if (nextObject == null)
                    return false;

                var isHoldingAnyNotes = false;

                foreach (var laneObjects in HeldLongNoteLanes)
                {
                    if (laneObjects.Count == 0)
                        continue;

                    isHoldingAnyNotes = true;
                }

                return !(nextObject.Value.StartTime - CurrentAudioPosition < GameplayAudioTiming.StartDelay + 5000) && !isHoldingAnyNotes;
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

            // Initialize SV
            UpdatePoolingPositions();
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
            HitObjectQueueLanes = new List<Queue<HitObjectInfo>>(keyCount);
            ActiveNoteLanes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            DeadNoteLanes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            HeldLongNoteLanes = new List<Queue<GameplayHitObjectKeys>>(keyCount);

            // Add HitObject Info to Info pool
            for (var i = 0; i < Ruleset.Map.GetKeyCount(); i++)
            {
                HitObjectQueueLanes.Add(new Queue<HitObjectInfo>());
                ActiveNoteLanes.Add(new Queue<GameplayHitObjectKeys>(InitialPoolSizePerLane));
                DeadNoteLanes.Add(new Queue<GameplayHitObjectKeys>());
                HeldLongNoteLanes.Add(new Queue<GameplayHitObjectKeys>());
            }

            // Sort Hit Object Info into their respective lanes
            foreach (var info in map.HitObjects)
                HitObjectQueueLanes[info.Lane - 1].Enqueue(info);
        }

        /// <summary>
        ///     Create the initial objects in the object pool
        /// </summary>
        private void InitializeObjectPool()
        {
            foreach (var lane in HitObjectQueueLanes)
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
        private void CreatePoolObject(API.Maps.Structures.HitObjectInfo info) => ActiveNoteLanes[info.Lane - 1].Enqueue(new GameplayHitObjectKeys(info, Ruleset, this));

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
        public GameplayHitObjectKeys GetClosestTap(int lane) => ActiveNoteLanes[lane].Count > 0 ? ActiveNoteLanes[lane].Peek() : null;

        /// <summary>
        ///     Returns the earliest active Long Note
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <returns></returns>
        public GameplayHitObjectKeys GetClosestRelease(int lane) => HeldLongNoteLanes[lane].Count > 0 ? HeldLongNoteLanes[lane].Peek() : null;

        /// <summary>
        ///     Updates the active objects in the pool + adds to score when applicable.
        /// </summary>
        private void UpdateAndScoreActiveObjects()
        {
            // Add more hit objects to the pool if necessary
            foreach (var lane in HitObjectQueueLanes)
            {
                while (lane.Count > 0 && CurrentTrackPosition - GetPositionFromTime(lane.Peek().StartTime) > CreateObjectPosition)
                {
                    CreatePoolObject(lane.Dequeue());
                }
            }

            // Check to see if the player missed any active notes
            foreach (var lane in ActiveNoteLanes)
            {
                while (lane.Count > 0 && CurrentAudioPosition > lane.Peek().Info.StartTime + Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay])
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

                    var view = (GameplayScreenView)Ruleset.Screen.View;
                    view.UpdateScoreAndAccuracyDisplays();

                    // Perform Playfield animations
                    var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
                    playfield.Stage.ComboDisplay.MakeVisible();
                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

                    // If ManiaHitObject is an LN, kill it and count it as another miss because of the tail.
                    // - missing an LN counts as two misses
                    if (hitObject.Info.IsLongNote)
                    {
                        KillPoolObject(hitObject);
                        Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);

                        view.UpdateScoreAndAccuracyDisplays();
                        Ruleset.ScoreProcessor.Stats.Add(stat);
                        screenView.UpdateScoreboardUsers();
                    }
                    // Otherwise just kill the object.
                    else
                    {
                        KillPoolObject(hitObject);
                    }
                }
            }

            // Update active objects.
            foreach (var lane in ActiveNoteLanes)
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
            foreach (var lane in HeldLongNoteLanes)
            {
                while (lane.Count > 0 && CurrentAudioPosition > lane.Peek().Info.EndTime + window)
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
                    screenView.UpdateScoreAndAccuracyDisplays();

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
            foreach (var lane in HeldLongNoteLanes)
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
            foreach (var lane in DeadNoteLanes)
            {
                while (lane.Count > 0 &&
                    (CurrentTrackPosition - lane.Peek().InitialLongNoteTrackPosition > RecycleObjectPosition))
                {
                    RecyclePoolObject(lane.Dequeue());
                }
            }

            // Update dead objects.
            foreach (var lane in DeadNoteLanes)
            {
                foreach (var hitObject in lane)
                {
                    hitObject.UpdateSpritePositions(CurrentTrackPosition);
                }
            }
        }

        /// <summary>
        ///     Force update LN Size if user changes scroll speed settings during gameplay.
        /// </summary>
        public void ForceUpdateLNSize()
        {
            // Update Object Reference Positions with new scroll speed
            UpdatePoolingPositions();

            // Update HitObject LN size
            for (var i = 0; i < ActiveNoteLanes.Count; i++)
            {
                foreach (var hitObject in ActiveNoteLanes[i])
                    hitObject.ForceUpdateLongnote(CurrentTrackPosition);
                foreach (var hitObject in DeadNoteLanes[i])
                    hitObject.ForceUpdateLongnote(CurrentTrackPosition);
            }
        }

        /// <summary>
        ///     Update Hitobject pooling positions to compensate for scroll speed.
        /// </summary>
        private void UpdatePoolingPositions()
        {
            RecycleObjectPosition = (ObjectPositionMagnitude / 4) / ScrollSpeed;
            CreateObjectPosition = -ObjectPositionMagnitude / ScrollSpeed;
        }

        /// <summary>
        ///     Kills a note at a specific index of the object pool.
        /// </summary>
        /// <param name="index"></param>
        public void KillPoolObject(GameplayHitObjectKeys gameplayHitObject)
        {
            // Change the sprite color to dead.
            gameplayHitObject.Kill();

            // Add to dead notes pool
            DeadNoteLanes[gameplayHitObject.Info.Lane - 1].Enqueue(gameplayHitObject);
        }

        /// <summary>
        ///     Recycles a pool object.
        /// </summary>
        /// <param name="index"></param>
        public void RecyclePoolObject(GameplayHitObjectKeys gameplayHitObject)
        {
            var lane = HitObjectQueueLanes[gameplayHitObject.Info.Lane - 1];
            if (lane.Count > 0)
            {
                var info = lane.Dequeue();
                gameplayHitObject.InitializeObject(this, info);
                ActiveNoteLanes[info.Lane - 1].Enqueue(gameplayHitObject);
            }
            else
            {
                gameplayHitObject.Destroy();
            }
        }

        /// <summary>
        ///     Changes a pool object to a long note that is held at the receptors.
        /// </summary>
        /// <param name="index"></param>
        public void ChangePoolObjectStatusToHeld(GameplayHitObjectKeys gameplayHitObject)
        {
            // Add to the held long notes.
            HeldLongNoteLanes[gameplayHitObject.Info.Lane - 1].Enqueue(gameplayHitObject);
            gameplayHitObject.CurrentlyBeingHeld = true;
        }

        /// <summary>
        ///     Kills a hold pool object.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="destroy"></param>
        public void KillHoldPoolObject(GameplayHitObjectKeys gameplayHitObject)
        {
            // Change start time and LN size.
            gameplayHitObject.InitialTrackPosition = GetPositionFromTime(CurrentAudioPosition);
            var temp = gameplayHitObject.Info;
            temp.StartTime = (int)CurrentAudioPosition;
            gameplayHitObject.Info = temp;
            gameplayHitObject.CurrentlyBeingHeld = false;
            gameplayHitObject.UpdateLongNoteSize(gameplayHitObject.InitialTrackPosition);
            gameplayHitObject.Kill();

            // Add to dead notes pool
            DeadNoteLanes[gameplayHitObject.Info.Lane - 1].Enqueue(gameplayHitObject);
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
            CurrentAudioPosition = Ruleset.Screen.Timing.Time + ConfigManager.GlobalAudioOffset.Value - MapManager.Selected.Value.LocalOffset;

            // Update SV index if necessary. Afterwards update Position.
            while (CurrentSvIndex < ScrollVelocities.Count && CurrentAudioPosition >= ScrollVelocities[CurrentSvIndex].StartTime)
            {
                CurrentSvIndex++;
            }
            CurrentTrackPosition = GetPositionFromTime(CurrentAudioPosition, CurrentSvIndex);
        }
    }
}
