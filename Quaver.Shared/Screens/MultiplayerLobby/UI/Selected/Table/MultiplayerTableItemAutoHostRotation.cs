using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class MultiplayerTableItemAutoHostRotation : MultiplayerTableItem
    {
        public MultiplayerTableItemAutoHostRotation(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            var checkbox = new MultiplayerAutoHostRotationCheckbox(game);
            Selector = checkbox;

            if (OnlineManager.CurrentGame != null)
                ClickAction = () => checkbox?.FireButtonClickEvent();
        }

        public override string GetName() => "Host Rotation";

        public override string GetValue() => SelectedGame.Value.HostRotation ? "Yes" : "No";
    }

    public class MultiplayerAutoHostRotationCheckbox : QuaverCheckbox
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerAutoHostRotationCheckbox(Bindable<MultiplayerGame> game)
            : base(new Bindable<bool>(game?.Value?.HostRotation ?? false))
        {
            Game = game;

            Clicked += (sender, args) =>
            {
                OnlineManager.Client.ChangeGameAutoHostRotation(BindedValue.Value);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BindedValue.Value = Game?.Value?.HostRotation ?? false;
        }
    }
}