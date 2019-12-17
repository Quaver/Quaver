using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Filter;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Multi.UI.Status
{
    public class MultiplayerGameStatusPanel : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        private MultiplayerLobbyFilterPanelBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMultiplayerGameName Name { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMultiplayerGameStatus Status { get; set; }

        /// <summary>
        /// </summary>
        private ShareMultiplayerMapsetButton ShareMapset { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerGameStatusPanel(Bindable<MultiplayerGame> game)
        {
            Game = game;

            Size = new ScalableVector2(WindowManager.Width, 88);
            Tint = ColorHelper.HexToColor("#242424");

            CreateBanner();
            CreateName();
            CreateStatus();
            CreateShareMapsetButton();
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new MultiplayerLobbyFilterPanelBanner(new ScalableVector2(960, Height))
            {
                Parent = this
            };
        }

        /// <summary>
        /// </summary>
        private void CreateName()
        {
            Name = new DrawableMultiplayerGameName(Game)
            {
                Parent = Banner,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(16, 18)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateStatus()
        {
            Status = new DrawableMultiplayerGameStatus(Game)
            {
                Parent = Banner,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(Name.X, -Name.Y)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateShareMapsetButton()
        {
            ShareMapset = new ShareMultiplayerMapsetButton(Game)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -25
            };
        }
    }
}