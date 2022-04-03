/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield.Hits;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    public class GameplayHitObjectKeys : GameplayHitObject
    {
        /// <summary>
        /// </summary>
        private GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Reference to the HitObjectManager controlling the object.
        /// </summary>
        private HitObjectManagerKeys HitObjectManager { get; }

        /// <summary>
        ///     Changes of SV direction during this LN.
        ///
        ///     Used for computing the earliest and latest visible position of this LN.
        /// </summary>
        private List<SVDirectionChange> SVDirectionChanges { get; set; }

        /// <summary>
        ///     Is determined by whether the player is holding the key that this hit object is binded to
        /// </summary>
        public bool CurrentlyBeingHeld { get; set; }

        /// <summary>
        ///     The Y-Offset from the receptor. >0 = this object hasnt passed receptors.
        /// </summary>
        public long InitialTrackPosition { get; set; }

        /// <summary>
        ///     Position of the LN end sprite.
        /// </summary>
        public long EndTrackPosition { get; set; }

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
        private long EarliestTrackPosition { get; set; }

        /// <summary>
        ///     Current size of the LN body sprite.
        /// </summary>
        private float CurrentLongNoteBodySize { get; set; }

        /// <summary>
        ///      The offset of the long note body from the hit object.
        ///
        ///      LN bodies are drawn from the middle of the start object to the middle of the end object. This is the
        ///      half-size of the start object.
        /// </summary>
        private float LongNoteBodyOffset { get; set; }

        /// <summary>
        ///     The offset of the hold end from hold body.
        ///
        ///     LN bodies are drawn from the middle of the start object to the middle of the end object. This is the
        ///     half-size of the end object.
        /// </summary>
        private float LongNoteEndOffset { get; set; }

        /// <summary>
        ///     The direction this hit object is traveling.
        /// </summary>
        private ScrollDirection ScrollDirection { get; set; }

        /// <summary>
        ///     The actual HitObject sprite.
        /// </summary>
        public AnimatableSprite HitObjectSprite { get; private set; }

        /// <summary>
        ///     The hold body sprite for long notes.
        /// </summary>
        public AnimatableSprite LongNoteBodySprite { get; private set; }

        /// <summary>
        ///     The hold end sprite for long notes.
        /// </summary>
        public AnimatableSprite LongNoteEndSprite { get; private set; }

        /// <summary>
        ///     General Position for hitting. Calculated from Hit Body Height and Hit Position Offset
        /// </summary>
        private float HitPosition { get; set; }

        /// <summary>
        ///     Position for LN ends.
        /// </summary>
        private float HoldEndHitPosition { get; set; }

        /// <summary>
        ///     Difference between the actual LN length and the LN body sprite length.
        ///
        ///     LN bodies are drawn from the middle of the start object to the middle of the end object, and this
        ///     difference takes those two half-heights into account.
        /// </summary>
        private float LongNoteSizeDifference { get; }

        /// <summary>
        ///     Base tint of the sprites.
        ///
        ///     Used for tint animation.
        /// </summary>
        public Color Tint { get; private set; }

        /// <summary>
        ///     The hit representing the press of this object.
        /// </summary>
        private DrawableReplayHit PressHit { get; set; }

        /// <summary>
        ///     The hit representing the release of this object.
        /// </summary>
        private DrawableReplayHit ReleaseHit { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public GameplayHitObjectKeys(HitObjectInfo info, GameplayRulesetKeys ruleset, HitObjectManagerKeys manager)
        {
            HitObjectManager = manager;
            Ruleset = ruleset;

            var lane = info.Lane - 1;
            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;

            LongNoteSizeDifference = playfield.LongNoteSizeAdjustment[lane];

            InitializeSprites(ruleset, lane, playfield.ScrollDirections[lane]);
            InitializeObject(manager, info);
        }

        /// <summary>
        ///     Initialize HitObject Sprite used for Object Pooling. Only gets initialized once upon object creation.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ruleset"></param>
        private void InitializeSprites(GameplayRulesetKeys ruleset, int lane, ScrollDirection direction)
        {
            // Reference variables
            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;
            var posX = playfield.Stage.Receptors[lane].X;
            var flipNoteBody = direction.Equals(ScrollDirection.Up) && SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteImagesOnUpscroll;
            ScrollDirection = direction;

            var scale = ConfigManager.GameplayNoteScale.Value / 100f;
            var laneSize = playfield.LaneSize * scale;

            // Create the base HitObjectSprite
            var notes = SkinManager.Skin.Keys[ruleset.Mode].NoteHitObjects[lane];
            HitObjectSprite = new AnimatableSprite(notes)
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                SpriteEffect = flipNoteBody ? SpriteEffects.FlipVertically : SpriteEffects.None
            };

            // Handle rotating the objects automatically
            if (SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].RotateHitObjectsByColumn)
                HitObjectSprite.Rotation = GetObjectRotation(MapManager.Selected.Value.Mode, lane);

            // Create Hold Body
            var bodies = SkinManager.Skin.Keys[ruleset.Mode].NoteHoldBodies[lane];
            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(laneSize , 0),
                Position = new ScalableVector2(posX, 0),
                Parent = playfield.Stage.HitObjectContainer
            };

            // Create the Hold End
            var end = SkinManager.Skin.Keys[ruleset.Mode].NoteHoldEnds[lane];
            LongNoteEndSprite = new AnimatableSprite(end)
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                Size = new ScalableVector2(laneSize, 0),
                Parent = playfield.Stage.HitObjectContainer
            };

            // We set the parent of the HitObjectSprite **AFTER** we create the long note
            // so that the body of the long note isn't drawn over the object.
            HitObjectSprite.Parent = playfield.Stage.HitObjectContainer;

            // Hits go above the hit object.
            PressHit = new DrawableReplayHit(Ruleset, HitObjectManager, lane);
            ReleaseHit = new DrawableReplayHit(Ruleset, HitObjectManager, lane);
        }

        /// <summary>
        ///     Initialize Object when created/recycled within its object pool.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="manager"></param>
        public void InitializeObject(HitObjectManagerKeys manager, HitObjectInfo info)
        {
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            HitPosition = info.IsLongNote ? playfield.HoldHitPositionY[info.Lane - 1] : playfield.HitPositionY[info.Lane - 1];
            HoldEndHitPosition = playfield.HoldEndHitPositionY[info.Lane - 1];
            Info = info;

            var scale = ConfigManager.GameplayNoteScale.Value / 100f;
            var skin = SkinManager.Skin.Keys[Ruleset.Map.Mode];
            var objectWidth = playfield.LaneSize;

            var laneSize = objectWidth * scale;
            var defaultLaneSize = skin.WidthForNoteHeightScale > 0 ? skin.WidthForNoteHeightScale : laneSize;

            if (Ruleset.Screen.Map.HasScratchKey)
            {
                if (info.Lane == Ruleset.Screen.Map.GetKeyCount())
                {
                    laneSize = skin.ScratchLaneSize * scale;
                    defaultLaneSize = skin.WidthForNoteHeightScale > 0 ? skin.WidthForNoteHeightScale : playfield.LaneSize;
                }
            }

            Tint = Color.White;
            var tint = Tint * (HitObjectManager.ShowHits ? HitObjectManagerKeys.SHOW_HITS_NOTE_ALPHA : 1);
            tint.A = 255;

            // Update Hit Object State
            HitObjectSprite.Image = GetHitObjectTexture(HitObjectSprite, info.Lane, manager.Ruleset.Mode);
            HitObjectSprite.Visible = true;
            HitObjectSprite.Tint = tint;

            var lastHitObjectFrame = HitObjectSprite.DefaultFrame + (SkinManager.Skin.Keys[Ruleset.Mode].NoteHitObjects[Info.Lane].Count / 9);
            HitObjectSprite.StartLoop(Direction.Forward, SkinManager.Skin?.Keys[manager.Ruleset.Mode].HitObjectsFps ?? 5, 0, lastHitObjectFrame);

            // Set long note end properties.
            LongNoteEndSprite.Image = GetHitObjectTexture(LongNoteEndSprite, info.Lane, manager.Ruleset.Mode, true, true);
            LongNoteEndSprite.Height = laneSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.Height / 2f;

            // Update the long note body sprite.
            LongNoteBodySprite.Image = GetHitObjectTexture(LongNoteBodySprite, info.Lane, manager.Ruleset.Mode, true, false);

            InitialTrackPosition = manager.GetPositionFromTime(Info.StartTime);
            CurrentlyBeingHeld = false;
            StopLongNoteAnimation();

            // Update hit body's size to match image ratio
            HitObjectSprite.Size = new ScalableVector2(laneSize, defaultLaneSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);
            LongNoteBodySprite.Width = laneSize;
            LongNoteEndSprite.Width = laneSize;
            LongNoteBodyOffset = HitObjectSprite.Height / 2;

            // Update Hit Object State depending if its an LN or not
            if (!Info.IsLongNote)
            {
                LongNoteEndSprite.Visible = false;
                LongNoteBodySprite.Visible = false;
                LatestTrackPosition = InitialTrackPosition;
            }
            else
            {
                SVDirectionChanges = HitObjectManager.GetSVDirectionChanges(info.StartTime, info.EndTime);

                LongNoteBodySprite.Tint = tint;
                LongNoteEndSprite.Tint = tint;
                LongNoteEndSprite.Visible = SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd;
                LongNoteBodySprite.Visible = true;
                EndTrackPosition = manager.GetPositionFromTime(Info.EndTime);
                UpdateLongNoteSize(InitialTrackPosition, Info.StartTime);

                var flipNoteEnd = playfield.ScrollDirections[info.Lane - 1].Equals(ScrollDirection.Up) && SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteEndImagesOnUpscroll;
                if (HitObjectManager.IsSVNegative(info.EndTime))
                    // LN ends on negative SV => end should be flipped (since it's going upside down).
                    flipNoteEnd = !flipNoteEnd;

                LongNoteEndSprite.SpriteEffect = flipNoteEnd ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }

            InitializeHits();

            // Update Positions
            UpdateSpritePositions(manager.CurrentTrackPosition, manager.CurrentVisualPosition);
        }

        /// <summary>
        ///     Initializes press and release hits with new data.
        /// </summary>
        private void InitializeHits()
        {
            PressHit.Visible = false;
            ReleaseHit.Visible = false;

            if (HitObjectManager.HitStats == null)
                return;

            if (!HitObjectManager.HitStats.ContainsKey(Info))
                return;

            var hitStats = HitObjectManager.HitStats[Info];

            foreach (var hitStat in hitStats)
            {
                if (hitStat.KeyPressType == KeyPressType.Release ||
                    hitStat.KeyPressType == KeyPressType.None && hitStat.Judgement == Judgement.Okay)
                {
                    ReleaseHit.InitializeWithHitStat(hitStat);
                    ReleaseHit.Visible = true;
                }
                else
                {
                    PressHit.InitializeWithHitStat(hitStat);
                    PressHit.Visible = true;
                }
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            HitObjectSprite.Destroy();
            LongNoteBodySprite.Destroy();
            LongNoteEndSprite.Destroy();
            PressHit.Destroy();
            ReleaseHit.Destroy();
        }

        /// <summary>
        ///     Calculates the position of the Hit Object with a position offset.
        /// </summary>
        /// <returns></returns>
        public float GetSpritePosition(long offset, float initialPos) => HitPosition + ((initialPos - offset) * (ScrollDirection.Equals(ScrollDirection.Down) ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed) / HitObjectManagerKeys.TrackRounding);

        /// <summary>
        ///     Calculates the position of the end Hit Object with a position offset.
        /// </summary>
        /// <returns></returns>
        public float GetEndSpritePosition(long offset, float initialPos) => HoldEndHitPosition + ((initialPos - offset) * (ScrollDirection.Equals(ScrollDirection.Down) ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed) / HitObjectManagerKeys.TrackRounding);

        /// <summary>
        ///     Updates the earliest and latest track positions as well as the current LN body size.
        /// </summary>
        public void UpdateLongNoteSize(long offset, double curTime)
        {
            var startPosition = InitialTrackPosition;
            if (curTime >= Info.StartTime)
                // If we're past the LN start, start from the current position.
                startPosition = offset;

            var earliestPosition = Math.Min(startPosition, EndTrackPosition);
            var latestPosition = Math.Max(startPosition, EndTrackPosition);

            foreach (var change in SVDirectionChanges)
            {
                if (curTime >= change.StartTime)
                    // We're past this change already.
                    continue;

                earliestPosition = Math.Min(earliestPosition, change.Position);
                latestPosition = Math.Max(latestPosition, change.Position);
            }

            EarliestTrackPosition = earliestPosition;
            LatestTrackPosition = latestPosition;
            CurrentLongNoteBodySize = (LatestTrackPosition - EarliestTrackPosition) * HitObjectManagerKeys.ScrollSpeed / HitObjectManagerKeys.TrackRounding - LongNoteSizeDifference;
        }

        /// <summary>
        ///     Will forcibly update LN on scroll speed change or specific modifier.
        /// </summary>
        public void ForceUpdateLongnote(long offset, double curTime)
        {
            // When LN end is not drawn, the LNs don't change their size as they are held.
            // So we only need to update if DrawLongNoteEnd is true.
            // The IsLongNote check is because UpdateLongNoteSize uses a property that is only initialized for LNs.
            if (Info.IsLongNote && SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd)
                UpdateLongNoteSize(offset, curTime);

            UpdateSpritePositions(offset, curTime);
        }

        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        public void UpdateSpritePositions(long offset, double curTime)
        {
            // Update Sprite position with regards to LN's state
            //
            // If the LN end is not drawn, don't move the LN start up with time since it ends up sliding above the LN in
            // the end.
            float spritePosition;
            if (CurrentlyBeingHeld && SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd)
            {
                UpdateLongNoteSize(offset, curTime);

                if (curTime > Info.StartTime)
                    spritePosition = HitPosition;
                else
                    spritePosition = GetSpritePosition(offset, InitialTrackPosition);
            }
            else
            {
                spritePosition = GetSpritePosition(offset, InitialTrackPosition);
            }

            // Update HitBody
            HitObjectSprite.Y = spritePosition;

            PressHit.UpdateSpritePositions(offset);
            ReleaseHit.UpdateSpritePositions(offset);

            // Disregard the rest if it isn't a long note.
            if (!Info.IsLongNote)
                return;

            //Update HoldBody Position and Size
            LongNoteBodySprite.Height = CurrentLongNoteBodySize;

            var earliestSpritePosition = GetSpritePosition(offset, EarliestTrackPosition);
            if (ScrollDirection.Equals(ScrollDirection.Down))
                LongNoteBodySprite.Y = earliestSpritePosition + LongNoteBodyOffset - CurrentLongNoteBodySize;
            else
                LongNoteBodySprite.Y = earliestSpritePosition + LongNoteBodyOffset;

            LongNoteEndSprite.Y = GetEndSpritePosition(offset, EndTrackPosition);

            // Stop drawing LN body + end if the ln reaches half the height of the hitobject
            // (prevents body + end extending below this point)
            if (CurrentLongNoteBodySize + LongNoteSizeDifference <= HitObjectSprite.Height / 2f || CurrentLongNoteBodySize <= 0 || curTime >= Info.EndTime && CurrentlyBeingHeld)
            {
                LongNoteEndSprite.Visible = false;
                LongNoteBodySprite.Visible = false;
            }
        }

        /// <summary>
        ///     Gets the correct HitObject texture also based on if we have note snapping and if
        ///     the note is a long note or note.
        ///
        ///     If the user has ColourObjectsBySnapDistance enabled in their skin, we load the one with their
        ///     specified color.
        ///
        ///     If not, we default it to the first beat snap in the list.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHitObjectTexture(AnimatableSprite sprite, int lane, GameMode mode, bool longNoteSprite = false, bool isLongNoteEnd = false)
        {
            lane = lane - 1;
            var skin = SkinManager.Skin.Keys[mode];

            if (skin.ColorObjectsBySnapDistance)
            {
                var objects = Info.IsLongNote ? skin.NoteHoldHitObjects[lane] : skin.NoteHitObjects[lane];

                if (longNoteSprite)
                    objects = isLongNoteEnd ? skin.NoteHoldEnds[lane] : skin.NoteHoldBodies[lane];

                if (HitObjectManager.SnapIndices.ContainsKey(Info))
                {
                    var snap = HitObjectManager.SnapIndices[Info];
                    var columns = objects.Count / 9;

                    sprite.DefaultFrame = snap * columns;

                    return snap < objects.Count ? objects[snap * columns] : objects[objects.Count - 1];
                }

                return objects.First();
            }

            if (longNoteSprite)
                return isLongNoteEnd ? skin.NoteHoldEnds[lane].First() : skin.NoteHoldBodies[lane].First();

            return Info.IsLongNote ? skin.NoteHoldHitObjects[lane].First() : skin.NoteHitObjects[lane].First();
        }

        /// <summary>
        ///     When the object itself dies, we want to change it to a dead color.
        /// </summary>
        public void Kill()
        {
            Tint = SkinManager.Skin.Keys[Ruleset.Mode].DeadNoteColor;
            var tint = Tint * (HitObjectManager.ShowHits ? HitObjectManagerKeys.SHOW_HITS_NOTE_ALPHA : 1);
            tint.A = 255;
            HitObjectSprite.Tint = tint;
            if (Info.IsLongNote)
            {
                LongNoteBodySprite.Tint = tint;
                LongNoteEndSprite.Tint = tint;
            }
        }

        /// <summary>
        ///     Fades out the object. Usually used for failure.
        /// </summary>
        /// <param name="dt"></param>
        public void FadeOut(double dt)
        {
            // HitObjectSprite.FadeOut(dt, 240);

            if (!Info.IsLongNote)
                return;

            // LongNoteBodySprite.FadeOut(dt, 240);
            // LongNoteEndSprite.FadeOut(dt, 240);
        }

        /// <summary>
        ///     Starts looping the long note sprite.
        ///     It will only be initiated when the player presses the note.
        /// </summary>
        public void StartLongNoteAnimation()
        {
            var lastHoldBodyFrame = LongNoteBodySprite.DefaultFrame + (SkinManager.Skin.Keys[Ruleset.Mode].NoteHoldBodies[Info.Lane].Count / 9);
            var lastHoldEndFrame = LongNoteEndSprite.DefaultFrame + (SkinManager.Skin.Keys[Ruleset.Mode].NoteHoldEnds[Info.Lane].Count / 9);
            LongNoteBodySprite.StartLoop(Direction.Forward, SkinManager.Skin?.Keys[Ruleset.Mode].HoldBodyFps ?? 5, 0, lastHoldBodyFrame);
            LongNoteEndSprite.StartLoop(Direction.Forward, SkinManager.Skin?.Keys[Ruleset.Mode].HoldEndFps ?? 5, 0, lastHoldEndFrame);
        }

        /// <summary>
        ///     Stops looping the long note sprite.
        ///     It will only be initiated when the player releases the note.
        /// </summary>
        public void StopLongNoteAnimation()
        {
            LongNoteBodySprite.StopLoop();
            LongNoteEndSprite.StopLoop();
        }

        /// <summary>
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="lane"></param>
        /// <returns></returns>
        public static float GetObjectRotation(GameMode mode, int lane)
        {
            switch (mode)
            {
                case GameMode.Keys4:
                    switch (lane)
                    {
                        case 0:
                            return  90;
                        case 1:
                            // Already downwards
                            break;
                        case 2:
                            return 180;
                        case 3:
                            return 270;
                    }
                    break;
                case GameMode.Keys7:
                    switch (lane)
                    {
                        case 0:
                            return 90;
                        case 1:
                            return 135;
                        case 2:
                            return 180;
                        case 3:
                            // Already downwards
                            break;
                        case 4:
                            return 180;
                        case 5:
                            return 225;
                        case 6:
                            return 270;
                    }

                    break;
            }

            return 0;
        }
    }
}
