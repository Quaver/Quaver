using Microsoft.Xna.Framework;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Database.Maps;
using Quaver.Graphics.Notifications;
using Quaver.Modifiers;
using Quaver.Online;
using Quaver.Scheduling;
using Quaver.Screens.Gameplay;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Helpers;
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