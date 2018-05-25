using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Graphics.Base;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.States.Gameplay.Mania.UI.HitObjects
{
    public class ManiaHitObject
    {
        /// <summary>
        /// Private field for ManiaHitObject QuaverSprite Position
        /// </summary>
        private Vector2 _hitObjectPosition;

        /// <summary>
        /// The lane which the ManiaHitObject belongs to.
        /// </summary>
        public int KeyLane { get; set; }

        /// <summary>
        /// When the note is supposed to be pressed.
        /// </summary>
        public float StartTime { get; set; }

        /// <summary>
        /// When the note is supposed to be held until. (If the ManiaHitObject is a long note)
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
        /// Determines if the ManiaHitObject is a long note or a regular note.
        /// </summary>
        public bool IsLongNote { get; set; }

        /// <summary>
        ///     The index of which to snap the note
        ///     See: Skin.cs
        /// </summary>
        public int SnapIndex { get; set; } = 1;

        /// <summary>
        ///     The HitSounds this object should play
        /// </summary>
        public HitSounds HitSounds { get; set; } 

        /// <summary>
        /// The position of the ManiaHitObject Sprites
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
        /// The Y-position of the ManiaHitObject Sprites
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
        ///     This method initializes the ManiaHitObject sprites
        /// </summary>
        internal void Initialize(bool downScroll, bool longNote, Drawable parent)
        {
            IsLongNote = longNote;
            var keyLaneIndex = KeyLane - 1;

            //Create hit body
            HitBodySprite = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                SpriteEffect = !Config.ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                Position = new UDim2D(_hitObjectPosition.X, _hitObjectPosition.Y),
            };

            // Create hit/hold body (placed ontop of hold body) if this is a long note.
            if (longNote)
            {
                // Create Hold Body
                HoldBodySprite = new Sprite()
                {
                    Alignment = Alignment.TopLeft,
                    Size = new UDim2D(HitObjectSize, InitialLongNoteSize),
                    Position = new UDim2D(_hitObjectPosition.X, _hitObjectPosition.Y),
                    Parent = parent
                };

                // Create Hold End
                HoldEndSprite = new Sprite()
                {
                    Alignment = Alignment.TopLeft,
                    Position = new UDim2D(_hitObjectPosition.X, _hitObjectPosition.Y),
                    Size = new UDim2D(HitObjectSize, 0),
                    Parent = parent,
                    SpriteEffect = !Config.ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None
                };

                // Choose the correct image based on the specific key lane for hold bodies.
                switch (GameBase.SelectedMap.Qua.Mode)
                {
                    case GameMode.Keys4:
                        HoldEndSprite.Image = GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex];
                        HoldBodySprite.Image = GameBase.LoadedSkin.NoteHoldBodies4K[keyLaneIndex][0];
                        HoldEndSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex].Height / GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex].Width;
                        HoldEndOffset = HoldEndSprite.SizeY / 2;
                        break;
                    case GameMode.Keys7:
                        HoldEndSprite.Image = GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex];
                        HoldBodySprite.Image = GameBase.LoadedSkin.NoteHoldBodies7K[keyLaneIndex];
                        HoldEndSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex].Height / GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex].Width;
                        HoldEndOffset = HoldEndSprite.SizeY / 2;
                        break;
                    default:
                        break;
                }
            }

            // Choose the correct image based on the specific key lane for hit body.
            switch (GameBase.SelectedMap.Qua.Mode)
            {
                case GameMode.Keys4:
                    try
                    {
                        // If the user has ColourObjectsBySnapDistance enabled in their skin,
                        // we'll try to load give the object the correct snap colour,
                        // otherwise, we default it to the default or first (1/1) texture in the list.
                        if (GameBase.LoadedSkin.ColourObjectsBySnapDistance && GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][SnapIndex] != null)
                            HitBodySprite.Image = (IsLongNote) ? GameBase.LoadedSkin.NoteHoldHitObjects4K[keyLaneIndex][SnapIndex]  
                                                                : GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][SnapIndex];
                        else
                            HitBodySprite.Image = (IsLongNote) ? GameBase.LoadedSkin.NoteHoldHitObjects4K[keyLaneIndex][0] 
                                                                : GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][0];

                        // Update hit body's size to match image ratio
                        HitBodySprite.Size = new UDim2D(HitObjectSize, HitObjectSize * HitBodySprite.Image.Height / HitBodySprite.Image.Width);
                        HitBodySprite.Parent = parent;
                        HoldBodyOffset = HitBodySprite.SizeY / 2;
                    }
                    catch (Exception e)
                    {
                        HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][0];
                        HitBodySprite.Size = new UDim2D(HitObjectSize, HitObjectSize * HitBodySprite.Image.Height / HitBodySprite.Image.Width);
                        HitBodySprite.Parent = parent;
                        HoldBodyOffset = HitBodySprite.SizeY / 2;
                    }
                    break;
                case GameMode.Keys7:
                    HitBodySprite.Image = GameBase.LoadedSkin.NoteHitObjects7K[keyLaneIndex][0];
                    HitBodySprite.Size = new UDim2D(HitObjectSize, HitObjectSize * HitBodySprite.Image.Height / HitBodySprite.Image.Width);
                    HitBodySprite.Parent = parent;
                    HoldBodyOffset = HitBodySprite.SizeY / 2;
                    break;
                default:
                    break;
            }
        }

        public void Update(bool downScroll)
        {
            // Only update note if it's inside the window
            if ((downScroll && _hitObjectPosition.Y + HitBodySprite.SizeY > 0) || (!downScroll && _hitObjectPosition.Y < GameBase.WindowRectangle.Height)) //todo: only update if object is inside boundary
            {
                // Update HitBody
                HitBodySprite.PosY = _hitObjectPosition.Y;

                // Update Long Note Body/Cap
                if (IsLongNote)
                {
                    // It will ignore the rest of the code after this statement if long note size is equal/less than 0
                    if (CurrentLongNoteSize <= 0)
                    {
                        HoldBodySprite.Visible = false;
                        HoldEndSprite.Visible = false;
                        return;
                    }

                    //Update HoldBody Position and Size
                    HoldBodySprite.SizeY = CurrentLongNoteSize;
                    HoldBodySprite.PosY = downScroll ? -(float)CurrentLongNoteSize + HoldBodyOffset + _hitObjectPosition.Y : _hitObjectPosition.Y + HoldBodyOffset;

                    //Update Hold End Position
                    HoldEndSprite.PosY = downScroll ? (_hitObjectPosition.Y - CurrentLongNoteSize - HoldEndOffset + HoldBodyOffset) : (_hitObjectPosition.Y + CurrentLongNoteSize - HoldEndOffset + HoldBodyOffset);
                }
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
        ///     This method will be called when the ManiaHitObject has to be killed (LN missed, LN early release)
        /// </summary>
        public void Kill()
        {
            if (IsLongNote)
            {
                HoldBodySprite.Tint = QuaverColors.DeadNote;
                HoldEndSprite.Tint = QuaverColors.DeadNote;
            }
            HitBodySprite.Tint = QuaverColors.DeadNote;
        }
    }
}
