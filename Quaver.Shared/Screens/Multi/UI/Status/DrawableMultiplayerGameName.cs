using System;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Status
{
    public class DrawableMultiplayerGameName : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        public DrawableMultiplayerGameName(Bindable<MultiplayerGame> game)
        {
            Game = game;
            Alpha = 0;

            CreateName();
            UpdateText();
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 26)
            {
                Parent = this,
            };
        }

        /// <summary>
        /// </summary>
        private void UpdateText() => ScheduleUpdate(() =>
        {
            Name.Text = Game.Value.Name;
            Size = new ScalableVector2(Name.Width, Name.Height);
        });
    }
}