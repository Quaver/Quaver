using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Graphics;
using Quaver.Graphics.Sprite;


namespace Quaver.Gameplay
{
    public class HitObject
    {
        /// <summary>
        /// Private field for HitObject Sprite Position
        /// </summary>
        private Vector2 _hitObjectPosition;

        /// <summary>
        /// The parent of the HitObjects
        /// </summary>
        internal Drawable ParentContainer { get; set; }

        /// <summary>
        /// The lane which the HitObject belongs to.
        /// </summary>
        public int KeyLane { get; set; }

        /// <summary>
        /// When the note is supposed to be pressed.
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// When the note is supposed to be held until. (If the HitObject is a long note)
        /// </summary>
        public float EndTime { get; set; }

        /// <summary>
        /// The HitObject's index that the object StartTime lies right after.
        /// </summary>
        public int SvIndex { get; set; }

        /// <summary>
        /// The Object's Y-Offset From the receptor.
        /// </summary>
        public ulong OffsetFromReceptor { get; set; }

        /// <summary>
        /// The LN End Y-Offset From the receptor.
        /// </summary>
        public ulong LnOffsetFromReceptor { get; set; }

        /// <summary>
        /// The initial size of this object's long note.
        /// </summary>
        public ulong InitialLongNoteSize { get; set; }

        /// <summary>
        /// The current size of this object's long note.
        /// </summary>
        public ulong CurrentLongNoteSize { get; set; }

        /// <summary>
        /// Determines if the HitObject is a long note or a regular note.
        /// </summary>
        public bool IsLongNote { get; set; }

        private Color deadColor { get; } = Color.Gray;

        /// <summary>
        /// The position of the HitObject Sprites
        /// </summary>
        public Vector2 HitObjectPosition
        {
            get => _hitObjectPosition;
            set
            {
                _hitObjectPosition = value;
            } 
        }

        /// <summary>
        /// The Y-position of the HitObject Sprites
        /// </summary>
        public float HitObjectPositionY
        {
            get => _hitObjectPosition.Y;
            set
            {
                _hitObjectPosition.Y = value;
            }
        }

        /// <summary>
        ///     The size of the hit object
        /// </summary>
        public float HitObjectSize { get; set; }

        /// <summary>
        ///     The main body of the hit object.
        /// </summary>
        private Sprite HitBodySprite { get; set; }

        /// <summary>
        ///     The hold body of the hit object.
        /// </summary>
        private Sprite HoldBodySprite { get; set; }

        /// <summary>
        ///     The cap/end of the hit object's LN.
        /// </summary>
        private Sprite HoldEndSprite { get; set; }

        /// <summary>
        ///     This method initializes the HitObject sprites
        /// </summary>
        public void Initialize(bool downScroll, bool longNote)
        {
            IsLongNote = longNote;

            if (longNote)
                HoldBodySprite = new Sprite()
                {
                    Image = GameBase.LoadedSkin.NoteHoldBody,
                    Alignment = Alignment.TopLeft,
                    Position = _hitObjectPosition,
                    Size = new Vector2(HitObjectSize, InitialLongNoteSize),
                    Parent = ParentContainer
                };

            HitBodySprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHitObject1,
                Alignment = Alignment.TopLeft,
                Position = _hitObjectPosition,
                Size = Vector2.One * HitObjectSize,
                Parent = ParentContainer
            };

            if (longNote) 
                HoldEndSprite = new Sprite()
                {
                    Image = GameBase.LoadedSkin.NoteHoldEnd,
                    Alignment = Alignment.TopLeft,
                    Position = _hitObjectPosition,
                    Size = Vector2.One * HitObjectSize,
                    Parent = ParentContainer,
                    SpriteEffect = downScroll ? SpriteEffects.FlipVertically : SpriteEffects.None
                };
        }

        public void Update(bool downScroll)
        {
            if (IsLongNote)
            {
                //Update HoldBody Position and Size
                HoldBodySprite.SizeY = CurrentLongNoteSize;
                HoldBodySprite.PositionY = downScroll ? -(float)CurrentLongNoteSize + HitObjectSize / 2f + _hitObjectPosition.Y : _hitObjectPosition.Y + HitObjectSize / 2f;

                //Update Hold End Position
                HoldEndSprite.PositionY = downScroll ? (_hitObjectPosition.Y - HoldBodySprite.SizeY) : _hitObjectPosition.Y + HoldBodySprite.SizeY;
            }

            //Update HitBody
            HitBodySprite.PositionY = _hitObjectPosition.Y;
        }

        /// <summary>
        ///     Destroy the hit object.
        /// </summary>
        public void Destroy()
        {
            if (IsLongNote)
            {
                HoldBodySprite.Destroy();
                HoldEndSprite.Destroy();
            }
            HitBodySprite.Destroy();
        }

        /// <summary>
        ///     This method will be called when the HitObject has to be killed (LN missed, LN early release)
        /// </summary>
        public void Kill()
        {
            if (IsLongNote)
            {
                HoldBodySprite.Tint = deadColor;
                HoldEndSprite.Tint = deadColor;
            }
            HitBodySprite.Tint = deadColor;
        }
    }
}
