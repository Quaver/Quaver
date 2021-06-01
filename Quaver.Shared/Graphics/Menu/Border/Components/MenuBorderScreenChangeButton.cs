using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens;
using Wobble;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Graphics.Menu.Border.Components
{
    public abstract class MenuBorderScreenChangeButton : IconTextButton
    {
        public abstract QuaverScreenType Screen { get; }

        public MenuBorderScreenChangeButton(Texture2D icon, WobbleFontStore font, string text, EventHandler onClick = null,
            Color? baseColor = null, Color? hoveredColor = null) : base(icon, font, text, onClick, baseColor, hoveredColor)
        {
            // ReSharper disable once ArrangeConstructorOrDestructorBody
            Clicked += (o, e) =>
            {
                var game = (QuaverGame) GameBase.Game;

                if (game?.CurrentScreen?.Type == Screen)
                {
                    NotificationManager.Show(NotificationLevel.Warning, $"You are already on this screen!");
                    return;
                }

                OnClick();
            };
        }

        public abstract void OnClick();
    }
}