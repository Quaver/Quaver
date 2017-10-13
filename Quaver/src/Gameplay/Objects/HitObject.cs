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
        //Sprite Objects
        internal Sprite _HitBodySprite;
        internal Sprite _HoldBodySprite;
        internal Sprite _HoldEndSprite;

        public HitObject() //Remove ContentManager Later
        {
            _HitBodySprite = new Sprite()
            {
                Image = GameBase.Content.Load<Texture2D>("note_hitObject"),
                Size = Vector2.One * 50,
                Alignment = Alignment.MidCenter
            };

            _HoldBodySprite = new Sprite()
            {
                Image = GameBase.Content.Load<Texture2D>("note_holdBody"),
                Size = Vector2.One * 50,
                Position = new Vector2(0, 12.5f),
                Alignment = Alignment.MidCenter
            };

            _HoldEndSprite = new Sprite()
            {
                Image = GameBase.Content.Load<Texture2D>("note_holdEnd"),
                Size = Vector2.One * 50,
                Position = new Vector2(0, 24),
                Alignment = Alignment.MidCenter
            };
        }

        /// <summary>
        /// When the note is supposed to be pressed.
        /// </summary>
        public float StartTime
        {
            get; set;
        }

        /// <summary>
        /// When the note is supposed to be held until. (If the HitObject is a long note)
        /// </summary>
        public float EndTime
        {
            get; set;
        }

        /// <summary>
        /// Determines if the HitObject is a long note or a regular note.
        /// </summary>
        public bool isLongNote
        {
            get; set;
        } = true;
        
        public void Draw()
        {
            //If the object is a long note
            if (isLongNote)
            {
                _HoldBodySprite.Draw();
                _HoldEndSprite.Draw();
            }

            _HitBodySprite.Draw();
        }
    }
}
