using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Menu.UI.BottomToolbar
{
    public class LogoButton : Button
    {
        public LogoButton()
        {
            Image = UserInterface.QuaverLogoName;
            Size = new ScalableVector2(112, 25);
            X = Width / 2f + 20;
            Y = -2;
            Clicked += (o, e) => { Process.Start("https://quavergame.com"); };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Tint = IsHovered ? Colors.MainAccent : Color.White;

            base.Update(gameTime);
        }
    }
}
