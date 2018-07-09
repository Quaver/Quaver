using System;
using Microsoft.Xna.Framework.Input;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.UI
{
    /// <inheritdoc />
    /// <summary>
    /// 
    /// </summary>
    internal class Cursor : Sprites.Sprite
    {
        //Mouse Click
        private bool MouseDown { get; set; }

        //QuaverCursor Size
        private float CursorSize { get; set; } = 40;

        //Click Size
        private float ClickCurrentSize { get; set; }
        private float ClickTargetSize { get; set; }

        internal Cursor()
        {
            Size = new UDim2D(CursorSize, CursorSize);
            Image = GameBase.Skin.Cursor;
            Alignment = Alignment.TopLeft;
        }

        private void MouseClicked()
        {
            //Do stuff to cursor when mouse is clicked
        }

        internal override void Update(double dt)
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

            //Resize QuaverCursor
            ClickCurrentSize = GraphicsHelper.Tween(ClickTargetSize, ClickCurrentSize, Math.Min(dt / 40, 1));
            SizeX = CursorSize + ClickCurrentSize;
            SizeY = SizeX;

            //Move QuaverCursor
            //Position = GraphicsHelper.PointToVector2(GameBase.MouseState.Position);
            PosX = GameBase.MouseState.Position.X - (CursorSize + ClickCurrentSize) / 2;
            PosY = GameBase.MouseState.Position.Y - (CursorSize + ClickCurrentSize) / 2;

            base.Update(dt);
        }
    }
}
