/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
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
        ///     Is determined by whether the player is holding the key that this hit object is binded to
        /// </summary>
        public bool CurrentlyBeingHeld { get; set; }

        /// <summary>
        ///     The Y-Offset from the receptor. >0 = this object hasnt passed receptors.
        /// </summary>
        public long InitialTrackPosition { get; set; }

        /// <summary>
        ///     The long note Y offset from the receptor.
        ///     TODO: I don't think this gets updated upon scroll speed change
        /// </summary>
        public long InitialLongNoteTrackPosition { get; private set; }

        /// <summary>
        ///     The Y position of the HitObject Sprites.
        /// </summary>
        private float SpritePosition { get; set; }

        /// <summary>
        ///     The width of the object.
        /// </summary>
        private float Width { get; set; }

        /// <summary>
        ///     The initial size of this object's long note.
        /// </summary>
        public float InitialLongNoteSize { get; set; }

        /// <summary>
        ///     The current size of this object's long note.
        ///     TODO: I don't think this gets updated upon scroll speed change
        /// </summary>
        private float CurrentLongNoteSize { get; set; }

        /// <summary>
        ///      The offset of the long note body from the hit object.
        /// </summary>
        private float LongNoteBodyOffset { get; set; }

        /// <summary>
        ///     The offset of the hold end from hold body.
        /// </summary>
        private float LongNoteEndOffset { get; set; }

        /// <summary>
        ///     The direction this hit object is traveling.
        /// </summary>
        private ScrollDirection ScrollDirection { get; set; }

        /// <summary>
        ///     The actual HitObject sprite.
        /// </summary>
        private AnimatableSprite HitObjectSprite { get; set; }

        /// <summary>
        ///     The hold body sprite for long notes.
        /// </summary>
        private AnimatableSprite LongNoteBodySprite { get; set; }

        /// <summary>
        ///     The hold end sprite for long notes.
        /// </summary>
        private AnimatableSprite LongNoteEndSprite { get; set; }

        /// <summary>
        ///     General Distance from the receptor. Calculated from hit body size and global offset
        /// </summary>
        private float HitPositionOffset { get; set; }

        /// <summary>
        ///     Distance from the receptor for the current HitObjectSprite's image
        /// </summary>
        private float SpritePositionOffset => ScrollDirection.Equals(ScrollDirection.Down)
            ? HitPositionOffset - HitObjectSprite.Height
            : HitPositionOffset + HitObjectSprite.Height;

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

            HitPositionOffset = playfield.HitPositionOffsets[lane];
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
            var flipNoteEnd = direction.Equals(ScrollDirection.Up) && SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteEndImagesOnUpscroll;

            ScrollDirection = direction;

            // Create the base HitObjectSprite
            HitObjectSprite = new AnimatableSprite(SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].NoteHitObjects[0])
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                Rotation = SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].UseArrowsHitObject ? HitObjectManagerKeys.HitObjectRotations[MapManager.Selected.Value.Mode][lane] : 0,
                SpriteEffect = flipNoteBody ? SpriteEffects.FlipVertically : SpriteEffects.None
            };

            // Create Hold Body
            LongNoteBodySprite = new AnimatableSprite(SkinManager.Skin.Keys[ruleset.Mode].NoteHoldBodies[lane])
            {
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(playfield.LaneSize, 0),
                Position = new ScalableVector2(posX, 0),
                Parent = playfield.Stage.HitObjectContainer
            };

            // Create the Hold End
            LongNoteEndSprite = new AnimatableSprite(SkinManager.Skin.Keys[ruleset.Mode].NoteHoldEnds[lane])
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                Size = new ScalableVector2(playfield.LaneSize, 0),
                Parent = playfield.Stage.HitObjectContainer,
                SpriteEffect = flipNoteEnd ? SpriteEffects.FlipVertically : SpriteEffects.None
            };

            // Set long note end properties.
            LongNoteEndSprite.Height = playfield.LaneSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.Height / 2f;

            // We set the parent of the HitObjectSprite **AFTER** we create the long note
            // so that the body of the long note isn't drawn over the object.
            HitObjectSprite.Parent = playfield.Stage.HitObjectContainer;
        }

        /// <summary>
        ///     Initialize Object when created/recycled within its object pool.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="manager"></param>
        public void InitializeObject(HitObjectManagerKeys manager, HitObjectInfo info)
        {
            Info = info;

            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;

            // Update Hit Object State
            HitObjectSprite.ReplaceFrames(GetHitObjectFrames(info.Lane, manager.Ruleset.Mode));
            HitObjectSprite.StartLoop(Direction.Forward, 60);
            HitObjectSprite.Visible = true;
            HitObjectSprite.Tint = Color.White;
            InitialTrackPosition = manager.GetPositionFromTime(Info.StartTime);
            CurrentlyBeingHeld = false;
            StopLongNoteAnimation();

            // Update hit body's size to match image ratio
            HitObjectSprite.Size = new ScalableVector2(playfield.LaneSize, playfield.LaneSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);
            LongNoteBodyOffset = HitObjectSprite.Height / 2;

            // Update Hit Object State depending if its an LN or not
            if (!Info.IsLongNote)
            {
                LongNoteEndSprite.Visible = false;
                LongNoteBodySprite.Visible = false;
                InitialLongNoteTrackPosition = InitialTrackPosition;
            }
            else
            {
                LongNoteBodySprite.Tint = Color.White;
                LongNoteEndSprite.Tint = Color.White;
                LongNoteEndSprite.Visible = true;
                LongNoteBodySprite.Visible = true;
                LongNoteBodySprite.ChangeTo(0);
                LongNoteEndSprite.ChangeTo(0);
                InitialLongNoteTrackPosition = manager.GetPositionFromTime(Info.EndTime);
                UpdateLongNoteSize(InitialTrackPosition);
                InitialLongNoteSize = CurrentLongNoteSize;
            }

            // Update Positions
            UpdateSpritePositions(manager.CurrentTrackPosition);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            HitObjectSprite.Destroy();
            LongNoteBodySprite.Destroy();
            LongNoteEndSprite.Destroy();
        }

        /// <summary>
        ///     Calculates the position of the Hit Object with a position offset.
        /// </summary>
        /// <returns></returns>
        public float GetSpritePosition(long offset) => SpritePositionOffset + ((InitialTrackPosition - offset) * (ScrollDirection.Equals(ScrollDirection.Down) ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed) / HitObjectManagerKeys.TrackRounding);

        /// <summary>
        ///     Updates LN size
        /// </summary>
        /// <param name="offset"></param>
        public void UpdateLongNoteSize(long offset) => CurrentLongNoteSize = (InitialLongNoteTrackPosition - offset) * HitObjectManagerKeys.ScrollSpeed / HitObjectManagerKeys.TrackRounding;

        /// <summary>
        ///     Will forcibly update LN on scroll speed change or specific modifier.
        /// </summary>
        public void ForceUpdateLongnote(long offset)
        {
            if (offset < InitialTrackPosition)
            {
                UpdateLongNoteSize(InitialTrackPosition);
                InitialLongNoteSize = CurrentLongNoteSize;
            }

            UpdateSpritePositions(offset);
        }

        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        public void UpdateSpritePositions(long offset)
        {
            // Update Sprite position with regards to LN's state
            if (CurrentlyBeingHeld)
            {
                if (offset > InitialTrackPosition)
                {
                    UpdateLongNoteSize(offset);
                    SpritePosition = SpritePositionOffset;
                }
                else
                {
                    CurrentLongNoteSize = InitialLongNoteSize;
                    SpritePosition = GetSpritePosition(offset);
                }
            }
            else
            {
                SpritePosition = GetSpritePosition(offset);
            }

            // Update HitBody
            HitObjectSprite.Y = SpritePosition;

            // Disregard the rest if it isn't a long note.
            if (!Info.IsLongNote)
                return;

            // It will ignore the rest of the code after this statement if long note size is equal/less than 0
            if (CurrentLongNoteSize <= 0)
            {
                LongNoteBodySprite.Visible = false;
                LongNoteEndSprite.Visible = false;
                HitObjectSprite.Visible = false;
                return;
            }

            //Update HoldBody Position and Size
            LongNoteBodySprite.Height = CurrentLongNoteSize;

            if (ScrollDirection.Equals(ScrollDirection.Down))
            {
                LongNoteBodySprite.Y = +LongNoteBodyOffset + SpritePosition - CurrentLongNoteSize;
                LongNoteEndSprite.Y = SpritePosition - CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
            }
            else
            {
                LongNoteBodySprite.Y = SpritePosition + LongNoteBodyOffset;
                LongNoteEndSprite.Y = SpritePosition + CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
            }
        }

        /// <summary>
        ///     Gets the correct textures for the Hit Object.
        /// </summary>
        /// <returns></returns>
        private List<Texture2D> GetHitObjectFrames(int lane, GameMode mode)
        {
            lane = lane - 1;
            var skin = SkinManager.Skin.Keys[mode];

            if (skin.ColorObjectsBySnapDistance)
            {
                var snap = Math.Min(HitObjectManager.SnapIndices[Info], HitObjectManager.MaxNoteSnapIndex);
                return Info.IsLongNote ? skin.NoteHoldHitObjects[snap] : skin.NoteHitObjects[snap];
            }

            return Info.IsLongNote ? skin.NoteHoldHitObjects[lane] : skin.NoteHitObjects[lane];
        }

        /// <summary>
        ///     When the object iself dies, we want to change it to a dead color.
        /// </summary>
        public void Kill()
        {
            HitObjectSprite.Tint = Colors.DeadLongNote;
            if (Info.IsLongNote)
            {
                LongNoteBodySprite.Tint = Colors.DeadLongNote;
                LongNoteEndSprite.Tint = Colors.DeadLongNote;
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
            LongNoteBodySprite.StartLoop(Direction.Forward, 60);
            LongNoteEndSprite.StartLoop(Direction.Forward, 60);
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
    }
}
