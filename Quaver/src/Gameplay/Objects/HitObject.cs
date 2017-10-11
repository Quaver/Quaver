using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Quaver.Graphics;

namespace Quaver.Gameplay
{
    public class HitObject
    {
        //Sprite Objects
        internal Sprite _HitBodySprite;
        internal Sprite _HoldBodySprite;
        internal Sprite _HoldEndSprite;

        public HitObject(GraphicsDevice graphicsDevice, ContentManager content) //Remove ContentManager Later
        {
            _HitBodySprite = new Sprite(graphicsDevice);
            _HitBodySprite.Image = content.Load<Texture2D>("TestImages/note_hitObject");
            _HitBodySprite.Size = Vector2.One * 50;
            _HitBodySprite.Alignment = Alignment.MidCenter;

            _HoldBodySprite = new Sprite(graphicsDevice);
            _HoldBodySprite.Image = content.Load<Texture2D>("TestImages/note_holdBody");
            _HoldBodySprite.Size = Vector2.One * 50;
            _HoldBodySprite.Alignment = Alignment.MidCenter;

            _HoldEndSprite = new Sprite(graphicsDevice);
            _HoldEndSprite.Image = content.Load<Texture2D>("TestImages/note_holdEnd");
            _HoldEndSprite.Size = Vector2.One * 50;
            _HoldEndSprite.Alignment = Alignment.MidCenter;
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
        
        public void Draw(SpriteBatch spriteBatch)
        {
            //If the object is a long note
            if (isLongNote)
            {
                _HoldBodySprite.Draw(spriteBatch);
                _HoldEndSprite.Draw(spriteBatch);
            }

            _HitBodySprite.Draw(spriteBatch);
        }
    }
}
