using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Graphics
{
    class BackgroundManager
    {
        public static Sprite Background;

        public static void Initialize()
        {
            Background = new Sprite()
            {
                SizeX = GameBase.Window.Width,
                SizeY = GameBase.Window.Height,
                Alignment = Alignment.MidCenter,
                Image = GameBase.UI.DiffSelectMask,
                Tint = Color.Gray
            };
        }

        public static void UnloadContent()
        {
            Background.Destroy();
        }

        public static void Update(double dt)
        {
            Background.Update(dt);
        }

        public static void Change(Texture2D newBG)
        {
            Background.Image = newBG;
        }

        public static void Draw()
        {
            Background.Draw();
        }

    }
}
