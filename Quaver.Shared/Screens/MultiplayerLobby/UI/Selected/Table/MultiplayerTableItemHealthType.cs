using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class MultiplayerTableItemHealthType : MultiplayerTableItem
    {
        public MultiplayerTableItemHealthType(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            var checkbox = new MultiplayerHealthTypeCheckbox(game);
            Selector = checkbox;

            if (OnlineManager.CurrentGame != null)
                ClickAction = () => checkbox?.FireButtonClickEvent();
        }

        public override string GetName() => "Lose A Life Upon Failing";

        public override string GetValue()
        {
            switch ((MultiplayerHealthType) SelectedGame.Value.HealthType)
            {
                case MultiplayerHealthType.Manual_Regeneration:
                    return "No";
                case MultiplayerHealthType.Lives:
                    return "Yes";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class MultiplayerHealthTypeCheckbox : QuaverCheckbox
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public MultiplayerHealthTypeCheckbox(Bindable<MultiplayerGame> game)
            : base(new Bindable<bool>(game?.Value?.HealthType == (byte) MultiplayerHealthType.Lives))
        {
            Game = game;

            Clicked += (sender, args) =>
            {
                if (BindedValue.Value)
                    OnlineManager.Client?.ChangeGameHealthType(MultiplayerHealthType.Lives);
                else
                    OnlineManager.Client?.ChangeGameHealthType(MultiplayerHealthType.Manual_Regeneration);
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            BindedValue.Value = Game?.Value?.HealthType == (byte) MultiplayerHealthType.Lives;
        }
    }
}