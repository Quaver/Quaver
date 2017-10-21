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
        //Parent Container
        private Drawable _ParentContainer;

        //Sprite Objects
        private Sprite _HitBodySprite;
        private Sprite _HoldBodySprite;
        private Sprite _HoldEndSprite;

        //Position Variables
        private Vector2 _Position;
        private float _HitObjectSize;

        public HitObject()
        {
            //TODO: add load images/stuff loogic later.
        }

        public void Initialize()
        {
            _HoldBodySprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHoldBody,
                Alignment = Alignment.TopLeft,
                Position = _Position,
                Size = Vector2.One * HitObjectSize,
                Parent = _ParentContainer
            };
            _HoldEndSprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHoldEnd,
                Alignment = Alignment.TopLeft,
                Position = _Position,
                Size = Vector2.One * HitObjectSize,
                Parent = _ParentContainer
            };

            _HitBodySprite = new Sprite()
            {
                Image = GameBase.LoadedSkin.NoteHitObject1,
                Alignment = Alignment.TopLeft,
                Position = _Position,
                Size = Vector2.One * HitObjectSize,
                Parent = _ParentContainer
            };
            UpdateObject();
        }

        /// <summary>
        /// The parent of the HitObjects
        /// </summary>
        internal Drawable ParentContainer
        {
            get
            {
                return _ParentContainer;
            }
            set
            {
                _ParentContainer = value;
            }
        }

        /// <summary>
        /// The lane which the HitObject belongs to.
        /// </summary>
        public int KeyLane
        {
            get; set;
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
        /// The _svPart's index that the object StartTime lies right after.
        /// </summary>
        public int SvPosition
        {
            get; set;
        }

        /// <summary>
        /// The Object's Y-Offset From the receptor.
        /// </summary>
        public ulong OffsetFromReceptor
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

        /// <summary>
        /// The position of the HitObject Sprites
        /// </summary>
        public Vector2 HitObjectPosition
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
            }
        }

        /// <summary>
        /// The Y Position of the HitObject Sprites
        /// </summary>
        public float HitObjectY
        {
            get
            {
                return _Position.Y;
            }
            set
            {
                _Position.Y = value;
            }
        }

        public float HitObjectSize
        {
            get
            {
                return _HitObjectSize;
            }
            set
            {
                _HitObjectSize = value;
            }
        }

        public void UpdateObject()
        {
            if (isLongNote)
            {
                _HoldBodySprite.PositionY = _Position.Y + HitObjectSize / 2f;
                _HoldEndSprite.PositionY = _Position.Y + HitObjectSize;

                _HoldBodySprite.UpdateRect();
                _HoldEndSprite.UpdateRect();
            }
            _HitBodySprite.PositionY = _Position.Y;
            _HitBodySprite.UpdateRect();
        }
        
        public void Draw()
        {
            if (isLongNote)
            {
                _HoldBodySprite.Draw();
                _HoldEndSprite.Draw();
            }

            _HitBodySprite.Draw();
        }

        public void Destroy()
        {
            _HoldBodySprite.Destroy();
            _HoldEndSprite.Destroy();
            _HitBodySprite.Destroy();
        }
    }
}
