using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Selected.Table
{
    public sealed class MultiplayerTableItemPlayers : MultiplayerTableItem
    {
        public MultiplayerTableItemPlayers(Bindable<MultiplayerGame> game, bool isMultiplayer)
            : base(game, isMultiplayer)
        {
            Selector = new MultiplayerMaxPlayersDropdown(game);
        }

        public override string GetName()
        {
            if (IsMultiplayer)
                return "Max Players";

            return "Players";
        }

        public override string GetValue() => $"{SelectedGame.Value.PlayerIds.Count}/{SelectedGame.Value.MaxPlayers}";
    }

    public class MultiplayerMaxPlayersDropdown : Dropdown
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        public MultiplayerMaxPlayersDropdown(Bindable<MultiplayerGame> game) : base(GetOptions(),
            new ScalableVector2(100, 36), 22, Colors.MainAccent, 0)
        {
            Game = game;
            Depth = 1;

            ItemSelected += (sender, args) => OnlineManager.Client?.ChangeGameMaxPlayers(int.Parse(args.Text));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SelectedIndex = Game.Value.MaxPlayers - 2;
            SelectedText.Text = Game.Value.MaxPlayers.ToString();

            base.Update(gameTime);
        }

        private static List<string> GetOptions()
        {
            var options = new List<string>();

            for (var i = 1; i < 16; i++)
                options.Add($"{i + 1}");

            return options;
        }
    }
}