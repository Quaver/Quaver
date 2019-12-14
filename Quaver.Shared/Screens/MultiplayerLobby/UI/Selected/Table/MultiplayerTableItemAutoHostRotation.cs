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
            Selector = new MultiplayerAutoHostRotationCheckbox(game);
        }

        public override string GetName() => "Auto Host Rotation";

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
            Depth = 1;

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