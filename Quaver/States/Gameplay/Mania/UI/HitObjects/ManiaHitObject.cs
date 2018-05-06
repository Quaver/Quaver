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
        private QuaverSprite HitBodyQuaverSprite { get; set; }

        /// <summary>
        ///     The hold body of the hit object.
        /// </summary>
        private QuaverSprite HoldBodyQuaverSprite { get; set; }

        /// <summary>
        ///     The cap/end of the hit object's LN.
        /// </summary>
        private QuaverSprite HoldEndQuaverSprite { get; set; }

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
            HitBodyQuaverSprite = new QuaverSprite()
            {
                Alignment = Alignment.TopLeft,
                SpriteEffect = !Config.ConfigManager.DownScroll4K.Value && GameBase.LoadedSkin.FlipNoteImagesOnUpScroll4K ? SpriteEffects.FlipVertically : SpriteEffects.None,
                Position = new UDim2D(_hitObjectPosition.X, _hitObjectPosition.Y),
            };

            // Create hit/hold body (placed ontop of hold body) if this is a long note.
            if (longNote)
            {
                // Create Hold Body
                HoldBodyQuaverSprite = new QuaverSprite()
                {
                    Alignment = Alignment.TopLeft,
                    Size = new UDim2D(HitObjectSize, InitialLongNoteSize),
                    Position = new UDim2D(_hitObjectPosition.X, _hitObjectPosition.Y),
                    Parent = parent
                };

                // Create Hold End
                HoldEndQuaverSprite = new QuaverSprite()
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
                        HoldEndQuaverSprite.Image = GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex];
                        HoldBodyQuaverSprite.Image = GameBase.LoadedSkin.NoteHoldBodies4K[keyLaneIndex];
                        HoldEndQuaverSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex].Height / GameBase.LoadedSkin.NoteHoldEnds4K[keyLaneIndex].Width;
                        HoldEndOffset = HoldEndQuaverSprite.SizeY / 2;
                        break;
                    case GameMode.Keys7:
                        HoldEndQuaverSprite.Image = GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex];
                        HoldBodyQuaverSprite.Image = GameBase.LoadedSkin.NoteHoldBodies7K[keyLaneIndex];
                        HoldEndQuaverSprite.SizeY = HitObjectSize * GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex].Height / GameBase.LoadedSkin.NoteHoldEnds7K[keyLaneIndex].Width;
                        HoldEndOffset = HoldEndQuaverSprite.SizeY / 2;
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
                            HitBodyQuaverSprite.Image = (IsLongNote) ? GameBase.LoadedSkin.NoteHoldHitObjects4K[keyLaneIndex][SnapIndex]  
                                                                : GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][SnapIndex];
                        else
                            HitBodyQuaverSprite.Image = (IsLongNote) ? GameBase.LoadedSkin.NoteHoldHitObjects4K[keyLaneIndex][0] 
                                                                : GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][0];

                        // Update hit body's size to match image ratio
                        HitBodyQuaverSprite.Size = new UDim2D(HitObjectSize, HitObjectSize * HitBodyQuaverSprite.Image.Height / HitBodyQuaverSprite.Image.Width);
                        HitBodyQuaverSprite.Parent = parent;
                        HoldBodyOffset = HitBodyQuaverSprite.SizeY / 2;
                    }
                    catch (Exception e)
                    {
                        HitBodyQuaverSprite.Image = GameBase.LoadedSkin.NoteHitObjects4K[keyLaneIndex][0];
                        HitBodyQuaverSprite.Size = new UDim2D(HitObjectSize, HitObjectSize * HitBodyQuaverSprite.Image.Height / HitBodyQuaverSprite.Image.Width);
                        HitBodyQuaverSprite.Parent = parent;
                        HoldBodyOffset = HitBodyQuaverSprite.SizeY / 2;
                    }
                    break;
                case GameMode.Keys7:
                    HitBodyQuaverSprite.Image = GameBase.LoadedSkin.NoteHitObjects7K[keyLaneIndex][0];
                    HitBodyQuaverSprite.Size = new UDim2D(HitObjectSize, HitObjectSize * HitBodyQuaverSprite.Image.Height / HitBodyQuaverSprite.Image.Width);
                    HitBodyQuaverSprite.Parent = parent;
                    HoldBodyOffset = HitBodyQuaverSprite.SizeY / 2;
                    break;
                default:
                    break;
            }
        }

        public void Update(bool downScroll)
        {
            // Only update note if it's inside the window
            if ((downScroll && _hitObjectPosition.Y + HitBodyQuaverSprite.SizeY > 0) || (!downScroll && _hitObjectPosition.Y < GameBase.WindowRectangle.Height)) //todo: only update if object is inside boundary
            {
                // Update HitBody
                HitBodyQuaverSprite.PosY = _hitObjectPosition.Y;

                // Update Long Note Body/Cap
                if (IsLongNote)
                {
                    // It will ignore the rest of the code after this statement if long note size is equal/less than 0
                    if (CurrentLongNoteSize <= 0)
                    {
                        HoldBodyQuaverSprite.Visible = false;
                        HoldEndQuaverSprite.Visible = false;
                        return;
                    }

                    //Update HoldBody Position and Size
                    HoldBodyQuaverSprite.SizeY = CurrentLongNoteSize;
                    HoldBodyQuaverSprite.PosY = downScroll ? -(float)CurrentLongNoteSize + HoldBodyOffset + _hitObjectPosition.Y : _hitObjectPosition.Y + HoldBodyOffset;

                    //Update Hold End Position
                    HoldEndQuaverSprite.PosY = downScroll ? (_hitObjectPosition.Y - CurrentLongNoteSize - HoldEndOffset + HoldBodyOffset) : (_hitObjectPosition.Y + CurrentLongNoteSize - HoldEndOffset + HoldBodyOffset);
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
                HoldBodyQuaverSprite.Destroy();
                HoldEndQuaverSprite.Destroy();
            }
            HitBodyQuaverSprite.Destroy();
        }

        /// <summary>
        ///     This method will be called when the ManiaHitObject has to be killed (LN missed, LN early release)
        /// </summary>
        public void Kill()
        {
            if (IsLongNote)
            {
                HoldBodyQuaverSprite.Tint = QuaverColors.DeadNote;
                HoldEndQuaverSprite.Tint = QuaverColors.DeadNote;
            }
            HitBodyQuaverSprite.Tint = QuaverColors.DeadNote;
        }
    }
}
