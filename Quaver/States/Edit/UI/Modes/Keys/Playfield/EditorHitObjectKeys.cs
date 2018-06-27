using System;
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
        /// 
        /// </summary>
        public Sprite HitObjectSprite { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private AnimatableSprite LongNoteBodySprite { get; set; }
        
        /// <summary>
        /// 
        /// </summary>         
        private Sprite LongNoteEndSprite { get; set; }
        
        /// <summary>
        ///     The width of the object.
        /// </summary>
        private float Width => Container.Playfield.ColumnSize;

        /// <summary>
        ///     
        /// </summary>
        internal float OffsetYFromReceptor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal float PositionY { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private float LongNoteOffsetYFromReceptor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private float InitialLongNoteSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private float CurrentLongNoteSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private float LongNoteBodyOffset { get; set; }
        
        /// <summary>
        ///     
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
                InitialLongNoteSize = (ulong)((LongNoteOffsetYFromReceptor - OffsetYFromReceptor) * 22 / (20f * GameBase.AudioEngine.PlaybackRate));
                CurrentLongNoteSize = InitialLongNoteSize;
            }
            
            // Create the base HitObjectSprite
            HitObjectSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(Container.Playfield.ColumnSize * (Info.Lane - 1), PositionY),
                Image = GetHitObjectTexture(), 
                Parent = Container
            };    
            
            HitObjectSprite.Size = new UDim2D(Container.Playfield.ColumnSize, Container.Playfield.ColumnSize * HitObjectSprite.Image.Height / HitObjectSprite.Image.Width);          
            LongNoteBodyOffset = HitObjectSprite.SizeY / 2;

            if (Info.IsLongNote)
                CreateLongNote();
        }

        private void CreateLongNote()
        {
            // Get the long note bodies to use.
            var bodies = GameBase.Skin.Keys[Container.Playfield.Mode].NoteHoldBodies[Info.Lane - 1];

            var sizeX = Container.Playfield.ColumnSize;
            var positionX = Container.Playfield.ColumnSize * (Info.Lane - 1);
            
            LongNoteBodySprite = new AnimatableSprite(bodies)
            {
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(sizeX, InitialLongNoteSize),
                Position = new UDim2D(positionX, PositionY),
                Parent = Container
            };
                                  
            // Create the Hold End
            LongNoteEndSprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new UDim2D(positionX, PositionY),
                Size = new UDim2D(sizeX),
                Parent = Container,
            };
            
            // Set long note end properties.
            LongNoteEndSprite.Image = GameBase.Skin.Keys[Container.Playfield.Mode].NoteHoldEnds[Info.Lane - 1];       
            LongNoteEndSprite.SizeY =  sizeX * LongNoteEndSprite.Image.Height / LongNoteEndSprite.Image.Width;
            LongNoteEndOffset = LongNoteEndSprite.SizeY / 2f;    
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        private Texture2D GetHitObjectTexture()
        {
            var skin = GameBase.Skin.Keys[Container.Playfield.Mode];
            return Info.IsLongNote ? skin.NoteHoldHitObjects[Info.Lane - 1][0] : skin.NoteHitObjects[Info.Lane - 1][0];
        }
        
        /// <summary>
        ///     Calculates the position from the offset.
        /// </summary>
        /// <returns></returns>
        internal float GetPosFromOffset(float offset)
        {
            return (float)(Container.Playfield.HitPositionY - (offset - (GameBase.AudioEngine.Time)) * 22 / (20 * GameBase.AudioEngine.PlaybackRate));
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
            
            // It will ignore the rest of the code after this statement if long note size is equal/less than 0
            if (CurrentLongNoteSize <= 0)
            {
                LongNoteBodySprite.Visible = false;
                LongNoteEndSprite.Visible = false;
                return;
            }

            //Update HoldBody Position and Size
            LongNoteBodySprite.SizeY = CurrentLongNoteSize;

            LongNoteBodySprite.PosY = -CurrentLongNoteSize + LongNoteBodyOffset + PositionY;
            LongNoteEndSprite.PosY = PositionY - CurrentLongNoteSize - LongNoteEndOffset + LongNoteBodyOffset;
        }
    }
}