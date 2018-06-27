using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Modes.Keys.Playfield
{
    internal class EditorHitObjectKeys
    {
        /// <summary>
        ///     The particular Qua HitObject this represents.
        /// </summary>
        internal HitObjectInfo Info { get; }

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
        internal float OffsetYFromReceptor { get; set; }

        /// <summary>
        ///     The current y position of the object.
        /// </summary>
        internal float PositionY { get; set; }

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
        ///     Ctor -
        /// </summary>
        /// <param name="container"></param>
        /// <param name="info"></param>
        internal EditorHitObjectKeys(EditorScrollContainerKeys container, HitObjectInfo info)
        {
            Container = container;
            Info = info;
            InitializeSprite();
        }

        /// <summary>
        ///     Creates the sprite for this HitObject.
        /// </summary>
        internal void InitializeSprite()
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
                Position = new UDim2D(PositionX, PositionY),
                Image = GetHitObjectTexture(), 
                Parent = Container
            };

            // Var made for readability.
            var colSize = Container.Playfield.ColumnSize;
            HitObjectSprite.Size = new UDim2D(colSize, colSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);    
            
            LongNoteBodyOffset = HitObjectSprite.SizeY / 2;

            if (Info.IsLongNote)
                CreateLongNote();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateLongNote()
        {
            // Get the long note bodies to use.
            var bodies = GameBase.Skin.Keys[Container.Playfield.Mode].NoteHoldBodies[Index];

            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(Container.Playfield.ColumnSize, InitialLongNoteSize),
                Position = new UDim2D(PositionX, PositionY),
                Parent = Container
            };
                                  
            // Create the Hold End
            LongNoteEndSprite = new Sprite
            {
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(PositionX, PositionY),
                Size = new UDim2D(Container.Playfield.ColumnSize),
                Parent = Container,
                Image = GameBase.Skin.Keys[Container.Playfield.Mode].NoteHoldEnds[Index]
            };

            // Set long note end properties.
            LongNoteEndSprite.SizeY =  Container.Playfield.ColumnSize * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.SizeY / 2f;    
        }

        /// <summary>
        ///     Gets the HitObject to use based on the lane.
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHitObjectTexture()
        {
            var skin = GameBase.Skin.Keys[Container.Playfield.Mode];
            return Info.IsLongNote ? skin.NoteHoldHitObjects[Index].First() : skin.NoteHitObjects[Index].First();
        }
        
        /// <summary>
        ///     Calculates the position from the offset.
        /// </summary>
        /// <returns></returns>
        internal float GetPosFromOffset(float offset)
        {
            return (float)(Container.Playfield.HitPositionY - (offset - GameBase.AudioEngine.Time) * Container.Playfield.ScrollSpeed);
        }

        /// <summary>
        ///     Updates the HitObject sprite positions
        /// </summary>
        internal void UpdateSpritePositions()
        {                       
            // Update HitBody
            HitObjectSprite.PosY = PositionY;
            
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

            LongNoteBodySprite.SizeY = CurrentLongNoteSize;
            LongNoteBodySprite.PosY = -CurrentLongNoteSize + LongNoteBodyOffset + PositionY;
            LongNoteEndSprite.PosY = PositionY - CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
        }
    }
}