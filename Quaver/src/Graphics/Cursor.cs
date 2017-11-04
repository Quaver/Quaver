using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Graphics;
using Quaver.Main;
using Quaver.Utility;

namespace Quaver.Graphics
{
    /// <summary>
    /// Todo: move class somewhere else?
    /// </summary>
    class Cursor : Sprite
    {
        public Cursor()
        {
            Size = new Vector2(40, 40);
            Image = GameBase.LoadedSkin.NoteHitObject1;
            Alignment = Alignment.TopLeft;
            Position = Vector2.Zero;
        }

        public override void Update(double dt)
        {
            Position = Util.PointToVector2(GameBase.MouseState.Position);
            PositionX -= 20;
            PositionY -= 20;

            UpdateRect();
            base.Update(dt);
            Draw();
        }
    }
}
