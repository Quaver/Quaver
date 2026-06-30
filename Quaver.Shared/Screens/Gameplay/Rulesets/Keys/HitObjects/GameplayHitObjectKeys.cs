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
using Quaver.Shared.Audio;
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
        new public NoteControllerKeys Info { get; private set; }

        /// <summary>
        /// </summary>
        private GameplayRulesetKeys Ruleset { get; }

        /// <summary>
        ///     Reference to the HitObjectManager controlling the object.
        /// </summary>
        private HitObjectManagerKeys HitObjectManager { get; }

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
        public ScrollDirection ScrollDirection { get; set; }

        /// <summary>
        ///     The actual HitObject sprite.
        /// </summary>
        public Sprite HitObjectSprite { get; private set; }

        /// <summary>
        ///     The hold body sprite for long notes.
        /// </summary>
        public AnimatableSprite LongNoteBodySprite { get; private set; }

        /// <summary>
        ///     The hold end sprite for long notes.
        /// </summary>
        public Sprite LongNoteEndSprite { get; private set; }

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
        private float LongNoteSizeDifference { get; set; }

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

        /// <summary>
        ///     Current size of the LN body sprite.
        /// </summary>
        private float CurrentLongNoteBodySize { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public GameplayHitObjectKeys(NoteControllerKeys info, GameplayRulesetKeys ruleset, HitObjectManagerKeys manager)
        {
            Info = info;
            HitObjectManager = manager;
            Ruleset = ruleset;

            var lane = info.Lane - 1;
            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;

            InitializeSprites(ruleset, lane, playfield.ScrollDirections[lane]);
            InitializeObject(manager, info);
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="lane"></param>
        /// <param name="ruleset"></param>
        /// <param name="manager"></param>
        public GameplayHitObjectKeys(int lane, GameplayRulesetKeys ruleset, HitObjectManagerKeys manager)
        {
            Info = null;
            HitObjectManager = manager;
            Ruleset = ruleset;

            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;

            InitializeSprites(ruleset, lane, playfield.ScrollDirections[lane]);
            Hide();
        }

        /// <summary>
        ///     Initialize HitObject Sprites used for Object Pooling. Only gets initialized once upon object creation.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ruleset"></param>
        private void InitializeSprites(GameplayRulesetKeys ruleset, int lane, ScrollDirection direction)
        {
            // Reference variables
            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;
            var posX = playfield.Stage.Receptors[lane].X;
            var flipNoteBody = direction.Equals(ScrollDirection.Up) &&
                               SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteImagesOnUpscroll;
            ScrollDirection = direction;

            var scale = ConfigManager.GameplayNoteScale.Value / 100f;
            var laneSize = playfield.LaneSize * scale;

            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
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
                Size = new ScalableVector2(laneSize, 0),
                Position = new ScalableVector2(posX, 0)
            };

            // Create the Hold End
            LongNoteEndSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                Size = new ScalableVector2(laneSize, 0)
            };

            // Set long note end properties.
            LongNoteEndSprite.Image = SkinManager.Skin.Keys[ruleset.Mode].NoteHoldEnds[lane];
            LongNoteEndSprite.Height = laneSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.Height / 2f;

            // Hits go above the hit object.
            PressHit = new DrawableReplayHit(Ruleset, HitObjectManager, lane);
            ReleaseHit = new DrawableReplayHit(Ruleset, HitObjectManager, lane);
        }

        /// <summary>
        ///     Initialize Object when created/reused from its object pool
        /// </summary>
        /// <param name="info"></param>
        /// <param name="manager"></param>
        public void InitializeObject(HitObjectManagerKeys manager, NoteControllerKeys info)
        {
            var hitObjectType = info.HitObjectInfo.Type;
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            HitPosition = info.IsLongNote
                ? playfield.HoldHitPositionY[(int)hitObjectType, info.Lane - 1]
                : playfield.HitPositionY[(int)hitObjectType, info.Lane - 1];
            HoldEndHitPosition = playfield.HoldEndHitPositionY[(int)hitObjectType, info.Lane - 1];
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
                    defaultLaneSize = skin.WidthForNoteHeightScale > 0
                        ? skin.WidthForNoteHeightScale
                        : playfield.LaneSize;
                }
            }

            Tint = info.State == HitObjectState.Dead ? SkinManager.Skin.Keys[Ruleset.Mode].DeadNoteColor : Color.White;
            var tint = Tint * (HitObjectManager.ShowHits ? HitObjectManagerKeys.SHOW_HITS_NOTE_ALPHA : 1);
            tint.A = 255;

            // Update hitobject sprites
            HitObjectSprite.Image = GetHitObjectTexture(info.Lane, manager.Ruleset.Mode);
            HitObjectSprite.Visible = true;
            HitObjectSprite.Tint = tint;
            StopLongNoteAnimation();

            // Update hit body's size to match image ratio
            HitObjectSprite.Size = new ScalableVector2(laneSize,
                defaultLaneSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);
            LongNoteBodySprite.Width = laneSize;
            LongNoteEndSprite.Width = laneSize;
            LongNoteBodyOffset = HitObjectSprite.Height / 2;


            // Update hitobject sprites depending on LN or not
            if (!Info.IsLongNote)
            {
                LongNoteEndSprite.Visible = false;
                LongNoteBodySprite.Visible = false;
            }
            else
            {
                LongNoteSizeDifference = playfield.LongNoteSizeAdjustment[(int)hitObjectType, info.Lane - 1];

                LongNoteBodySprite.Tint = tint;
                LongNoteEndSprite.Tint = tint;
                var bodies = GetHoldBodyTexture(info.Lane, manager.Ruleset.Mode);
                LongNoteBodySprite.ReplaceFrames(bodies);

                LongNoteEndSprite.Image = GetHoldEndTexture(info.Lane, manager.Ruleset.Mode);
                LongNoteEndSprite.Visible = SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd;
                LongNoteBodySprite.Visible = true;

                // Set long note end properties.
                LongNoteEndSprite.Height = laneSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
                LongNoteEndOffset = LongNoteEndSprite.Height / 2f;

                InitializeLongNoteSize();

                var flipNoteEnd = playfield.ScrollDirections[info.Lane - 1].Equals(ScrollDirection.Up) &&
                                  SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteEndImagesOnUpscroll;
                if (Info.ShouldFlipLongNoteEnd)
                    // LN ends on negative SV => end should be flipped (since it's going upside down).
                    flipNoteEnd = !flipNoteEnd;

                LongNoteEndSprite.SpriteEffect = flipNoteEnd ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }

            InitializeHits();

            // Update Positions
            UpdateSpritePositions(manager.CurrentVisualAudioOffset);
            var hitObjectContainer = GetHitObjectContainer(playfield);
            LongNoteBodySprite.Parent = hitObjectContainer;
            LongNoteEndSprite.Parent = hitObjectContainer;

            // We set the parent of the HitObjectSprite **AFTER** we create the long note
            // so that the body of the long note isn't drawn over the object.
            HitObjectSprite.Parent = hitObjectContainer;
        }

        private Container GetHitObjectContainer(GameplayPlayfieldKeys playfield)
        {
            var editorLayer = Info.HitObjectInfo.EditorLayer;

            if (editorLayer < 0 || editorLayer >= playfield.Stage.HitObjectContainers.Length)
                return playfield.Stage.HitObjectContainers[0];

            return playfield.Stage.HitObjectContainers[editorLayer];
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

            if (!HitObjectManager.HitStats.ContainsKey(Info.HitObjectInfo))
                return;

            var hitStats = HitObjectManager.HitStats[Info.HitObjectInfo];

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
        ///     Updates the earliest and latest track positions as well as the current LN body size.
        /// </summary>
        public void InitializeLongNoteSize()
        {
            Info.InitializeLongNoteSize();
            CurrentLongNoteBodySize = Info.CurrentLongNoteBodySize - LongNoteSizeDifference;
        }

        /// <summary>
        ///     Updates the earliest and latest track positions as well as the current LN body size.
        /// </summary>
        public void UpdateLongNoteSize(double curTime)
        {
            if (Info.State is HitObjectState.Alive)
            {
                Info.InitializeLongNoteSize();
                CurrentLongNoteBodySize = Info.CurrentLongNoteBodySize - LongNoteSizeDifference;
            }

            if (Info.State != HitObjectState.Held || !SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd)
                return;

            Info.UpdateLongNoteSize(curTime);
            CurrentLongNoteBodySize = Info.CurrentLongNoteBodySize - LongNoteSizeDifference;
        }

        /// <summary>
        ///     Will forcibly update LN on scroll speed change or specific modifier.
        /// </summary>
        public void ForceUpdateLongnote(double curTime)
        {
            // When LN end is not drawn, the LNs don't change their size as they are held.
            // So we only need to update if DrawLongNoteEnd is true.
            // The IsLongNote check is because UpdateLongNoteSize uses a property that is only initialized for LNs.
            if (Info.IsLongNote && SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd)
                UpdateLongNoteSize(curTime);

            UpdateSpritePositions(curTime);
        }

        /// <summary>
        ///     A constant reduction in size of the LN based on Percy amount
        /// </summary>
        private float PercyReduction =>
            Info.TimingGroupController.ScrollSpeed * ConfigManager.PercyAmount.Value *
            AudioEngine.Track.Rate;

        /// <summary>
        ///     Shrinks the height based on Percy amount
        /// </summary>
        /// <param name="height">Original unmodified height before applying Percy.</param>
        /// <returns>Shrunk height, minimum 0.</returns>
        private float PercyHeight(double height)
        {
            return (float)Math.Max(0, height - PercyReduction);
        }

        /// <summary>
        ///     Adjusts the position based on Percy amount
        /// </summary>
        /// <param name="position">Original unadjusted position value.</param>
        /// <returns>Adjusted position after applying Percy amount.</returns>
        private float PercyPosition(double position)
        {
            if (ScrollDirection.Equals(ScrollDirection.Down))
                return (float)(position + PercyReduction);
            return (float)(position - PercyReduction);
        }

        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        public void UpdateSpritePositions(double curTime)
        {
            Info.UpdatePositions(curTime);
            // Update Sprite position with regards to LN's state
            //
            // If the LN end is not drawn, don't move the LN start up with time since it ends up sliding above the LN in
            // the end.
            float spritePosition;

            UpdateLongNoteSize(curTime);
            if (Info.State == HitObjectState.Held && SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd)
            {
                if (curTime > Info.StartTime)
                    spritePosition = HitPosition;
                else
                    spritePosition = Info.GetSpritePosition(HitPosition, Info.InitialTrackPosition);
            }
            else
            {
                spritePosition = Info.GetSpritePosition(HitPosition, Info.InitialTrackPosition);
            }

            // Update HitBody
            HitObjectSprite.Y = spritePosition;

            PressHit.UpdateSpritePositions();
            ReleaseHit.UpdateSpritePositions();

            // Disregard the rest if it isn't a long note.
            if (!Info.IsLongNote)
                return;

            //Update HoldBody Position and Size
            var currentLongNoteBodySize = CurrentLongNoteBodySize;
            var earliestHeldPosition = Info.GetSpritePosition(HitPosition, Info.EarliestHeldPosition);

            var longNoteBodyHeight = PercyHeight(currentLongNoteBodySize);
            LongNoteBodySprite.Height = longNoteBodyHeight;

            if (ScrollDirection.Equals(ScrollDirection.Down))
                LongNoteBodySprite.Y = earliestHeldPosition + LongNoteBodyOffset - longNoteBodyHeight;
            else
                LongNoteBodySprite.Y = earliestHeldPosition + LongNoteBodyOffset;

            LongNoteEndSprite.Y = PercyPosition(Info.GetSpritePosition(HoldEndHitPosition, Info.LatestHeldPosition));

            // Stop drawing LN body + end if the ln reaches half the height of the hitobject
            // (prevents body + end extending below this point)
            var longNoteOverlap =
                longNoteBodyHeight + LongNoteSizeDifference <= HitObjectSprite.Height / 2f ||
                longNoteBodyHeight <= 0 ||
                                  curTime >= Info.EndTime && Info.State is HitObjectState.Held or HitObjectState.Dead;
            LongNoteEndSprite.Visible = !longNoteOverlap && SkinManager.Skin.Keys[Ruleset.Mode].DrawLongNoteEnd;
            LongNoteBodySprite.Visible = !longNoteOverlap;
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
        private Texture2D GetHitObjectTexture(int lane, GameMode mode)
        {
            lane = lane - 1;
            var skin = SkinManager.Skin.Keys[mode];
            var objects = Info.HitObjectInfo.Type switch
            {
                HitObjectType.Mine when Info.IsLongNote => skin.NoteMineStarts[lane],
                HitObjectType.Mine when !Info.IsLongNote => skin.NoteMines[lane],
                HitObjectType.Normal when Info.IsLongNote => skin.NoteHoldHitObjects[lane],
                HitObjectType.Normal when !Info.IsLongNote => skin.NoteHitObjects[lane],
                _ => throw new ArgumentOutOfRangeException(nameof(Info.HitObjectInfo.Type),
                    "Unknown HitObjectType for GetHitObjectTexture")
            };

            if (skin.ColorObjectsBySnapDistance &&
                HitObjectManager.SnapIndices.TryGetValue(Info.StartTime, out var snap))
            {
                return snap < objects.Count ? objects[snap] : objects[^1];
            }

            return objects.First();
        }

        /// <summary>
        ///     Gets the correct Hold Body texture also based on if we have note snapping and if
        ///     the note is a long note or note.
        /// </summary>
        /// <returns></returns>
        private List<Texture2D> GetHoldBodyTexture(int lane, GameMode mode)
        {
            lane = lane - 1;
            var skin = SkinManager.Skin.Keys[mode];

            return Info.HitObjectInfo.Type switch
            {
                HitObjectType.Mine when Info.IsLongNote => skin.NoteMineBodies[lane],
                HitObjectType.Normal when Info.IsLongNote => skin.NoteHoldBodies[lane],
                _ => throw new ArgumentOutOfRangeException(nameof(Info.HitObjectInfo.Type),
                    "Unknown HitObjectType for GetHitObjectTexture")
            };
        }

        /// <summary>
        ///     Gets the correct Hold End texture also based on if we have note snapping and if
        ///     the note is a long note or note.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHoldEndTexture(int lane, GameMode mode)
        {
            lane = lane - 1;
            var skin = SkinManager.Skin.Keys[mode];

            return Info.HitObjectInfo.Type switch
            {
                HitObjectType.Mine when Info.IsLongNote => skin.NoteMineEnds[lane],
                HitObjectType.Normal when Info.IsLongNote => skin.NoteHoldEnds[lane],
                _ => throw new ArgumentOutOfRangeException(nameof(Info.HitObjectInfo.Type),
                    "Unknown HitObjectType for GetHitObjectTexture")
            };
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
        public void StartLongNoteAnimation() => LongNoteBodySprite.StartLoop(Direction.Forward, 30);

        /// <summary>
        ///     Stops looping the long note sprite.
        ///     It will only be initiated when the player releases the note.
        /// </summary>
        public void StopLongNoteAnimation() => LongNoteBodySprite.StopLoop();

        /// <summary>
        ///     Hide all sprites associated with the hitobject.
        /// </summary>
        public void Hide()
        {
            HitObjectSprite.Visible = false;
            LongNoteBodySprite.Visible = false;
            LongNoteEndSprite.Visible = false;

            PressHit.Visible = false;
            ReleaseHit.Visible = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="lane"></param>
        /// <returns></returns>
        public static float GetObjectRotation(GameMode mode, int lane)
        {
            return SkinManager.Skin.Keys[mode].HitObjectRotations[lane] / 180f * MathF.PI;
        }
    }
}
