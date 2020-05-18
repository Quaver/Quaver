using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected
{
    public class SelectedGamePanelContainer : Sprite
    {
        /// <summary>
        ///     If the container is for multiplayer. If not, then it'll be for the multiplayer lobby
        /// </summary>
        private bool IsMultiplayer { get; }

        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> SelectedGame { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus NoGameSelected { get; set; }

        /// <summary>
        /// </summary>
        private SelectedGamePanelMatch Match { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="selectedGame"></param>
        /// <param name="isMultiplayer"></param>
        /// <param name="size"></param>
        public SelectedGamePanelContainer(Bindable<MultiplayerGame> selectedGame, bool isMultiplayer, ScalableVector2 size)
        {
            SelectedGame = selectedGame;
            IsMultiplayer = isMultiplayer;

            Alpha = 0;
            Size = size;

            CreateNoGameSelectedText();
            CreateSelectedGamePanelMatch();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            NoGameSelected.Visible = SelectedGame.Value == null;
            Match.Visible = SelectedGame.Value != null;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateNoGameSelectedText()
        {
            NoGameSelected = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "No match selected!\n\nClick on a game to view its details,\nor create your own".ToUpper(), 20)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSelectedGamePanelMatch()
        {
            Match = new SelectedGamePanelMatch(SelectedGame, IsMultiplayer, Size)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
            };
        }
    }
}