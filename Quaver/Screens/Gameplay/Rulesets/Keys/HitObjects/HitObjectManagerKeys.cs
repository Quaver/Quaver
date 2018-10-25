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
        ///     Hit Object info used for object pool and gameplay
        /// </summary>
        public List<Queue<HitObjectInfo>> Info { get; set; }

        /// <summary>
        ///     Object pool for every hit object.
        ///     Every hit object in the pool is split by the hit object's lane
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> ObjectPool { get; set; }

        /// <summary>
        ///     The object pool size.
        /// </summary>
        public int InitialPoolSizePerLane { get; } = 2;

        /// <summary>
        ///     The position at which the next Hit Object must be at in order to add a new Hit Object to the pool.
        /// </summary>
        private float CreateObjectPosition { get; set; } = 1500;

        /// <summary>
        ///     The position at which the earliest Hit Object must be at before its recycled.
        /// </summary>
        private float RecycleObjectPosition { get; set; } = 1500;

        /// <summary>
        ///     Reference to the ruleset this HitObject manager is for.
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; private set; }

        /// <summary>
        ///     The list of dead notes (grayed out LN's)
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> DeadNotes { get; private set; }

        /// <summary>
        ///     The list of currently held long notes.
        /// </summary>
        public List<Queue<GameplayHitObjectKeys>> HeldLongNotes { get; private set; }

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

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int ObjectsLeft
        {
            get
            {
                var total = 0;
                foreach (var lane in ObjectPool) total += lane.Count;
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
                foreach (var lane in ObjectPool)
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
            Info = new List<Queue<HitObjectInfo>>(keyCount);
            ObjectPool = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            DeadNotes = new List<Queue<GameplayHitObjectKeys>>(keyCount);
            HeldLongNotes = new List<Queue<GameplayHitObjectKeys>>(keyCount);

            // Add HitObject Info to Info pool
            for (var i = 0; i < Ruleset.Map.GetKeyCount(); i++)
            {
                Info.Add(new Queue<HitObjectInfo>());
                ObjectPool.Add(new Queue<GameplayHitObjectKeys>(InitialPoolSizePerLane));
                DeadNotes.Add(new Queue<GameplayHitObjectKeys>());
                HeldLongNotes.Add(new Queue<GameplayHitObjectKeys>());
            }

            // Sort Hit Object Info into their respective lanes
            foreach (var info in map.HitObjects)
                Info[info.Lane - 1].Enqueue(info);
        }

        /// <summary>
        ///     Create the initial objects in the object pool
        /// </summary>
        private void InitializeObjectPool()
        {
            foreach (var lane in Info)
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
        private void CreatePoolObject(HitObjectInfo info) => ObjectPool[info.Lane - 1].Enqueue(new GameplayHitObjectKeys(info, Ruleset));

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateAndScoreActiveObjects();
            UpdateAndScoreHeldObjects();
            UpdateDeadObjects();
        }

        /// <summary>
        ///     Returns the earliest un-tapped Hit Object
        /// </summary>
        /// <param name="laneIndex"></param>
        /// <returns></returns>
        public GameplayHitObjectKeys GetClosestTap(int lane) => ObjectPool[lane].Count > 0 ? ObjectPool[lane].Peek() : null;

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
            foreach (var lane in Info)
            {
                while (lane.Count > 0 && Ruleset.Screen.TrackManager.GetPositionFromTime(lane.Peek().StartTime) - Ruleset.Screen.TrackManager.Position < CreateObjectPosition)
                {
                    CreatePoolObject(lane.Dequeue());
                }
            }

            // Check to see if the player missed any active notes
            foreach (var lane in ObjectPool)
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
            foreach (var lane in ObjectPool)
            {
                foreach (var hitObject in lane)
                {
                    hitObject.HandleAnimations();
                    hitObject.UpdateSpritePositions(Ruleset.Screen.TrackManager.Position);
                }
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
                {
                    hitObject.HandleAnimations();
                    hitObject.UpdateSpritePositions(Ruleset.Screen.TrackManager.Position);
                }
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
                    (Ruleset.Screen.TrackManager.Position > lane.Peek().LongNoteTrackPosition + RecycleObjectPosition))
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
                    hitObject.UpdateSpritePositions(Ruleset.Screen.TrackManager.Position);
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
            var lane = Info[hitObject.Info.Lane - 1];
            if (lane.Count > 0)
            {
                var info = lane.Dequeue();
                hitObject.InitializeObject(info, Ruleset);
                ObjectPool[info.Lane - 1].Enqueue(hitObject);
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
            hitObject.TrackPosition = Ruleset.Screen.TrackManager.GetPositionFromTime(time);
            hitObject.Info.StartTime = (int)time;
            hitObject.CurrentlyBeingHeld = false;
            hitObject.UpdateLongNoteSize(hitObject.TrackPosition);
            hitObject.Kill();

            // Add to dead notes pool
            DeadNotes[hitObject.Info.Lane - 1].Enqueue(hitObject);
        }
    }
}
