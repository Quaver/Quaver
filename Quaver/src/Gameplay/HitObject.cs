﻿using System;
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

            //Create hold body if this object is an LN
            if (longNote)
                HoldBodySprite = new Sprite()
                {
                    Image = GameBase.LoadedSkin.NoteHoldBody,
                    Alignment = Alignment.TopLeft,
                    Position = _hitObjectPosition,
                    Size = new Vector2(HitObjectSize, InitialLongNoteSize),
                    Parent = ParentContainer
                };

            //Create hit body
            HitBodySprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = _hitObjectPosition,
                Size = Vector2.One * HitObjectSize,
                Parent = ParentContainer
            };

            // Choose the correct image based on the specific key lane.
            switch (GameBase.SelectedBeatmap.Qua.KeyCount)
            {
                case 4:
                    if (KeyLane == 1 || KeyLane == 4)
                        HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObject1;
                    else
                        HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObject2;
                    break;
                case 7:
                    if (KeyLane == 1 || KeyLane == 3 || KeyLane == 5 || KeyLane == 7)
                        HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObject1;
                    else if (KeyLane == 2 || KeyLane == 6)
                        HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObject2;
                    else if (KeyLane == 4)
                        HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObject3;
                    break;
                default:
                    break;
            }

            // Scale hit object accordingly
            HitBodySprite.SizeX = HitObjectSize;
            HitBodySprite.SizeY = HitObjectSize * (float)HitBodySprite.Image.Height / HitBodySprite.Image.Width;


            // Create hold body (placed ontop of hold body) if this is a long note.
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
            if (true) //todo: only update if object is inside boundary
            {
                if (IsLongNote)
                {
                    if (CurrentLongNoteSize <= 0)
                    {
                        HoldBodySprite.Visible = false;
                        HoldEndSprite.Visible = false;

                    }
                    else
                    {
                        //Update HoldBody Position and Size
                        HoldBodySprite.SizeY = CurrentLongNoteSize;
                        HoldBodySprite.PositionY = downScroll ? -(float)CurrentLongNoteSize + (HitObjectSize / 2f) + _hitObjectPosition.Y : _hitObjectPosition.Y + (HitObjectSize / 2f);

                        //Update Hold End Position
                        HoldEndSprite.PositionY = downScroll ? (_hitObjectPosition.Y - HoldBodySprite.SizeY) : (_hitObjectPosition.Y + HoldBodySprite.SizeY);
                    }
                }

                //Update HitBody
                HitBodySprite.PositionY = _hitObjectPosition.Y;
            }
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
