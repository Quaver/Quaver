using System.Diagnostics;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics.Buttons;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.BottomBar
{
    internal class LogoButton : Button
    {
        internal LogoButton()
        {
            Image = UserInterface.QuaverLogoName;
            Size = new UDim2D(112, 25);
            PosX = SizeX / 2f + 20;
            PosY = -2;
            Clicked += (o, e) => { Process.Start("https://quavergame.com"); };
        }

        protected override void MouseOut()
        {
            Tint = Color.White;
        }

        protected override void MouseOver()
        {
            Tint = Colors.MainAccent;
        }
    }
}