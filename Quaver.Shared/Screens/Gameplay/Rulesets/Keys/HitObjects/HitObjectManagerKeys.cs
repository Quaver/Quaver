/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MoreLinq;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    public class HitObjectManagerKeys : HitObjectManager
    {
        /// <summary>
        ///     Used to Round TrackPosition from Long to Float
        /// </summary>
        public static float TrackRounding { get; } = 100;

        /// <summary>
        ///     The speed at which objects travel across the screen.
        /// </summary>
        public static float ScrollSpeed
        {
            get
            {
                var speed = ConfigManager.ScrollSpeed4K;

                if (MapManager.Selected.Value.Qua != null)
                    speed = MapManager.Selected.Value.Qua.Mode == GameMode.Keys4 ? ConfigManager.ScrollSpeed4K : ConfigManager.ScrollSpeed7K;

                var scalingFactor = QuaverGame.SkinScalingFactor;

                var game = GameBase.Game as QuaverGame;

                if (game?.CurrentScreen is IHasLeftPanel)
                    scalingFactor = (1920f - GameplayPlayfieldKeys.PREVIEW_PLAYFIELD_WIDTH) / 1366f;

                var scrollSpeed = (speed.Value / 10f) / (20f * AudioEngine.Track.Rate) * scalingFactor * WindowManager.BaseToVirtualRatio;

                return scrollSpeed;
            }
        }

        /// <summary>
        ///     Reference to the ruleset this HitObject manager is for.
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Qua with normalized SVs.
        /// </summary>
        private Qua Map;

        /// <summary>
        ///     Number of lanes
        /// </summary>
        private int KeyCount { get; }

        /// <summary>
        ///     Used for constructing <see cref="HitObjectQueueLanes"/>
        /// </summary>
        public List<GameplayHitObjectKeysInfo> HitObjectInfos { get; private set; }

        /// <summary>
        ///     Hitobject info queues used for input and scoring.
        ///     One queue for each lane.
        /// </summary>
        public List<Queue<GameplayHitObjectKeysInfo>> HitObjectQueueLanes { get; set; }

        /// <summary>
        ///     Queues for held long notes used for input and scoring.
        ///     One queue for each lane.
        /// </summary>
        public List<Queue<GameplayHitObjectKeysInfo>> HeldLongNoteLanes { get; private set; }

        /// <summary>
        ///     Pools of reusable GameplayHitObjectKeys's for drawing hitobjects on screen.
        ///     One pool for each lane.
        /// </summary>
        public List<ConcurrentBag<GameplayHitObjectKeys>> HitObjectPools { get; private set; }

        /// <summary>
        ///      All hitobjects that are currently rendered on screen.
        /// </summary>
        public HashSet<GameplayHitObjectKeysInfo> RenderedHitObjectInfos { get; private set; }

        /// <summary>
        ///     Allows for quickly finding all hitobjects close to some position.
        /// </summary>
        public SpatialHashMap<GameplayHitObjectKeysInfo> SpatialHashMap { get; private set; }

        /// <summary>
        ///     Used by <see cref="UpdateHitObjects"/> to avoid instantiating a new hash set every update
        /// </summary>
        public HashSet<GameplayHitObjectKeysInfo> InRangeHitObjectInfos { get; private set; }

        /// <summary>
        ///     List of added hit object positions calculated from SV. Used for optimization
        /// </summary>
        public List<long> VelocityPositionMarkers { get; set; } = new List<long>();

        /// <summary>
        ///     Loose upper bound of the number of hitobjects on screen at one time.
        /// </summary>
        public int MaxHitObjectCount { get; private set; }

        /// <summary>
        ///     Only objects within this distance of the <see cref="CurrentTrackPosition"/> are rendered.
        /// </summary>
        public long RenderThreshold => (long)(WindowManager.Height * TrackRounding / ScrollSpeed);

        /// <summary>
        ///     Current position of the receptors
        /// </summary>
        public long CurrentTrackPosition { get; private set; }

        /// <summary>
        ///     Current SV index used for optimization when using UpdateCurrentPosition()
        ///     Default value is 0. "0" means that Current time has not passed first SV point yet.
        /// </summary>
        private int CurrentSvIndex { get; set; } = 0;

        /// <summary>
        ///     Current audio position relative to start of audio, with song and user offset values applied.
        ///     Used for things related to timing that are unaffected by screen latency.
        /// </summary>
        public double CurrentAudioOffset { get; private set; }

        /// <summary>
        ///     Current audio position relative to start of audio, with song, user, and visual offset values applied.
        ///     Used for things related to timing that are affected by screen latency.
        /// </summary>
        public double CurrentVisualAudioOffset { get; private set; }

        /// <summary>
        ///     A mapping from hit objects to the associated hit stats from a replay.
        ///
        ///     Set to null when not applicable (e.g. outside of a replay).
        /// </summary>
        public Dictionary<HitObjectInfo, List<HitStat>> HitStats { get; private set; }

        /// <summary>
        ///     Note alpha when showing hits.
        /// </summary>
        public const float SHOW_HITS_NOTE_ALPHA = 0.3f;

        /// <summary>
        ///     Whether hits are currently shown.
        /// </summary>
        private bool _showHits = false;
        public bool ShowHits
        {
            get => _showHits;
            set
            {
                if (HitStats == null)
                    return;

                _showHits = value;

                foreach (GameplayHitObjectKeys hitObject in RenderedHitObjectInfos.Select(info => info.HitObject).Concat(HitObjectPools.SelectMany(lane => lane)))
                {
                    var tint = hitObject.Tint * (_showHits ? 1 : SHOW_HITS_NOTE_ALPHA);
                    var newTint = hitObject.Tint * (_showHits ? SHOW_HITS_NOTE_ALPHA : 1);

                    hitObject.HitObjectSprite.Tint = tint;
                    hitObject.HitObjectSprite.ClearAnimations();
                    hitObject.HitObjectSprite.FadeToColor(newTint, Easing.OutQuad, 250);
                    hitObject.LongNoteBodySprite.Tint = tint;
                    hitObject.LongNoteBodySprite.ClearAnimations();
                    hitObject.LongNoteBodySprite.FadeToColor(newTint, Easing.OutQuad, 250);
                    hitObject.LongNoteEndSprite.Tint = tint;
                    hitObject.LongNoteEndSprite.ClearAnimations();
                    hitObject.LongNoteEndSprite.FadeToColor(newTint, Easing.OutQuad, 250);
                }

                var playfield = (GameplayPlayfieldKeys) Ruleset.Playfield;

                playfield.Stage.HitContainer.Children.ForEach(x =>
                {
                    if (!(x is Sprite sprite))
                        return;

                    if (_showHits)
                    {
                        sprite.Alpha = 0;
                        sprite.FadeTo(1, Easing.OutQuad, 250);
                    }
                    else
                    {
                        sprite.Alpha = 1;
                        sprite.FadeTo(0, Easing.OutQuad, 250);
                    }
                });
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override bool IsComplete
        {
            get
            {
                // If there are objects to hit, we're not done.
                if (HitObjectQueueLanes.Any(lane => lane.Any()))
                    return false;

                // If there are held LNs, we're not done.
                if (HeldLongNoteLanes.Any(lane => lane.Any()))
                    return false;

                // Wait for dead LNs to finish scrolling
                return CurrentVisualAudioOffset > Map.Length;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override HitObjectInfo NextHitObject
        {
            get
            {
                HitObjectInfo nextHitObject = null;
                foreach (var lane in HitObjectQueueLanes)
                {
                    if (lane.Count == 0)
                        continue;

                    if (nextHitObject is null)
                    {
                        nextHitObject = lane.Peek().HitObjectInfo;
                        continue;
                    }

                    var hitObject = lane.Peek();
                    if (hitObject.StartTime < nextHitObject.StartTime)
                    {
                        nextHitObject = hitObject.HitObjectInfo;
                    }
                }

                return nextHitObject;
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

                return !(nextObject.StartTime - CurrentAudioOffset < GameplayAudioTiming.StartDelay + 5000) && !isHoldingAnyNotes;
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
            Map = map.WithNormalizedSVs();
            KeyCount = Map.GetKeyCount(Map.HasScratchKey);

            // Initialize SV
            InitializePositionMarkers();
            UpdateCurrentTrackPosition();

            InitializeHitStats();

            // Initialize HitObjects
            InitializeHitObjectInfo(map);
            InitializeObjectPool();
            ResetHitObjectInfo();

            AudioEngine.Track.RateChanged += OnRateChanged;
            ConfigManager.ScrollSpeed4K.ValueChanged += On4KScrollSpeedChanged;
            ConfigManager.ScrollSpeed7K.ValueChanged += On7KScrollSpeedChanged;
        }

        public override void Destroy()
        {
            AudioEngine.Track.RateChanged -= OnRateChanged;

            // ReSharper disable twice DelegateSubtraction
            ConfigManager.ScrollSpeed4K.ValueChanged -= On4KScrollSpeedChanged;
            ConfigManager.ScrollSpeed7K.ValueChanged -= On7KScrollSpeedChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Fills in the HitStats dictionary.
        /// </summary>
        private void InitializeHitStats()
        {
            // Don't show hit stats in the song select preview.
            if (Ruleset.Screen.IsSongSelectPreview)
                return;

            var inputManager = ((KeysInputManager) Ruleset.InputManager).ReplayInputManager;

            if (inputManager == null)
                return;

            HitStats = new Dictionary<HitObjectInfo, List<HitStat>>(HitObjectInfo.ByValueComparer);

            foreach (var hitStat in inputManager.VirtualPlayer.ScoreProcessor.Stats)
            {
                if (!HitStats.ContainsKey(hitStat.HitObject))
                    HitStats.Add(hitStat.HitObject, new List<HitStat>());

                HitStats[hitStat.HitObject].Add(hitStat);
            }
        }

        /// <summary>
        ///     Initialize collections of hitobject info that don't change between resets.
        /// </summary>
        /// <param name="map"></param>
        private void InitializeHitObjectInfo(Qua map)
        {
            HitObjectInfos = map.HitObjects.Select(info => new GameplayHitObjectKeysInfo(info, this)).ToList();

            // Using cell size equal to render area guarantees a consistent two cells accessed per update
            SpatialHashMap = new SpatialHashMap<GameplayHitObjectKeysInfo>(2 * RenderThreshold);
            foreach (var info in HitObjectInfos)
            {
                if (!info.IsLongNote)
                {
                    SpatialHashMap.Add(info.InitialTrackPosition, info);
                }
                else
                {
                    SpatialHashMap.Add(info.EarliestTrackPosition, info.LatestTrackPosition, info);
                }
            }

            // find an upper bound for number of hitobjects on screen at one time
            // each frame will always use the contents of two cells, so multiply the max by two for a loose upper bound
            MaxHitObjectCount = SpatialHashMap.Dictionary.Dictionary.Select(pair => pair.Value.Count).Max() * 2;

            HitObjectQueueLanes = new List<Queue<GameplayHitObjectKeysInfo>>(KeyCount);
            HeldLongNoteLanes = new List<Queue<GameplayHitObjectKeysInfo>>(KeyCount);
            RenderedHitObjectInfos = new HashSet<GameplayHitObjectKeysInfo>(MaxHitObjectCount);
            InRangeHitObjectInfos = new HashSet<GameplayHitObjectKeysInfo>(MaxHitObjectCount);

            for (int i = 0; i < KeyCount; i++)
            {
                HitObjectQueueLanes.Add(new Queue<GameplayHitObjectKeysInfo>(HitObjectInfos.Count));
                HeldLongNoteLanes.Add(new Queue<GameplayHitObjectKeysInfo>(1));
            }
        }

        /// <summary>
        ///     Initialize collections that change between resets.
        /// </summary>
        private void ResetHitObjectInfo()
        {
            // stop rendering hitobjects
            foreach (var info in RenderedHitObjectInfos)
            {
                HitObjectPools[info.Lane - 1].Add(info.Unlink());
            }

            // reset collections that change during gameplay
            HitObjectQueueLanes.ForEach(lane => lane.Clear());
            HeldLongNoteLanes.ForEach(lane => lane.Clear());
            RenderedHitObjectInfos.Clear();

            // populate hitobject queues
            foreach (var info in HitObjectInfos)
            {
                // skip hitobjects that we're already past
                if ((info.IsLongNote ? info.EndTime : info.StartTime) < CurrentAudioOffset)
                {
                    info.State = HitObjectState.Removed;
                    continue;
                }

                // reset hitobject states before queueing them back up
                info.State = HitObjectState.Alive;
                HitObjectQueueLanes[info.Lane - 1].Enqueue(info);
            }
        }

        /// <summary>
        ///     Create the initial objects in the object pool
        /// </summary>
        private void InitializeObjectPool()
        {
            HitObjectPools = new List<ConcurrentBag<GameplayHitObjectKeys>>(KeyCount);

            for (int lane = 0; lane < HitObjectQueueLanes.Count; lane++)
            {
                var pool = new ConcurrentBag<GameplayHitObjectKeys>();

                // assuming a roughly equal distribution of hitobjects across the lanes for a rough upper bound per lane
                // if its too low, more will be allocated during gameplay
                for (int i = 0; i < MaxHitObjectCount / KeyCount; i++)
                {
                    pool.Add(new GameplayHitObjectKeys(lane, Ruleset, this));
                }

                HitObjectPools.Add(pool);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateCurrentTrackPosition();

            ScoreMissedHitObjects();
            ScoreMissedReleases();

            UpdateHitObjects();
        }

        /// <summary>
        ///     Returns the earliest un-tapped Hit Object
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <returns></returns>
        public GameplayHitObjectKeysInfo GetClosestTap(int lane) => HitObjectQueueLanes[lane].Count > 0 ? HitObjectQueueLanes[lane].Peek() : null;

        /// <summary>
        ///     Returns the earliest active Long Note
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <returns></returns>
        public GameplayHitObjectKeysInfo GetClosestRelease(int lane) => HeldLongNoteLanes[lane].Count > 0 ? HeldLongNoteLanes[lane].Peek() : null;

        /// <summary>
        ///     Determine which hitobjects to render and update rendered hitobjects' sprite positions.
        /// </summary>
        private void UpdateHitObjects()
        {
            // stop rendering hitobjects outside range
            bool shouldRemove(GameplayHitObjectKeysInfo info)
            {
                if (info.State == HitObjectState.Removed || !HitObjectInRange(info))
                {
                    HitObjectPools[info.Lane - 1].Add(info.Unlink());
                    return true;
                }

                return false;
            }

            RenderedHitObjectInfos.RemoveWhere(info => shouldRemove(info));

            // start rendering new hitobjects in range
            InRangeHitObjectInfos.Clear();
            InRangeHitObjectInfos.UnionWith(SpatialHashMap.GetValues(CurrentTrackPosition - RenderThreshold));
            InRangeHitObjectInfos.UnionWith(SpatialHashMap.GetValues(CurrentTrackPosition + RenderThreshold));
            InRangeHitObjectInfos.RemoveWhere(info => info.HitObject != null || info.State == HitObjectState.Removed || !HitObjectInRange(info));

            foreach (var info in InRangeHitObjectInfos)
            {
                GameplayHitObjectKeys hitObject;
                if (!HitObjectPools[info.Lane - 1].TryTake(out hitObject))
                    hitObject = new GameplayHitObjectKeys(info.Lane - 1, Ruleset, this);

                info.Link(hitObject);
                RenderedHitObjectInfos.Add(info);
            }

            // update sprite positions
            foreach (var info in RenderedHitObjectInfos)
            {
                info.HitObject.UpdateSpritePositions(CurrentTrackPosition, CurrentVisualAudioOffset);
            }
        }

        /// <summary>
        ///     Determine if the hitobject is inside rendering range.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>true if the hitobject should be rendered, otherwise false.</returns>
        private bool HitObjectInRange(GameplayHitObjectKeysInfo info)
        {
            if (!info.IsLongNote)
            {
                return Math.Abs(info.InitialTrackPosition - CurrentTrackPosition) <= RenderThreshold;
            }
            else
            {
                if (Math.Abs(info.EarliestTrackPosition - CurrentTrackPosition) <= RenderThreshold)
                    return true;

                if (Math.Abs(info.LatestTrackPosition - CurrentTrackPosition) <= RenderThreshold)
                    return true;

                if (info.EarliestTrackPosition <= CurrentTrackPosition - RenderThreshold && CurrentTrackPosition - RenderThreshold <= info.LatestTrackPosition)
                    return true;

                if (info.EarliestTrackPosition <= CurrentTrackPosition + RenderThreshold && CurrentTrackPosition + RenderThreshold <= info.LatestTrackPosition)
                    return true;

                return false;
            }
        }

        /// <summary>
        ///     Check if any hitobjects were missed from lack of input
        /// </summary>
        private void ScoreMissedHitObjects()
        {
            if (Ruleset.Screen.Failed)
                return;

            // Check to see if the player missed any active notes
            foreach (var lane in HitObjectQueueLanes)
            {
                while (lane.Count > 0 && (int)CurrentAudioOffset > lane.Peek().StartTime + Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay])
                {
                    // Current hit object
                    var info = lane.Dequeue();

                    // Update scoreboard for simulated plays
                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();

                    // Add new hit stat data and update score
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, info.HitObjectInfo, info.StartTime, Judgement.Miss,
                                            int.MinValue, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                    Ruleset.ScoreProcessor.Stats.Add(stat);

                    var im = Ruleset.InputManager as KeysInputManager;

                    if (im?.ReplayInputManager == null)
                        Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);

                    var view = (GameplayScreenView)Ruleset.Screen.View;
                    view.UpdateScoreAndAccuracyDisplays();

                    // Perform Playfield animations
                    var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;

                    if (im?.ReplayInputManager == null)
                    {
                        playfield.Stage.ComboDisplay.MakeVisible();
                        playfield.Stage.JudgementHitBursts[Math.Clamp(info.Lane - 1, 0, playfield.Stage.JudgementHitBursts.Count - 1)].PerformJudgementAnimation(Judgement.Miss);
                    }

                    // If ManiaHitObject is an LN, kill it and count it as another miss because of the tail.
                    // - missing an LN counts as two misses
                    if (info.IsLongNote)
                    {
                        info.State = HitObjectState.Dead;

                        if (im?.ReplayInputManager == null)
                            Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss, true);

                        view.UpdateScoreAndAccuracyDisplays();
                        Ruleset.ScoreProcessor.Stats.Add(stat);
                        screenView.UpdateScoreboardUsers();
                    }
                    // Otherwise just kill the object.
                    else
                    {
                        info.State = HitObjectState.Dead;
                    }
                }
            }
        }

        /// <summary>
        ///     Check if any long note releases were missed from not releasing the input
        /// </summary>
        private void ScoreMissedReleases()
        {
            if (Ruleset.Screen.Failed)
                return;

            // The release window. (Window * Multiplier)
            var window = Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[Judgement.Okay];

            // Check to see if any LN releases were missed (Counts as an okay instead of a miss.)
            foreach (var lane in HeldLongNoteLanes)
            {
                while (lane.Count > 0 && (int)CurrentAudioOffset > lane.Peek().EndTime + window)
                {
                    // Current hit object
                    var info = lane.Dequeue();

                    // The judgement that is given when a user completely fails to release.
                    var missedReleaseJudgement = Judgement.Good;

                    // Add new hit stat data and update score
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, info.HitObjectInfo, info.EndTime, missedReleaseJudgement,
                                                int.MinValue, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);

                    Ruleset.ScoreProcessor.Stats.Add(stat);

                    var im = Ruleset.InputManager as KeysInputManager;

                    if (im?.ReplayInputManager == null)
                        Ruleset.ScoreProcessor.CalculateScore(missedReleaseJudgement, true);

                    // Update scoreboard for simulated plays
                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();
                    screenView.UpdateScoreAndAccuracyDisplays();

                    // Perform Playfield animations
                    var stage = ((GameplayPlayfieldKeys)Ruleset.Playfield).Stage;

                    if (im?.ReplayInputManager == null)
                    {
                        stage.ComboDisplay.MakeVisible();
                        stage.JudgementHitBursts[Math.Clamp(info.Lane - 1, 0, stage.JudgementHitBursts.Count - 1)].PerformJudgementAnimation(missedReleaseJudgement);
                    }

                    stage.HitLightingObjects[info.Lane - 1].StopHolding();

                    // Update the object
                    info.State = HitObjectState.Removed;
                }
            }
        }

        /// <summary>
        ///     Force update LN Size if user changes scroll speed settings during gameplay.
        /// </summary>
        public void ForceUpdateLNSize()
        {
            foreach (var info in RenderedHitObjectInfos)
            {
                info.HitObject.ForceUpdateLongnote(CurrentTrackPosition, CurrentVisualAudioOffset);
            }

            foreach (var lane in HeldLongNoteLanes)
            {
                foreach (var info in lane)
                {
                    info.HitObject.ForceUpdateLongnote(CurrentTrackPosition, CurrentVisualAudioOffset);
                }
            }
        }

        /// <summary>
        ///     Changes a pool object to a long note that is held at the receptors.
        /// </summary>
        /// <param name="index"></param>
        public void ChangeHitObjectToHeld(GameplayHitObjectKeysInfo info)
        {
            // Add to the held long notes.
            info.State = HitObjectState.Held;
            HeldLongNoteLanes[info.Lane - 1].Enqueue(info);
        }

        /// <summary>
        ///     Create SV-position points for computation optimization
        /// </summary>
        private void InitializePositionMarkers()
        {
            if (Map.SliderVelocities.Count == 0)
                return;

            // Compute for Change Points
            var position = (long)(Map.SliderVelocities[0].StartTime * Map.InitialScrollVelocity * TrackRounding);
            VelocityPositionMarkers.Add(position);

            for (var i = 1; i < Map.SliderVelocities.Count; i++)
            {
                position += (long)((Map.SliderVelocities[i].StartTime - Map.SliderVelocities[i - 1].StartTime)
                                   * Map.SliderVelocities[i - 1].Multiplier * TrackRounding);
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
            int i;
            for (i = 0; i < Map.SliderVelocities.Count; i++)
            {
                if (time < Map.SliderVelocities[i].StartTime)
                    break;
            }

            return GetPositionFromTime(time, i);
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
            if (Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
                return (long)(time * TrackRounding);

            if (index == 0)
            {
                // Time starts before the first SV point
                return (long) (time * Map.InitialScrollVelocity * TrackRounding);
            }

            index--;

            var curPos = VelocityPositionMarkers[index];
            curPos += (long)((time - Map.SliderVelocities[index].StartTime) * Map.SliderVelocities[index].Multiplier * TrackRounding);
            return curPos;
        }

        /// <summary>
        ///     Get SV direction changes between startTime and endTime.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public List<SVDirectionChange> GetSVDirectionChanges(double startTime, double endTime)
        {
            var changes = new List<SVDirectionChange>();

            if (Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
                return changes;

            // Find the first SV index.
            int i;
            for (i = 0; i < Map.SliderVelocities.Count; i++)
            {
                if (startTime < Map.SliderVelocities[i].StartTime)
                    break;
            }

            bool forward;
            if (i == 0)
                forward = Map.InitialScrollVelocity >= 0;
            else
                forward = Map.SliderVelocities[i - 1].Multiplier >= 0;

            // Loop over SV changes between startTime and endTime.
            for (; i < Map.SliderVelocities.Count && endTime >= Map.SliderVelocities[i].StartTime; i++)
            {
                var multiplier = Map.SliderVelocities[i].Multiplier;
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (multiplier == 0)
                    // Zero speed means we're staying in the same spot.
                    continue;

                if (forward == (multiplier > 0))
                    // The direction hasn't changed.
                    continue;

                forward = multiplier > 0;
                changes.Add(new SVDirectionChange
                {
                    StartTime = Map.SliderVelocities[i].StartTime,
                    Position = VelocityPositionMarkers[i]
                });
            }

            return changes;
        }

        /// <summary>
        ///     Returns true if the playfield is going backwards at the given time.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsSVNegative(double time)
        {
            if (Ruleset.ScoreProcessor.Mods.HasFlag(ModIdentifier.NoSliderVelocity))
                return false;

            // Find the SV index at time.
            int i;
            for (i = 0; i < Map.SliderVelocities.Count; i++)
            {
                if (time < Map.SliderVelocities[i].StartTime)
                    break;
            }

            i--;

            // Find index of the last non-zero SV.
            for (; i >= 0; i--)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (Map.SliderVelocities[i].Multiplier != 0)
                    break;
            }

            if (i == -1)
                return Map.InitialScrollVelocity < 0;

            return Map.SliderVelocities[i].Multiplier < 0;
        }

        /// <summary>
        ///     Update Current position of the hit objects
        /// </summary>
        /// <param name="audioTime"></param>
        public void UpdateCurrentTrackPosition()
        {
            CurrentAudioOffset = Ruleset.Screen.Timing.Time + ConfigManager.GlobalAudioOffset.Value * AudioEngine.Track.Rate
                                   - MapManager.Selected.Value.LocalOffset - MapManager.Selected.Value.OnlineOffset;

            CurrentVisualAudioOffset = CurrentAudioOffset + ConfigManager.VisualOffset.Value * AudioEngine.Track.Rate;

            // Update SV index if necessary. Afterwards update Position.
            while (CurrentSvIndex < Map.SliderVelocities.Count && CurrentVisualAudioOffset >= Map.SliderVelocities[CurrentSvIndex].StartTime)
            {
                CurrentSvIndex++;
            }
            CurrentTrackPosition = GetPositionFromTime(CurrentVisualAudioOffset, CurrentSvIndex);
        }

        /// <summary>
        ///     Handles skipping forward in the pool
        /// </summary>
        public void HandleSkip()
        {
            CurrentSvIndex = 0;
            UpdateCurrentTrackPosition();

            ResetHitObjectInfo();

            Update(new GameTime());
        }

        private void OnRateChanged(object sender, TrackRateChangedEventArgs e) => ForceUpdateLNSize();

        private void On7KScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e) => ForceUpdateLNSize();

        private void On4KScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e) => ForceUpdateLNSize();
    }
}
