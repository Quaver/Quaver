using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class MultiplayerTableItemFreeMod : MultiplayerTableItem
    {
        public MultiplayerTableItemFreeMod(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            var checkbox  = new MultiplayerFreeModCheckbox(game);
            Selector = checkbox;

            if (OnlineManager.CurrentGame != null)
                ClickAction = () => checkbox?.FireButtonClickEvent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string GetName() => "Free Mod";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override string GetValue()
            => SelectedGame.Value.FreeModType.HasFlag(MultiplayerFreeModType.Regular) ? "Yes" : "No";
    }

    public class MultiplayerFreeModCheckbox : QuaverCheckbox
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerFreeModCheckbox(Bindable<MultiplayerGame> game)
            : base(new Bindable<bool>(game?.Value?.FreeModType.HasFlag(MultiplayerFreeModType.Regular) ?? false))
        {
            Game = game;
            Depth = 1;

            Clicked += (sender, args) =>
            {
                if (game?.Value == null)
                    return;

                if (game.Value.FreeModType.HasFlag(MultiplayerFreeModType.Regular))
                    OnlineManager.Client?.ChangeGameFreeModType((MultiplayerFreeModType) (OnlineManager.CurrentGame.FreeModType - MultiplayerFreeModType.Regular));
                else
                    OnlineManager.Client?.ChangeGameFreeModType(OnlineManager.CurrentGame.FreeModType | MultiplayerFreeModType.Regular);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BindedValue.Value = Game?.Value?.FreeModType.HasFlag(MultiplayerFreeModType.Regular) ?? false;
        }
    }
}