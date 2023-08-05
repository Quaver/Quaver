using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class MultiplayerTableItemAutoHost : MultiplayerTableItem
    {
        public MultiplayerTableItemAutoHost(Bindable<MultiplayerGame> game, bool isMultiplayer) : base(game, isMultiplayer)
        {
            var checkbox = new MultiplayerAutoHostCheckbox(game);
            Selector = checkbox;

            if (OnlineManager.CurrentGame != null)
                ClickAction = () => checkbox?.FireButtonClickEvent();
        }

        public override string GetName() => "Auto Host";

        public override string GetValue() => SelectedGame.Value.IsAutoHost ? "Yes" : "No";
    }

    public class MultiplayerAutoHostCheckbox : QuaverCheckbox
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerAutoHostCheckbox(Bindable<MultiplayerGame> game)
            : base(new Bindable<bool>(game?.Value?.IsAutoHost ?? false))
        {
            Game = game;

            Clicked += (sender, args) => { OnlineManager.Client.ChangeAutoHost(BindedValue.Value); };
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BindedValue.Value = Game?.Value?.IsAutoHost ?? false;
        }
    }
}