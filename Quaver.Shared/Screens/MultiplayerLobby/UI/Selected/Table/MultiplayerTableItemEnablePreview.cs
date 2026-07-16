using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Online;
using Wobble.Bindables;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class MultiplayerTableItemEnablePreview : MultiplayerTableItem
    {
        public MultiplayerTableItemEnablePreview(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            var checkbox = new MultiplayerEnablePreviewCheckbox(game);
            Selector = checkbox;

            if (OnlineManager.CurrentGame != null)
                ClickAction = () => checkbox?.FireButtonClickEvent();
        }

        public override string GetName() => MultiplayerLobbyLocalization.Get("EnablePreview");

        public override string GetValue() => SelectedGame.Value.EnablePreview ? MultiplayerLobbyLocalization.Get("Yes") : MultiplayerLobbyLocalization.Get("No");
    }

    public class MultiplayerEnablePreviewCheckbox : QuaverCheckbox
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerEnablePreviewCheckbox(Bindable<MultiplayerGame> game)
            : base(new Bindable<bool>(game?.Value?.EnablePreview ?? false))
        {
            Game = game;

            Clicked += (sender, args) =>
            {
                OnlineManager.Client.ChangeGameEnablePreview(BindedValue.Value);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BindedValue.Value = Game?.Value?.EnablePreview ?? false;
        }
    }
}
