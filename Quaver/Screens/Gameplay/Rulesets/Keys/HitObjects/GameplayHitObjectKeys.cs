using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Maps.Structures;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    public class GameplayHitObjectKeys : GameplayHitObject
    {
        /// <summary>
        ///     Is determined by whether the player is holding the key that this hit object is binded to
        /// </summary>
        public bool CurrentlyBeingHeld { get; set; }

        /// <summary>
        ///     If the note is a long note.
        ///     In .qua format, long notes are defined as if the end time is greater than 0.
        /// </summary>
        public bool IsLongNote { get; private set; }

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
        ///     The SpriteEffects. Flips the image horizontally if we are using upscroll.
        /// </summary>
        private static SpriteEffects Effects => !ConfigManager.DownScroll4K.Value &&
                                                SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].FlipNoteImagesOnUpscroll
            ? SpriteEffects.FlipVertically
            : SpriteEffects.None;

        /// <summary>
        ///     General Distance from the receptor. Calculated from hit body size and global offset
        /// </summary>
        public static float HitPositionOffset { get; set; } = 0;

        /// <summary>
        ///     Distance from the receptor for the current HitObjectSprite's image
        /// </summary>
        public float SpritePositionOffset => HitPositionOffset - HitObjectSprite.Height;

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="ruleset"></param>
        /// <param name="info"></param>
        public GameplayHitObjectKeys(HitObjectInfo info, GameplayRulesetKeys ruleset, HitObjectManagerKeys manager)
        {
            InitializeSprites(ruleset, info.Lane - 1);
            InitializeObject(manager, info);
        }

        /// <summary>
        ///     Initialize HitObject Sprite used for Object Pooling. Only gets initialized once upon object creation.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ruleset"></param>
        private void InitializeSprites(GameplayRulesetKeys ruleset, int lane)
        {
            // Reference variables
            var playfield = (GameplayPlayfieldKeys)ruleset.Playfield;
            var posX = playfield.Stage.Receptors[lane].X;

            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(posX, 0),
                SpriteEffect = Effects,
                Image = GetHitObjectTexture(lane, ruleset.Mode)
            };

            // Update hit body's size to match image ratio
            HitObjectSprite.Size = new ScalableVector2(playfield.LaneSize, playfield.LaneSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);
            LongNoteBodyOffset = HitObjectSprite.Height / 2;

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
                SpriteEffect = Effects
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
            Info = info;

            // Update Hit Object State
            HitObjectSprite.Visible = true;
            HitObjectSprite.Tint = Color.White;
            IsLongNote = Info.EndTime > 0;
            InitialTrackPosition = manager.GetPositionFromTime(Info.StartTime);
            CurrentlyBeingHeld = false;
            StopLongNoteAnimation();

            // Update Hit Object State depending if its an LN or not
            if (!IsLongNote)
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
        public float GetSpritePosition(long offset) => SpritePositionOffset + ((InitialTrackPosition - offset) * (GameplayRulesetKeys.IsDownscroll ? -HitObjectManagerKeys.ScrollSpeed : HitObjectManagerKeys.ScrollSpeed) / HitObjectManagerKeys.TrackRounding); 

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
            if (!IsLongNote)
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

            if (GameplayRulesetKeys.IsDownscroll)
            {
                LongNoteBodySprite.Y = + LongNoteBodyOffset + SpritePosition - CurrentLongNoteSize;
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
            var skin = SkinManager.Skin.Keys[mode];

            if (skin.ColorObjectsBySnapDistance)
                return IsLongNote ? skin.NoteHoldHitObjects[lane][SnapIndex] : skin.NoteHitObjects[lane][SnapIndex];

            return IsLongNote ? skin.NoteHoldHitObjects[lane].First() : skin.NoteHitObjects[lane].First();
        }

        /// <summary>
        ///     When the object iself dies, we want to change it to a dead color.
        /// </summary>
        public void Kill()
        {
            HitObjectSprite.Tint = Colors.DeadLongNote;
            if (IsLongNote)
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

            if (!IsLongNote)
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
