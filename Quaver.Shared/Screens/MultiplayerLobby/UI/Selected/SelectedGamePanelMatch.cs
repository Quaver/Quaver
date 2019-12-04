using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected
{
    public class SelectedGamePanelMatch : Sprite, IMultiplayerGameComponent
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public Bindable<MultiplayerGame> SelectedGame { get; set; }

        /// <summary>
        /// </summary>
        private SelectedGamePanelMatchBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private DrawableMultiplayerTable Table { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="selectedGame"></param>
        /// <param name="size"></param>
        public SelectedGamePanelMatch(Bindable<MultiplayerGame> selectedGame, ScalableVector2 size)
        {
            SelectedGame = selectedGame;
            Size = size;
            Alpha = 0;

            CreateBanner();
            CreateTable();

            UpdateState();

            SelectedGame.ValueChanged += OnSelectedGameChanged;
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new SelectedGamePanelMatchBanner(SelectedGame, new ScalableVector2(Width, 136))
            {
                Parent = this,
                X = 1,
                Alignment = Alignment.TopCenter
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTable()
        {
            Table = new DrawableMultiplayerTable(SelectedGame, new ScalableVector2(Width,
                Height - Banner.Height))
            {
                Parent = this,
                Y = Banner.Y + Banner.Height
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void UpdateState()
        {
            foreach (var child in Children)
            {
                if (child is IMultiplayerGameComponent component)
                    component.UpdateState();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedGameChanged(object sender, BindableValueChangedEventArgs<MultiplayerGame> e)
            => UpdateState();
    }
}