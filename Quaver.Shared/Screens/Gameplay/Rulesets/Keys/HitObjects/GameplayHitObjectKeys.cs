/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
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
        private GameplayRuleset Ruleset { get; }

        /// <summary>
        ///     Reference to the HitObjectManager controlling the object.
        /// </summary>
        private HitObjectManager HitObjectManager { get; }

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
        /// </summary>
        public long InitialLongNoteTrackPosition { get; private set; }

        /// <summary>
        ///     The Y position of the HitObject Sprites.
        /// </summary>
        private float SpritePosition { get; set; }

        /// <summary>
        ///     The initial size of this object's long note.
        /// </summary>
        public float InitialLongNoteSize { get; set; }

        /// <summary>
        ///     The current size of this object's long note.
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
        private Sprite HitObjectSprite { get; set; }

        /// <summary>
        ///     The hold body sprite for long notes.
        /// </summary>
        private AnimatableSprite LongNoteBodySprite { get; set; }

        /// <summary>
        ///     The hold end sprite for long notes.
        /// </summary>
        private Sprite LongNoteEndSprite { get; set; }

        /// <summary>
        ///     General Position for hitting. Calculated from Hit Body Height and Hit Position Offset
        /// </summary>
        private float HitPosition { get; set; }

        /// <summary>
        ///     Size Difference between HitObject and HoldHitOBject
        /// </summary>
        private float LongNoteSizeDifference { get; }

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
            var flipNoteEnd = direction.Equals(ScrollDirection.Up) && SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteEndImagesOnUpscroll;
            ScrollDirection = direction;

            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                SpriteEffect = flipNoteBody ? SpriteEffects.FlipVertically : SpriteEffects.None
            };

            // Create Hold Body
            var bodies = SkinManager.Skin.Keys[ruleset.Mode].NoteHoldBodies[lane];
            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(playfield.LaneSize, 0),
                Position = new ScalableVector2(posX, 0),
                Parent = playfield.Stage.HitObjectContainer
            };

            // Create the Hold End
            LongNoteEndSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                Size = new ScalableVector2(playfield.LaneSize, 0),
                Parent = playfield.Stage.HitObjectContainer,
                SpriteEffect = flipNoteEnd ? SpriteEffects.FlipVertically : SpriteEffects.None
            };

            // Set long note end properties.
            LongNoteEndSprite.Image = SkinManager.Skin.Keys[ruleset.Mode].NoteHoldEnds[lane];
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
            var playfield = (GameplayPlayfieldKeys)Ruleset.Playfield;
            HitPosition = info.IsLongNote ? playfield.HoldHitPositionY[info.Lane - 1] : playfield.HitPositionY[info.Lane - 1];
            Info = info;

            // Update Hit Object State
            HitObjectSprite.Image = GetHitObjectTexture(info.Lane, manager.Ruleset.Mode);
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
        public float GetSpritePosition(long offset) => HitPosition + ((InitialTrackPosition - offset) * (ScrollDirection.Equals(ScrollDirection.Down) ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed) / HitObjectManagerKeys.TrackRounding);

        /// <summary>
        ///     Updates LN size
        /// </summary>
        /// <param name="offset"></param>
        public void UpdateLongNoteSize(long offset) => CurrentLongNoteSize = (InitialLongNoteTrackPosition - offset) * HitObjectManagerKeys.ScrollSpeed / HitObjectManagerKeys.TrackRounding - LongNoteSizeDifference;

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
                    SpritePosition = HitPosition;
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

            if (skin.ColorObjectsBySnapDistance)
                return Info.IsLongNote ? skin.NoteHoldHitObjects[lane][HitObjectManager.SnapIndices[Info]] : skin.NoteHitObjects[lane][HitObjectManager.SnapIndices[Info]];

            return Info.IsLongNote ? skin.NoteHoldHitObjects[lane].First() : skin.NoteHitObjects[lane].First();
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
        public void StartLongNoteAnimation() => LongNoteBodySprite.StartLoop(Direction.Forward, 30);

        /// <summary>
        ///     Stops looping the long note sprite.
        ///     It will only be initiated when the player releases the note.
        /// </summary>
        public void StopLongNoteAnimation() => LongNoteBodySprite.StopLoop();
    }
}
