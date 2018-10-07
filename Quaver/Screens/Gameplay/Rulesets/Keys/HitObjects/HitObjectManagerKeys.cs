using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring.Data;
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
        ///     Reference to the ruleset this HitObject manager is for.
        /// </summary>
        public GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     The list of dead notes (grayed out LN's)
        /// </summary>
        public List<GameplayHitObject> DeadNotes { get; }

        /// <summary>
        ///     The list of currently held long notes.
        /// </summary>
        public List<GameplayHitObject> HeldLongNotes { get; }

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
        public override int ObjectsLeft => ObjectPool.Count + HeldLongNotes.Count + DeadNotes.Count;

        /// <summary>
        ///     The offset of the hit position.
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

        /// <summary>
        ///     The time it takes for a dead note to be removed after missing it.
        /// </summary>
        private uint DeadNoteRemovalTime { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="size"></param>
        public HitObjectManagerKeys(GameplayRulesetKeys ruleset, int size) : base(size)
        {
            Ruleset = ruleset;

            DeadNotes = new List<GameplayHitObject>();
            HeldLongNotes = new List<GameplayHitObject>();

            // Set the dead note removal time.
            DeadNoteRemovalTime = (uint)(1000 * AudioEngine.Track.Rate);
        }

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
        ///     Gets the index of the nearest object in a given lane.
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="songTime"></param>
        /// <returns></returns>
        public int GetIndexOfNearestLaneObject(int lane, double songTime)
        {
            // Search for closest ManiaHitObject that is inside the HitTiming Window
            for (var i = 0; i < PoolSize && i < ObjectPool.Count; i++)
            {
                if (ObjectPool[i].Info.Lane == lane && ObjectPool[i].Info.StartTime - songTime > -Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay])
                    return i;
            }

            // Search held long notes as well for the time being.
            for (var i = 0; i < HeldLongNotes.Count; i++)
            {
                if (HeldLongNotes[i].Info.Lane == lane && HeldLongNotes[i].Info.EndTime - songTime > -Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay] * Ruleset.ScoreProcessor.WindowReleaseMultiplier[Judgement.Okay])
                    return i;
            }

            return -1;
        }

        /// <summary>
        ///     Updates the active objects in the pool + adds to score when applicable.
        /// </summary>
        private void UpdateAndScoreActiveObjects()
        {
            // Update active objects.
            for (var i = 0; i < ObjectPool.Count && i < PoolSize; i++)
            {
                var hitObject = (GameplayHitObjectKeys)ObjectPool[i];

                // If the user misses the note.
                if ((int) Ruleset.Screen.Timing.Time > hitObject.TrueStartTime + Ruleset.ScoreProcessor.JudgementWindow[Judgement.Okay])
                {
                    // Add a miss to their score.
                    Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);

                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();

                    // Add new hit stat data.
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, hitObject.Info, (int) Ruleset.Screen.Timing.Time, Judgement.Miss,
                                            int.MinValue, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                    Ruleset.ScoreProcessor.Stats.Add(stat);

                    // Make the combo display visible since it is now changing.
                    var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
                    playfield.Stage.ComboDisplay.MakeVisible();

                    // Perform hit burst animation
                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(Judgement.Miss);

                    // If ManiaHitObject is an LN, kill it and count it as another miss because of the tail.
                    if (hitObject.IsLongNote)
                    {
                        KillPoolObject(i);
                        Ruleset.ScoreProcessor.CalculateScore(Judgement.Miss);

                        // Add a duplicate stat since it's an LN, and it counts as two misses.
                        Ruleset.ScoreProcessor.Stats.Add(stat);

                        screenView.UpdateScoreboardUsers();
                    }
                    // Otherwise recycle the object.
                    else
                    {
                        RecyclePoolObject(i);
                    }

                    i--;
                }
                // The note is still active and ready to be updated.
                else
                {
                    // Set new HitObject positions.
                    hitObject.PositionY = hitObject.GetPosFromOffset(hitObject.OffsetYFromReceptor);
                    hitObject.UpdateSpritePositions();
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

            // Update the currently held long notes.
            for (var i = 0; i < HeldLongNotes.Count; i++)
            {
                var hitObject = (GameplayHitObjectKeys)HeldLongNotes[i];

                // If the LN's release was missed. (Counts as an okay instead of a miss.)
                if ((int) Ruleset.Screen.Timing.Time > hitObject.TrueEndTime + window)
                {
                    // The judgement that is given when a user completely fails to release.
                    const Judgement missedJudgement = Judgement.Okay;

                    // Calc new score.
                    Ruleset.ScoreProcessor.CalculateScore(missedJudgement);

                    // Add new hit stat data.
                    var stat = new HitStat(HitStatType.Miss, KeyPressType.None, hitObject.Info, (int) Ruleset.Screen.Timing.Time, Judgement.Okay,
                                                int.MinValue, Ruleset.ScoreProcessor.Accuracy, Ruleset.ScoreProcessor.Health);
                    Ruleset.ScoreProcessor.Stats.Add(stat);

                    var screenView = (GameplayScreenView)Ruleset.Screen.View;
                    screenView.UpdateScoreboardUsers();

                    // Make the combo display visible since it is now changing.
                    var playfield = (GameplayPlayfieldKeys) Ruleset.Playfield;
                    playfield.Stage.ComboDisplay.MakeVisible();

                    // Perform hit burst animation
                    playfield.Stage.JudgementHitBurst.PerformJudgementAnimation(missedJudgement);

                    // Stop the hitlighting animation.
                    playfield.Stage.HitLightingObjects[hitObject.Info.Lane - 1].StopHolding();

                    // Remove from the queue of long notes.
                    hitObject.Destroy();
                    HeldLongNotes.RemoveAt(i);
                    i--;
                }
                // The long note is still active and ready to be updated.
                else
                {
                    // Start looping the long note body.
                    if (!hitObject.LongNoteBodySprite.IsLooping && !Ruleset.Screen.IsPaused)
                        hitObject.LongNoteBodySprite.StartLoop(Direction.Forward, 30);

                    // If it is looping however and the game is paused, we'll want to stop the loop.
                    else if (hitObject.LongNoteBodySprite.IsLooping && Ruleset.Screen.IsPaused)
                        hitObject.LongNoteBodySprite.StopLoop();

                    // Set the long note size and position.
                    if (Ruleset.Screen.Timing.Time > hitObject.TrueStartTime)
                    {
                        hitObject.CurrentLongNoteSize = (ulong)((hitObject.LongNoteOffsetYFromReceptor - Ruleset.Screen.Timing.Time) * ScrollSpeed);
                        hitObject.PositionY = hitObject.PositionY = hitObject.GetPosFromOffset((float)Ruleset.Screen.Timing.Time);
                    }
                    else
                    {
                        hitObject.CurrentLongNoteSize = hitObject.InitialLongNoteSize;
                        hitObject.PositionY = hitObject.GetPosFromOffset(hitObject.OffsetYFromReceptor);
                    }

                    // Update the sprite positions of the object.
                    hitObject.UpdateSpritePositions();
                }
            }
        }

        /// <summary>
        ///     Updates all of the dead objects in the pool.
        /// </summary>
        private void UpdateDeadObjects()
        {
            // Update dead objects.
            for (var i = 0; i < DeadNotes.Count; i++)
            {
                var hitObject = (GameplayHitObjectKeys)DeadNotes[i];

                // Stop looping the animation.
                if (hitObject.IsLongNote && hitObject.LongNoteBodySprite.IsLooping)
                    hitObject.LongNoteBodySprite.StopLoop();

                // If the note is past the time of removal, then destroy it.
                if (Ruleset.Screen.Timing.Time > hitObject.TrueEndTime + DeadNoteRemovalTime
                    && Ruleset.Screen.Timing.Time > hitObject.TrueStartTime + DeadNoteRemovalTime)
                {
                    // Remove from dead notes pool.
                    hitObject.Destroy();
                    DeadNotes.RemoveAt(i);
                    i--;
                }
                // Otherwise keep updating it accordingly.
                else
                {
                    hitObject.PositionY = hitObject.GetPosFromOffset(hitObject.OffsetYFromReceptor);
                    hitObject.UpdateSpritePositions();
                }
            }
        }

        /// <summary>
        ///     Kills a note at a specific index of the object pool.
        /// </summary>
        /// <param name="index"></param>
        public void KillPoolObject(int index)
        {
            var hitObject = (GameplayHitObjectKeys)ObjectPool[index];

            // Change the sprite color to dead.
            hitObject.ChangeSpriteColorToDead();

            // Add the object to the dead notes pool.
            DeadNotes.Add(hitObject);

            // Remove the old HitObject
            ObjectPool.RemoveAt(index);

            //Initialize the new ManiaHitObject (create the hit object sprites)
            InitializeNewPoolObject();
        }

        /// <summary>
        ///     Initializes a new note in the pool.
        /// </summary>
        private void InitializeNewPoolObject()
        {
            if (ObjectPool.Count >= PoolSize)
                ObjectPool[PoolSize - 1].InitializeSprite(Ruleset.Playfield);
        }

        /// <summary>
        ///     Recycles a pool object.
        /// </summary>
        /// <param name="index"></param>
        public void RecyclePoolObject(int index)
        {
            // Destroy and remove the old object.
            ObjectPool[index].Destroy();
            ObjectPool.RemoveAt(index);

            // Initialize a new one in its place at the end.
            InitializeNewPoolObject();
        }

        /// <summary>
        ///     Changes a pool object to a long note that is held at the receptors.
        /// </summary>
        /// <param name="index"></param>
        public void ChangePoolObjectStatusToHeld(int index)
        {
            // Add to the held long notes.
            HeldLongNotes.Add(ObjectPool[index]);

            // Remove the one from the object pool
            ObjectPool.RemoveAt(index);

            // Initialize the new note.
            InitializeNewPoolObject();
        }

        /// <summary>
        ///     Kills a hold pool object.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="destroy"></param>
        public void KillHoldPoolObject(int index, bool destroy = false)
        {
            var hitObject = (GameplayHitObjectKeys)HeldLongNotes[index];

            // Change start time and y offset.
            // TODO: This.
            hitObject.TrueStartTime = (float)Ruleset.Screen.Timing.Time;
            hitObject.OffsetYFromReceptor = hitObject.TrueStartTime;

            if (destroy)
            {
                hitObject.Destroy();
            }
            else
            {
                hitObject.ChangeSpriteColorToDead();
            }

            // Remove from the hold pool
            HeldLongNotes.RemoveAt(index);
            DeadNotes.Add(hitObject);
        }
    }
}
