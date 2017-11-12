using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                SizeX = GameBase.Window.Size.X,
                SizeY = GameBase.Window.Size.Y,
                Alignment = Alignment.MidCenter,
                Image = GameBase.LoadedSkin.NoteHitObject1
            };
        }

        public static void UnloadContent()
        {
            
        }

        public static void Update(double dt)
        {
            
        }

        public static void Change(Texture2D newBG)
        {
            Background.Image = newBG;
        }

        public static void Draw()
        {
            Console.WriteLine(Background.GlobalRect);
            Background.Draw();
        }

    }
}
