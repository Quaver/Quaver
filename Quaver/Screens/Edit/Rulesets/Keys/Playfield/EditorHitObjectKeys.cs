using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Audio;
using Quaver.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Edit.Rulesets.Keys.Playfield
{
    public class EditorHitObjectKeys
    {
        /// <summary>
        ///     The particular Qua HitObject this represents.
        /// </summary>
        public API.Maps.Structures.HitObjectInfo Info { get; }

        /// <summary>
        ///     The container for HitObjects
        /// </summary>
        private EditorScrollContainerKeys Container { get; }

        /// <summary>
        ///     The actual note HitObject
        /// </summary>
        public Sprite HitObjectSprite { get; private set; }

        /// <summary>
        ///     The LN Body Sprite.
        /// </summary>
        private AnimatableSprite LongNoteBodySprite { get; set; }

        /// <summary>
        ///     The LN end sprite.
        /// </summary>         
        private Sprite LongNoteEndSprite { get; set; }

        /// <summary>
        ///     The index of this object, used for any arrays.
        /// </summary>
        private int Index => Info.Lane - 1;

        /// <summary>
        ///     The x position of the object.
        /// </summary>
        private float PositionX => Container.Playfield.ColumnSize * Index;

        /// <summary>
        ///     The width of the HitObject
        /// </summary>
        private float Width => Container.Playfield.ColumnSize;

        /// <summary>
        ///     The offset from the receptor.
        /// </summary>
        public float OffsetYFromReceptor { get; set; }

        /// <summary>
        ///     The current y position of the object.
        /// </summary>
        public float PositionY { get; set; }

        /// <summary>
        ///     The y offset of the LN from the receptors.
        /// </summary>
        private float LongNoteOffsetYFromReceptor { get; set; }

        /// <summary>
        ///     The initial size of the long note.
        /// </summary>
        private float InitialLongNoteSize { get; set; }

        /// <summary>
        ///     The current size of the long note.
        /// </summary>
        private float CurrentLongNoteSize { get; set; }

        /// <summary>
        ///     The offset of the LN body.
        /// </summary>
        private float LongNoteBodyOffset { get; set; }

        /// <summary>
        ///     the offset of the LN end.
        /// </summary>
        private float LongNoteEndOffset { get; set; }

        /// <summary>
        ///     Keeps track of if we've already played hitsounds for this object.
        /// </summary>
        private bool HitsoundsPlayed { get; set; }

        /// <summary>
        ///     The index of the beat snap.
        /// </summary>
        private int SnapIndex { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="container"></param>
        /// <param name="info"></param>
        public EditorHitObjectKeys(EditorScrollContainerKeys container, API.Maps.Structures.HitObjectInfo info)
        {
            Container = container;
            Info = info;
            InitializeSprite();
        }

        /// <summary>
        ///     Creates the sprite for this HitObject.
        /// </summary>
        public void InitializeSprite()
        {
            OffsetYFromReceptor = Info.StartTime;
            PositionY = OffsetYFromReceptor;
            ;
            // Disregard non-long note objects after this point, so we can initailize them separately.
            if (Info.IsLongNote)
            {
                LongNoteOffsetYFromReceptor = Info.EndTime;
                InitialLongNoteSize = (ulong)((LongNoteOffsetYFromReceptor - OffsetYFromReceptor) * Container.Playfield.ScrollSpeed);
                CurrentLongNoteSize = InitialLongNoteSize;
            }

            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(PositionX, PositionY),
                Image = GetHitObjectTexture(),
            };

            // Var made for readability.
            var colSize = Container.Playfield.ColumnSize;
            HitObjectSprite.Size = new ScalableVector2(colSize, colSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);

            LongNoteBodyOffset = HitObjectSprite.Height / 2;

            if (Info.IsLongNote)
                CreateLongNote();

            // Set parent last so the LN's draw under the HitObjects.
            HitObjectSprite.Parent = Container;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateLongNote()
        {
            // Get the long note bodies to use.
            var bodies = SkinManager.Skin.Keys[Container.Playfield.Mode].NoteHoldBodies[Index];

            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Container.Playfield.ColumnSize, InitialLongNoteSize),
                Position = new ScalableVector2(PositionX, PositionY),
                Parent = Container
            };

            // Create the Hold End
            LongNoteEndSprite = new Sprite
            {
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(PositionX, PositionY),
                Size = new ScalableVector2(Container.Playfield.ColumnSize, 0),
                Parent = Container,
                Image = SkinManager.Skin.Keys[Container.Playfield.Mode].NoteHoldEnds[Index]
            };

            // Set long note end properties.
            LongNoteEndSprite.Height = Container.Playfield.ColumnSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.Height / 2f;
        }

        /// <summary>
        ///     Gets the HitObject to use based on the lane.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHitObjectTexture()
        {
            // Get Note Snapping
            if (SkinManager.Skin.Keys[Container.Playfield.Mode].ColorObjectsBySnapDistance)
                SnapIndex = GameplayHitObject.GetBeatSnap(Info, Container.Playfield.Screen.Map.GetTimingPointAt(Info.StartTime));
            else
                SnapIndex = 0;

            var skin = SkinManager.Skin.Keys[Container.Playfield.Mode];
            return Info.IsLongNote ? skin.NoteHoldHitObjects[Index][SnapIndex] : skin.NoteHitObjects[Index][SnapIndex];
        }

        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        public void UpdateSpritePositions()
        {
            // Update HitBody
            HitObjectSprite.Y = PositionY;

            // Disregard the rest if it isn't a long note.
            if (!Info.IsLongNote)
                return;

            // Don't handle long notes if the current size is non-existent.
            if (CurrentLongNoteSize <= 0)
            {
                LongNoteBodySprite.Visible = false;
                LongNoteEndSprite.Visible = false;
                return;
            }

            LongNoteBodySprite.Height = CurrentLongNoteSize;
            LongNoteBodySprite.Y = -CurrentLongNoteSize + LongNoteBodyOffset + PositionY;
            LongNoteEndSprite.Y = PositionY - CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
        }

        /// <summary>
        ///     Plays the object's hitsounds once it hits to the hit pos.
        /// </summary>
        public void PlayHitsoundsIfNecessary()
        {
            // For some reason, there's an audio delay when playing hitsounds. We have to account for this
            // by playing the sound slightly earlier.
            const int delay = 50;

            if (AudioEngine.Track.Time < Info.StartTime - delay)
                HitsoundsPlayed = false;

            if (!HitsoundsPlayed && AudioEngine.Track.Time >= Info.StartTime - delay && AudioEngine.Track.Time < Info.StartTime && AudioEngine.Track.IsPlaying)
                HitObjectManager.PlayObjectHitSounds(Info);

            if (AudioEngine.Track.Time >= Info.StartTime - delay)
                HitsoundsPlayed = true;
        }

        /// <summary>
        ///     Makes the HitObject completely invisible.
        /// </summary>
        public void MakeInvisible()
        {
            HitObjectSprite.Visible = false;

            if (!Info.IsLongNote)
                return;

            LongNoteBodySprite.Visible = false;
            LongNoteEndSprite.Visible = false;
        }

        /// <summary>
        ///     Makes the HitObject completely visible.
        /// </summary>
        public void MakeVisible()
        {
            HitObjectSprite.Visible = true;

            if (!Info.IsLongNote)
                return;

            LongNoteBodySprite.Visible = true;
            LongNoteEndSprite.Visible = true;
        }
    }
}
