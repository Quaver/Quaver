using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Screens.Result.UI
{
    public class ResultButtonContainer : Sprite
    {
        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public ResultButtonContainer()
        {
            Size = new ScalableVector2(WindowManager.Width - 56, 60);
            Tint = Color.Black;
            Alpha = 0.45f;

            AddBorder(Color.White, 2);
        }
    }
}