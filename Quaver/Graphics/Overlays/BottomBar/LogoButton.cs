using System.Diagnostics;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Colors;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.BottomBar
{
    internal class LogoButton : Button
    {
        internal LogoButton()
        {
            Image = GameBase.QuaverUserInterface.QuaverLogoName;
            Size = new UDim2D(135, 30);
            PosX = SizeX / 2f + 20;
            PosY = -2;
            Clicked += (o, e) => { Process.Start("https://quavergame.com"); };
        }
        
        protected override void MouseOut()
        {
            Size = new UDim2D(135, 30);
            Tint = Color.White;
        }

        protected override void MouseOver()
        {
            Tint = QuaverColors.MainAccent;
        }
    }
}