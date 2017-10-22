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
        /// The _svPart's index that the object StartTime lies right after.
        /// </summary>
        public int SvPosition { get; set; }

        /// <summary>
        /// The Object's Y-Offset From the receptor.
        /// </summary>
        public ulong OffsetFromReceptor { get; set; }

        /// <summary>
        /// Determines if the HitObject is a long note or a regular note.
        /// </summary>
        public bool IsLongNote { get; set; } = true;

        /// <summary>
        /// The position of the HitObject Sprites
        /// </summary>
        public Vector2 HitObjectPosition { get; set; }

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
                Size = Vector2.One * HitObjectSize,
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
                HoldBodySprite.PositionY = HitObjectPosition.Y + HitObjectSize / 2f;
                HoldEndSprite.PositionY = HitObjectPosition.Y + HitObjectSize;

                HoldBodySprite.UpdateRect();
                HoldEndSprite.UpdateRect();
            }

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
    }
}
