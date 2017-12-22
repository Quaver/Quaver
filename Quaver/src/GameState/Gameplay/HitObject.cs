using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Enums;
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

        /// <summary>
        /// Tint of object when it is killed
        /// </summary>
        private Color DeadColor { get; } = Color.Gray;

        /// <summary>
        /// Tint of object determined by it's beat snap
        /// </summary>
        public Color NoteColor { get; set; } = Color.White;

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
        public void Initialize(bool downScroll, bool longNote)
        {
            IsLongNote = longNote;

            //Create hold body if this object is an LN
            if (longNote)
            {
                HoldBodySprite = new Sprite()
                {
                    Alignment = Alignment.TopLeft,
                    Size = new Vector2(HitObjectSize, InitialLongNoteSize),
                    Position = _hitObjectPosition,
                    Parent = ParentContainer
                };

                // Choose the correct image based on the specific key lane.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        for (var i = 0; i < GameBase.LoadedSkin.NoteHoldBodies.Length; i++)
                            if (KeyLane - 1 == i)
                                HoldBodySprite.Image = GameBase.LoadedSkin.NoteHoldBodies[i];
                        break;
                    case GameModes.Keys7:
                        for (var i = 0; i < GameBase.LoadedSkin.NoteHoldBodies7K.Length; i++)
                            if (KeyLane - 1 == i)
                                HoldBodySprite.Image = GameBase.LoadedSkin.NoteHoldBodies7K[i];
                        break;
                    default:
                        break;
                }
            }

            //Create hit body
            HitBodySprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Position = _hitObjectPosition,
                Size = Vector2.One * HitObjectSize,
                Parent = ParentContainer
            };

            // Add the tint if the skin permits it.
            if (GameBase.LoadedSkin.ColourObjectsBySnapDistance)
                HitBodySprite.Tint = NoteColor;

            // Choose the correct image based on the specific key lane.
            switch (GameBase.SelectedBeatmap.Qua.Mode)
            {
                case GameModes.Keys4:
                    for (var i = 0; i < GameBase.LoadedSkin.NoteHitObjects.Length; i++)
                        if (KeyLane - 1 == i)
                            HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects[i];
                    break;
                case GameModes.Keys7:
                    for (var i = 0; i < GameBase.LoadedSkin.NoteHitObjects7K.Length; i++)
                        if (KeyLane - 1 == i)
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
                    Position = _hitObjectPosition,
                    SizeX = HitObjectSize,
                    Parent = ParentContainer,
                    SpriteEffect = downScroll ? SpriteEffects.FlipVertically : SpriteEffects.None
                };
                HoldEndOffset = HoldEndSprite.SizeY / 2;

                // Choose the correct image based on the specific key lane.
                switch (GameBase.SelectedBeatmap.Qua.Mode)
                {
                    case GameModes.Keys4:
                        for (var i = 0; i < GameBase.LoadedSkin.NoteHoldEnds.Length; i++)
                        {
                            if (KeyLane - 1 != i)
                                return;

                            HoldEndSprite.Image = GameBase.LoadedSkin.NoteHoldEnds[i];
                            HoldEndSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds[i].Height / GameBase.LoadedSkin.NoteHoldEnds[i].Width;
                        }
                        break;
                    case GameModes.Keys7:
                        for (var i = 0; i < GameBase.LoadedSkin.NoteHoldEnds7K.Length; i++)
                        {
                            if (KeyLane - 1 != i)
                                return;

                            HoldEndSprite.Image = GameBase.LoadedSkin.NoteHoldEnds7K[i];
                            HoldEndSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds7K[i].Height / GameBase.LoadedSkin.NoteHoldEnds7K[i].Width;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void Update(bool downScroll)
        {
            // Only update note if it's inside the window
            if ((downScroll && _hitObjectPosition.Y > GameBase.Window.Y) || (!downScroll && _hitObjectPosition.Y < GameBase.Window.Y + GameBase.Window.Z)) //todo: only update if object is inside boundary
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
                        HoldBodySprite.PositionY = downScroll ? -(float)CurrentLongNoteSize + HoldBodyOffset + _hitObjectPosition.Y : _hitObjectPosition.Y + HoldBodyOffset;

                        //Update Hold End Position
                        HoldEndSprite.PositionY = downScroll ? (_hitObjectPosition.Y - HoldBodySprite.SizeY - HoldEndOffset + HoldBodyOffset) : (_hitObjectPosition.Y + HoldBodySprite.SizeY + HoldEndOffset - HoldBodyOffset);
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
                HoldBodySprite.Tint = DeadColor;
                HoldEndSprite.Tint = DeadColor;
            }
            HitBodySprite.Tint = DeadColor;
        }
    }
}
