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
    public sealed class MultiplayerTableItemRuleset : MultiplayerTableItem
    {
        public MultiplayerTableItemRuleset(Bindable<MultiplayerGame> game, bool isMultiplayer) : base(game, isMultiplayer)
        {
            Selector = new MultiplayerRulesetDropdown(game);
        }

        public override string GetName() => "Ruleset";

        public override string GetValue()
        {
            switch (SelectedGame.Value.Ruleset)
            {
                case MultiplayerGameRuleset.Free_For_All:
                    return "Free-For-All";
                // case MultiplayerGameRuleset.Team:
                //     return "Team";
                //case MultiplayerGameRuleset.Battle_Royale:
                //    return "Battle Royale";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class MultiplayerRulesetDropdown : Dropdown
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        public MultiplayerRulesetDropdown(Bindable<MultiplayerGame> game) : base(new List<string>()
        {
            "Free-For-All",
            //"Team",
            //"Battle Royale"
        }, new ScalableVector2(170, 36), 22, Colors.MainAccent, 0)
        {
            Game = game;

            ItemSelected += (sender, args) => OnlineManager.Client?.ChangeGameRuleset((MultiplayerGameRuleset) args.Index);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SelectedIndex = (int) Game.Value.Ruleset;
            SelectedText.Text = Options[SelectedIndex];

            base.Update(gameTime);
        }
    }
}