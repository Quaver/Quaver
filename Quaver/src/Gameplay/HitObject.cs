using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Gameplay
{
    public class HitObject
    {
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
        /// The position of the HitObject Sprites
        /// </summary>
        public Vector2 HitObjectPosition { get; set; } = Vector2.Zero;

        /// <summary>
        ///     The size of the hit object
        /// </summary>
        public float HitObjectSize { get; set; }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        private Sprite HitBodySprite { get; set; }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        private Sprite HoldBodySprite { get; set; }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        private Sprite HoldEndSprite { get; set; }

        /// <summary>
        ///     TODO: Initializes the HitObject sprite
        /// </summary>
        public void Initialize()
        {
            HoldBodySprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.TopLeft,
                Position = HitObjectPosition,
                Size = new Vector2(HitObjectSize, InitialLongNoteSize),
                Parent = ParentContainer
            };

            HoldEndSprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHoldEnd,
                Alignment = Alignment.TopLeft,
                Position = HitObjectPosition,
                Size = Vector2.One * HitObjectSize,
                Parent = ParentContainer
            };

            HitBodySprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHitObject1,
                Alignment = Alignment.TopLeft,
                Position = HitObjectPosition,
                Size = Vector2.One * HitObjectSize,
                Parent = ParentContainer
            };

            UpdateObject();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void UpdateObject()
        {
            if (IsLongNote)
            {
                //Update HoldBody Position and Size
                HoldBodySprite.PositionY = HitObjectPosition.Y + HitObjectSize / 2f;
                HoldBodySprite.SizeY = CurrentLongNoteSize;

                //Update Hold End Position
                HoldEndSprite.PositionY = HitObjectPosition.Y + HoldBodySprite.SizeY;

                //Update Hold Rects
                HoldBodySprite.UpdateRect();
                HoldEndSprite.UpdateRect();
            }

            //Update HitBody
            HitBodySprite.PositionY = HitObjectPosition.Y;
            HitBodySprite.UpdateRect();
        }
        
        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Draw()
        {
            if (IsLongNote)
            {
                HoldBodySprite.Draw();
                HoldEndSprite.Draw();
            }

            HitBodySprite.Draw();
        }

        /// <summary>
        ///     TODO: Add Summary
        /// </summary>
        public void Destroy()
        {
            HoldBodySprite.Destroy();
            HoldEndSprite.Destroy();
            HitBodySprite.Destroy();
        }

        /// <summary>
        /// This method will be called when the HitObject has to be killed (LN missed, LN early release)
        /// </summary>
        public void Kill()
        {
            //Console.WriteLine("KILLED");

            Color newTint = Color.Gray;
            HoldBodySprite.Tint = newTint;
            HoldEndSprite.Tint = newTint;
            HitBodySprite.Tint = newTint;
        }
    }
}
