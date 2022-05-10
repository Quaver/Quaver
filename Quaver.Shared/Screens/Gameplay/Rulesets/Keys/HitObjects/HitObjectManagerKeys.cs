/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections;
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
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Input;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Selection;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Logging;
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
        ///     The speed at which objects fall down from the screen.
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
        ///     Length of the Map.
        /// </summary>
        private int MapLength { get; }

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

        /// <summary>
        ///     Current audio position with song, user and visual offset values applied.
        /// </summary>
        public double CurrentVisualPosition { get; private set; }

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

                foreach (GameplayHitObjectKeys hitObject in ActiveNoteLanes.Concat(DeadNoteLanes).Concat(HeldLongNoteLanes).Flatten())
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
                if (ActiveNoteLanes.Any(lane => lane.Any()))
                    return false;

                // If there are held LNs, we're not done.
                if (HeldLongNoteLanes.Any(lane => lane.Any()))
                    return false;

                // If there are dead LNs, we're done when we're past the map length.
                if (DeadNoteLanes.Any(lane => lane.Any()))
                    // If this is "return false;" then the game never ends if the map ends with an LN and a 0× SV
                    // and the LN is missed. This is because it never leaves DeadNoteLanes since the playfield doesn't
                    // move.
                    return CurrentVisualPosition > MapLength;

                // If there are no objects left, we're done.
                return true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override HitObjectInfo NextHitObject
        {
            get
            {
                HitObjectInfo nextObject = null;

                var earliestObjectTime = int.MaxValue;

                // Some objects are already queued in ActiveNoteLanes, check that first.
                foreach (var objectsInLane in ActiveNoteLanes)
                {
                    if (objectsInLane.Count == 0)
                        continue;

                    var hitObject = objectsInLane.Peek();

                    if (hitObject.Info.StartTime >= earliestObjectTime)
                        continue;

                    earliestObjectTime = hitObject.Info.StartTime;
                    nextObject = hitObject.Info;
                }

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

                return !(nextObject.StartTime - CurrentAudioPosition < GameplayAudioTiming.StartDelay + 5000) && !isHoldingAnyNotes;
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
            MapLength = Map.Length;

            // Initialize SV
            UpdatePoolingPositions();
            InitializePositionMarkers();
            UpdateCurrentTrackPosition();

            InitializeHitStats();

            // Initialize Object Pool
            InitializeInfoPool(map);
            InitializeObjectPool();

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

            HitStats = new Dictionary<HitObjectInfo, List<HitStat>>();

            foreach (var hitStat in inputManager.VirtualPlayer.ScoreProcessor.Stats)
            {
                if (!HitStats.ContainsKey(hitStat.HitObject))
                    HitStats.Add(hitStat.HitObject, new List<HitStat>());

                HitStats[hitStat.HitObject].Add(hitStat);
            }
        }

        /// <summary>
        ///     Initialize Info Pool. Info pool is used to pass info around to Hit Objects.
        /// </summary>
        /// <param name="map"></param>
        private void InitializeInfoPool(Qua map, bool skipObjects = false)
        {
            // Initialize collections
            var keyCount = Ruleset.Map.GetKeyCount(map.HasScratchKey);
            HitObjectQueueLanes = new List<Queue<HitObjectInfo>>(keyCount);
            ActiveNoteLanes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            DeadNoteLanes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            HeldLongNoteLanes = new List<Queue<GameplayHitObjectKeys>>(keyCount);

            // Add HitObject Info to Info pool
            for (var i = 0; i < keyCount; i++)
            {
                HitObjectQueueLanes.Add(new Queue<HitObjectInfo>());
                ActiveNoteLanes.Add(new Queue<GameplayHitObjectKeys>(InitialPoolSizePerLane));
                DeadNoteLanes.Add(new Queue<GameplayHitObjectKeys>());
                HeldLongNoteLanes.Add(new Queue<GameplayHitObjectKeys>());
            }

            // Sort Hit Object Info into their respective lanes
            foreach (var info in map.HitObjects)
            {
                // Skip objects that aren't a second within range
                if (skipObjects)
                {
                    if (!info.IsLongNote)
                    {
                        if (info.StartTime < CurrentAudioPosition)
                            continue;
                    }
                    else
                    {
                        if (info.StartTime < CurrentAudioPosition && info.EndTime < CurrentAudioPosition)
                            continue;
                    }
                }

                HitObjectQueueLanes[info.Lane - 1].Enqueue(info);
            }
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

            ScoreActiveObjects();

            // Update active objects.
            foreach (var lane in ActiveNoteLanes)
            {
                foreach (var hitObject in lane)
                    hitObject.UpdateSpritePositions(CurrentTrackPosition, CurrentVisualPosition);
            }
        }

        /// <summary>
        /// </summary>
        private void ScoreActiveObjects()
        {
            if (Ruleset.Screen.Failed)
                return;

            // Check to see if the player missed any active notes
            foreach (var lane in ActiveNoteLanes)
            {
                while (lane.Count > 0 && (int)CurrentAudioPosition > lane.Peek().Info.StartTime + Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay])
                {
                    // Current hit object
                    var hitObject = lane.Dequeue();

                    // Update scoreboard for simulated plays
                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();

                    // Add new hit stat data and update score
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, hitObject.Info, hitObject.Info.StartTime, Judgement.Miss,
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
                        playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);
                    }

                    // If ManiaHitObject is an LN, kill it and count it as another miss because of the tail.
                    // - missing an LN counts as two misses
                    if (hitObject.Info.IsLongNote)
                    {
                        KillPoolObject(hitObject);

                        if (im?.ReplayInputManager == null)
                            Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss, true);

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
        }

        /// <summary>
        ///     Updates the held long note objects in the pool + adds to score when applicable.
        /// </summary>
        private void UpdateAndScoreHeldObjects()
        {
            ScoreHeldObjects();

            // Update the currently held long notes.
            foreach (var lane in HeldLongNoteLanes)
            {
                foreach (var hitObject in lane)
                    hitObject.UpdateSpritePositions(CurrentTrackPosition, CurrentVisualPosition);
            }
        }

        /// <summary>
        /// </summary>
        private void ScoreHeldObjects()
        {
            if (Ruleset.Screen.Failed)
                return;

            // The release window. (Window * Multiplier)
            var window = Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[Judgement.Okay];

            // Check to see if any LN releases were missed (Counts as an okay instead of a miss.)
            foreach (var lane in HeldLongNoteLanes)
            {
                while (lane.Count > 0 && (int)CurrentAudioPosition > lane.Peek().Info.EndTime + window)
                {
                    // Current hit object
                    var hitObject = lane.Dequeue();

                    // The judgement that is given when a user completely fails to release.
                    var missedReleaseJudgement = Judgement.Good;

                    // Add new hit stat data and update score
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, hitObject.Info, hitObject.Info.EndTime, missedReleaseJudgement,
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
                        stage.JudgementHitBurst.PerformJudgementAnimation(missedReleaseJudgement);
                    }

                    stage.HitLightingObjects[hitObject.Info.Lane - 1].StopHolding();

                    // Update Pooling
                    RecyclePoolObject(hitObject);
                }
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
                    (CurrentTrackPosition - lane.Peek().LatestTrackPosition > RecycleObjectPosition))
                {
                    RecyclePoolObject(lane.Dequeue());
                }
            }

            // Update dead objects.
            foreach (var lane in DeadNoteLanes)
            {
                foreach (var hitObject in lane)
                {
                    hitObject.UpdateSpritePositions(CurrentTrackPosition, CurrentVisualPosition);
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
                    hitObject.ForceUpdateLongnote(CurrentTrackPosition, CurrentVisualPosition);
                foreach (var hitObject in DeadNoteLanes[i])
                    hitObject.ForceUpdateLongnote(CurrentTrackPosition, CurrentVisualPosition);
                foreach (var hitObject in HeldLongNoteLanes[i])
                    hitObject.ForceUpdateLongnote(CurrentTrackPosition, CurrentVisualPosition);
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
        /// <param name="gameplayHitObject"></param>
        public void KillHoldPoolObject(GameplayHitObjectKeys gameplayHitObject, bool setTint = true)
        {
            // Change start time and LN size.
            gameplayHitObject.InitialTrackPosition = GetPositionFromTime(CurrentVisualPosition);
            gameplayHitObject.CurrentlyBeingHeld = false;
            gameplayHitObject.UpdateLongNoteSize(CurrentTrackPosition, CurrentVisualPosition);

            if (setTint)
                gameplayHitObject.Kill();

            // Add to dead notes pool
            DeadNoteLanes[gameplayHitObject.Info.Lane - 1].Enqueue(gameplayHitObject);
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
            CurrentAudioPosition = Ruleset.Screen.Timing.Time + ConfigManager.GlobalAudioOffset.Value * AudioEngine.Track.Rate
                                   - MapManager.Selected.Value.LocalOffset - MapManager.Selected.Value.OnlineOffset;

            CurrentVisualPosition = CurrentAudioPosition + ConfigManager.VisualOffset.Value * AudioEngine.Track.Rate;

            // Update SV index if necessary. Afterwards update Position.
            while (CurrentSvIndex < Map.SliderVelocities.Count && CurrentVisualPosition >= Map.SliderVelocities[CurrentSvIndex].StartTime)
            {
                CurrentSvIndex++;
            }
            CurrentTrackPosition = GetPositionFromTime(CurrentVisualPosition, CurrentSvIndex);
        }

        /// <summary>
        ///     Handles skipping forward in the pool
        /// </summary>
        public void HandleSkip()
        {
            DestroyAllObjects();

            CurrentSvIndex = 0;
            UpdateCurrentTrackPosition();

            InitializeInfoPool(Ruleset.Map, true);
            InitializeObjectPool();
            Update(new GameTime());
        }

        /// <summary>
        /// </summary>
        public void DestroyAllObjects()
        {
            DestroyPoolList(ActiveNoteLanes);
            DestroyPoolList(HeldLongNoteLanes);
            DestroyPoolList(DeadNoteLanes);

            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;

            for (var i = playfield.Stage.HitObjectContainer.Children.Count - 1; i >= 0; i--)
                playfield.Stage.HitObjectContainer.Children[i].Destroy();

            playfield.Stage.HitObjectContainer.Children.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="objects"></param>
        private void DestroyPoolList(List<Queue<GameplayHitObjectKeys>> objects)
        {
            foreach (var lane in objects)
            {
                while (lane.Count > 0)
                    lane.Dequeue().Destroy();
            }
        }

        private void OnRateChanged(object sender, TrackRateChangedEventArgs e) => ForceUpdateLNSize();

        private void On7KScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e) => ForceUpdateLNSize();

        private void On4KScrollSpeedChanged(object sender, BindableValueChangedEventArgs<int> e) => ForceUpdateLNSize();
    }
}
