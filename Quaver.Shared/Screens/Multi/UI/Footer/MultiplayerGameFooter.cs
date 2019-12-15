using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Menu.Border.Components.Buttons;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class MultiplayerGameFooter : MenuBorder
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        private IconTextButtonMultiplayerSelectMap SelectMap { get; }

        /// <summary>
        /// </summary>
        private IconTextButtonMultiplayerSwitchTeams SwitchTeams { get; }

        public MultiplayerGameFooter(QuaverScreen screen, Bindable<MultiplayerGame> game) : base(MenuBorderType.Footer,
            new List<Drawable>()
            {
                new IconTextButtonLeaveMultiplayerGame(screen),
                new IconTextButtonOptions(),
                new IconTextButtonMultiplayerLeaderboard((MultiplayerGameScreen) screen),
                new IconTextButtonMultiplayerCommands(),
            },
            new List<Drawable>()
            {
                new IconTextButtonMultiplayerReady(game),
                new IconTextButtonMultiplayerMatchHistory(game),
            })
        {
            Game = game;

            SelectMap = new IconTextButtonMultiplayerSelectMap(screen) { DestroyIfParentIsNull = false};
            SwitchTeams = new IconTextButtonMultiplayerSwitchTeams(Game) { DestroyIfParentIsNull = false};

            UpdateState();

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnPlayerReady += OnPlayerReady;
                OnlineManager.Client.OnPlayerNotReady += OnPlayerNotReady;
                OnlineManager.Client.OnGameHostChanged += OnHostChanged;
                OnlineManager.Client.OnGameRulesetChanged += OnRulesetChanged;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnPlayerReady -= OnPlayerReady;
            OnlineManager.Client.OnPlayerNotReady -= OnPlayerNotReady;
            OnlineManager.Client.OnGameHostChanged -= OnHostChanged;
            OnlineManager.Client.OnGameRulesetChanged -= OnRulesetChanged;

            SelectMap?.Destroy();
            SwitchTeams?.Destroy();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void UpdateState() => AddScheduledUpdate(() =>
        {
            // Map selection button
            if (Game.Value.HostId == OnlineManager.Self?.OnlineUser.Id)
            {
                if (!RightAlignedItems.Contains(SelectMap))
                    RightAlignedItems.Add(SelectMap);
            }
            else
            {
                if (RightAlignedItems.Contains(SelectMap))
                {
                    RightAlignedItems.Remove(SelectMap);
                    SelectMap.Parent = null;
                }
            }

            // Switch Teams
            if (Game.Value.Ruleset == MultiplayerGameRuleset.Team)
            {
                if (!RightAlignedItems.Contains(SwitchTeams))
                    RightAlignedItems.Add(SwitchTeams);
            }
            else
            {
                if (RightAlignedItems.Contains(SwitchTeams))
                {
                    RightAlignedItems.Remove(SwitchTeams);
                    SwitchTeams.Parent = null;
                }
            }

            AlignRightItems();
        });

        private void OnPlayerNotReady(object sender, PlayerNotReadyEventArgs e) => UpdateState();

        private void OnPlayerReady(object sender, PlayerReadyEventArgs e) => UpdateState();

        private void OnHostChanged(object sender, GameHostChangedEventArgs e) => UpdateState();

        private void OnRulesetChanged(object sender, RulesetChangedEventArgs e) => UpdateState();
    }
}