﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;

namespace Quaver.GameState.Gameplay
{
    public class HitObject
    {
        /// <summary>
        /// Private field for HitObject Sprite Position
        /// </summary>
        private Vector2 _hitObjectPosition;

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

        /// <summary>
        ///     The index of which to snap the note
        ///     See: Skin.cs
        /// </summary>
        public int SnapIndex { get; set; } = 1;

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
        ///     The offset of the hold body from hit body.
        /// </summary>
        private float HoldBodyOffset { get; set; }

        /// <summary>
        ///     The offset of the hold end from hold body.
        /// </summary>
        private float HoldEndOffset { get; set; }

        /// <summary>
        ///     This method initializes the HitObject sprites
        /// </summary>
        internal void Initialize(bool downScroll, bool longNote, Drawable parent)
        {
            IsLongNote = longNote;
            var keyLaneIndex = KeyLane - 1;

            //Create hold body if this object is an LN
            if (longNote)
            {
                HoldBodySprite = new Sprite()
                {
                    Alignment = Alignment.TopLeft,
                    Size = new UDim2(HitObjectSize, InitialLongNoteSize),
                    Position = new UDim2(_hitObjectPosition.X, _hitObjectPosition.Y),
                    Parent = parent
                };

                // Choose the correct image based on the specific key lane.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        HoldBodySprite.Image = GameBase.LoadedSkin.NoteHoldBodies4K[keyLaneIndex];
                        break;
                    case GameModes.Keys7:
                        HoldBodySprite.Image = GameBase.LoadedSkin.NoteHoldBodies7K[keyLaneIndex];
                        break;
                    default:
                        break;
                }
            }

            //Create hit body
            HitBodySprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = new UDim2(_hitObjectPosition.X, _hitObjectPosition.Y),
                Parent = parent
            };

            // Choose the correct image based on the specific key lane.
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    for (var i = 0; i < 4; i++)
                    {
                        try
                        {
                            // If the user has ColourObjectsBySnapDistance enabled in their skin,
                            // we'll try to load give the object the correct snap colour,
                            // otherwise, we default it to the default or first (1/1) texture in the list.
                            if (GameBase.LoadedSkin.ColourObjectsBySnapDistance && GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][SnapIndex] != null)
                                HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][SnapIndex];
                            else
                                HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][0];
                        }
                        catch (Exception e)
                        {
                            HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][0];
                        }
                    }
                    break;
                case GameModes.Keys7:
                    for (var i = 0; i < GameBase.LoadedSkin.NoteHitObjects7K.Length; i++)
                        if (keyLaneIndex == i)
                            HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects7K[i];
                    break;
                default:
                    break;
            }

            // Scale hit object accordingly
            HitBodySprite.SizeX = HitObjectSize;
            HitBodySprite.SizeY = HitObjectSize * HitBodySprite.Image.Height / HitBodySprite.Image.Width;
            HoldBodyOffset = HitBodySprite.SizeY / 2;


            // Create hold body (placed ontop of hold body) if this is a long note.
            if (longNote)
            {
                HoldEndSprite = new Sprite()
                {
                    Alignment = Alignment.TopLeft,
                    Position = new UDim2(_hitObjectPosition.X, _hitObjectPosition.Y),
                    Size = new UDim2(HitObjectSize, 0),
                    Parent = parent,
                    SpriteEffect = downScroll ? SpriteEffects.FlipVertically : SpriteEffects.None
                };

                // Choose the correct image based on the specific key lane.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        HoldEndSprite.Image = GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex];
                        HoldEndSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex].Height / GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex].Width;
                        break;
                    case GameModes.Keys7:
                        HoldEndSprite.Image = GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex];
                        HoldEndSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex].Height / GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex].Width;
                        break;
                    default:
                        break;
                }

                //Update Hold End Size
                HoldEndOffset = HoldEndSprite.SizeY / 2;
            }
        }

        public void Update(bool downScroll)
        {
            // Only update note if it's inside the window
            if ((downScroll && _hitObjectPosition.Y + HitBodySprite.SizeY > 0) || (!downScroll && _hitObjectPosition.Y < GameBase.WindowRectangle.Height)) //todo: only update if object is inside boundary
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
                        HoldBodySprite.PosY = downScroll ? -(float)CurrentLongNoteSize + HoldBodyOffset + _hitObjectPosition.Y : _hitObjectPosition.Y + HoldBodyOffset;

                        //Update Hold End Position
                        HoldEndSprite.PosY = downScroll ? (_hitObjectPosition.Y - CurrentLongNoteSize - HoldEndOffset + HoldBodyOffset) : (_hitObjectPosition.Y + CurrentLongNoteSize - HoldEndOffset + HoldBodyOffset);
                    }
                }

                //Update HitBody
                HitBodySprite.PosY = _hitObjectPosition.Y;
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
                HoldBodySprite.Tint = Color.DimGray;
                HoldEndSprite.Tint = Color.DimGray;
            }
            HitBodySprite.Tint = Color.DimGray;
        }
    }
}
