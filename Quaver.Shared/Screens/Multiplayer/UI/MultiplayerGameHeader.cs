using System;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI
{
    public class MultiplayerGameHeader : ScrollContainer
    {
        private SpriteTextBitmap GameMode { get; }

        private SpriteTextBitmap RoomName { get; }

        public MultiplayerGameHeader()
            : base(new ScalableVector2(650, 42), new ScalableVector2(382, 42))
        {
            Alpha = 0;

            GameMode = new SpriteTextBitmap(FontsBitmap.GothamBold, GetRulesetName(OnlineManager.CurrentGame.Ruleset))
            {
                Parent = this,
                X = 24,
                FontSize = 14,
                Tint = Color.White
            };

            RoomName = new SpriteTextBitmap(FontsBitmap.GothamRegular, OnlineManager.CurrentGame.Name)
            {
                Parent = this,
                X = 24,
                FontSize = 16,
                Tint = Colors.SecondaryAccent,
                Y = GameMode.Y + GameMode.Height + 4
            };

            AddContainedDrawable(RoomName);

            OnlineManager.Client.OnGameRulesetChanged += OnGameRulesetChanged;
            OnlineManager.Client.OnGameNameChanged += OnGameNameChanged;
        }

        public override void Destroy()
        {
            OnlineManager.Client.OnGameRulesetChanged -= OnGameRulesetChanged;
            OnlineManager.Client.OnGameNameChanged -= OnGameNameChanged;
            base.Destroy();
        }

        private string GetRulesetName(MultiplayerGameRuleset ruleset)
        {
            switch (ruleset)
            {
                case MultiplayerGameRuleset.Free_For_All:
                    return "Free For All";
                case MultiplayerGameRuleset.Team:
                    return "Team Versus";
                case MultiplayerGameRuleset.Battle_Royale:
                    return "Battle Royale";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnGameNameChanged(object sender, GameNameChangedEventArgs e) => RoomName.Text = e.Name;

        private void OnGameRulesetChanged(object sender, RulesetChangedEventArgs e) => GameMode.Text = GetRulesetName(e.Ruleset);
    }
}