using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected
{
    public class SelectedGamePanel : Sprite
    {
        /// <summary>
        /// </summary>
        public bool IsMultiplayer { get; }

        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> SelectedGame { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus SubHeader { get; set; }

        /// <summary>
        /// </summary>
        private SelectedGamePanelContainer Container { get; set; }

        /// <summary>
        /// </summary>
        private Sprite PanelRectangle { get; set; }

        /// <summary>
        /// </summary>
        public SelectedGamePanel(Bindable<MultiplayerGame> selectedGame, bool isMultiplayer = false)
        {
            SelectedGame = selectedGame;
            IsMultiplayer = isMultiplayer;

            Size = new ScalableVector2(564, 838);
            Alpha = 0;

            CreateHeaderText();
            CreateSubHeaderText();
            CreateContainer();
        }

        /// <summary>
        ///    Creates <see cref="Header"/>
        /// </summary>
        private void CreateHeaderText()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoHeavy), "MATCH DETAILS", 30)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
            };
        }

        /// <summary>
        ///    Creates <see cref="SubHeader"/>
        /// </summary>
        private void CreateSubHeaderText()
        {
            SubHeader = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "View information about a match".ToUpper(), 18)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Tint = ColorHelper.HexToColor("#808080")
            };

            SubHeader.Y = Header.Y + Header.Height - SubHeader.Height - 3;
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            PanelRectangle = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Header.Y + Header.Height + 8,
                Size = new ScalableVector2(Width,Height - Header.Height - 8),
                Image = UserInterface.LeaderboardScoresPanel
            };

            Container = new SelectedGamePanelContainer(SelectedGame, IsMultiplayer,
                new ScalableVector2(PanelRectangle.Width - 4, PanelRectangle.Height - 4))
            {
                Parent = PanelRectangle,
                Alignment = Alignment.MidCenter
            };
        }
    }
}