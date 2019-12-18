using System.Collections.Generic;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Multi.UI.Status.Name;
using Quaver.Shared.Screens.Multi.UI.Status.Password;
using Quaver.Shared.Screens.Multi.UI.Status.Selection;
using Quaver.Shared.Screens.Multi.UI.Status.Sharing;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Filter;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
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
        private FilterPanelBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMultiplayerGameName Name { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMultiplayerGameStatus Status { get; set; }

        /// <summary>
        /// </summary>
        private PausePlayButton PausePlayButton { get; set; }

        /// <summary>
        /// </summary>
        private ShareMultiplayerMapsetButton ShareMapset { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerChangeGameNameButton ChangeName { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerEditPasswordButton EditPasswordButton { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerSelectMapButton SelectMap { get; set; }

        /// <summary>
        ///     Items that are aligned from right to left
        /// </summary>
        private List<Drawable> RightItems { get; } = new List<Drawable>();

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
            CreatePausePlayButton();
            CreateShareMapsetButton();
            CreateEditPasswordButton();
            CreateChangeGameNameButton();
            //CreateSelectMapButton();

            AlignRightItems();
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new FilterPanelBanner(this)
            {
                Parent = this,
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
                Position = new ScalableVector2(24, 18)
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
        private void CreatePausePlayButton()
        {
            PausePlayButton = new PausePlayButton(UserInterface.JukeboxPauseButton, UserInterface.JukeboxPlayButton)
            {
                Size = new ScalableVector2(36, 36),
            };

            RightItems.Add(PausePlayButton);
        }

        /// <summary>
        /// </summary>
        private void CreateShareMapsetButton()
        {
            ShareMapset = new ShareMultiplayerMapsetButton(Game);
            RightItems.Add(ShareMapset);
        }

        /// <summary>
        /// </summary>
        private void CreateEditPasswordButton()
        {
            EditPasswordButton = new MultiplayerEditPasswordButton(Game);
            RightItems.Add(EditPasswordButton);
        }

        /// <summary>
        /// </summary>
        private void CreateChangeGameNameButton()
        {
            ChangeName = new MultiplayerChangeGameNameButton(Game);
            RightItems.Add(ChangeName);
        }

        /// <summary>
        /// </summary>
        private void CreateSelectMapButton()
        {
            SelectMap = new MultiplayerSelectMapButton(Game);
        }

        /// <summary>
        ///     Aligns the items from right to left
        /// </summary>
        private void AlignRightItems()
        {
            for (var i = 0; i < RightItems.Count; i++)
            {
                var item = RightItems[i];

                item.Parent = this;

                item.Alignment = Alignment.MidRight;

                const int padding = 25;
                var spacing = 36;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
        }
    }
}