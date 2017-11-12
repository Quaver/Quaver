using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.Utility;

namespace Quaver.Graphics.Sprite
{
    /// <summary>
    /// Todo: move class somewhere else?
    /// </summary>
    class Cursor : Sprite
    {
        //Mouse Click
        private bool MouseDown { get; set; }

        //Cursor Size
        private float CursorSize { get; set; } = 40;

        //Click Size
        private float ClickCurrentSize { get; set; }
        private float ClickTargetSize { get; set; }

        public Cursor()
        {
            Size = new Vector2(CursorSize, CursorSize);
            Image = GameBase.LoadedSkin.NoteHitObject1;
            Alignment = Alignment.TopLeft;
            Position = Vector2.Zero;
        }

        private void MouseClicked()
        {
            //Do stuff to cursor when mouse is clicked
        }

        public override void Update(double dt)
        {
            //Mouse Click
            if (GameBase.MouseState.LeftButton == ButtonState.Pressed)
            {
                if (!MouseDown)
                {
                    MouseDown = true;
                    MouseClicked();
                    ClickTargetSize = CursorSize / 3;
                    ClickCurrentSize = ClickTargetSize;
                }
            }
            else
            {
                MouseDown = false;
                ClickTargetSize = 0;
            }

            //Resize Cursor
            ClickCurrentSize = Util.Tween(ClickTargetSize, ClickCurrentSize, Math.Min(dt / 40, 1));
            SizeX = CursorSize + ClickCurrentSize;
            SizeY = SizeX;

            //Move Cursor
            Position = Util.PointToVector2(GameBase.MouseState.Position);
            PositionX -= (CursorSize + ClickCurrentSize) /2;
            PositionY -= (CursorSize + ClickCurrentSize) /2;

            base.Update(dt);
        }
    }
}
