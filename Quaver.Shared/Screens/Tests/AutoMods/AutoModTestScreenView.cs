using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.UI.AutoMods;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.AutoMods
{
    public class AutoModTestScreenView : ScreenView
    {
        public AutoModTestScreenView(Screen screen) : base(screen)
        {
            var mapset = new List<Qua> {  };

            _ = new EditorAutoModPanel(mapset.First(), mapset, Container as EditorAutoModPanelContainer)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                X = 100
            };
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#292d3e"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}