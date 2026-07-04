using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection;
using Wobble;

namespace Quaver.Shared.Screens.Multiplayer.UI
{
    public class MenuFooterMultiplayer : MenuFooter
    {
        /// <summary>
        /// </summary>
        private ButtonText ReadyUp { get; set; }

        /// <summary>
        /// </summary>
        private ButtonText SelectMap { get; set; }

        /// <summary>
        /// </summary>
        private ButtonText SelectModifiers { get; set; }

        /// <summary>
        /// </summary>
        private ButtonText ChangeTeam { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leftAligned"></param>
        /// <param name="rightAlighed"></param>
        /// <param name="colorTheme"></param>
        public MenuFooterMultiplayer(List<ButtonText> leftAligned, List<ButtonText> rightAlighed, Color colorTheme)
            : base(leftAligned, rightAlighed, colorTheme)
        {
            CreateReadyUpButton();
            CreateSelectMapButton();
            CreateModifiersButton();
            CreateChangeTeamButton();
            AlignRightItems(RightAligned);

            OnlineManager.Client.OnGameHostChanged += OnGameHostChanged;
            OnlineManager.Client.OnPlayerReady += OnPlayerReady;
            OnlineManager.Client.OnPlayerNotReady += OnPlayerNotReady;
            OnlineManager.Client.OnGameCountdownStart += OnCountdownStart;
            OnlineManager.Client.OnGameCountdownStop += OnCountdownStop;
            OnlineManager.Client.OnFreeModTypeChanged += OnFreeModTypeChanged;
            OnlineManager.Client.OnGameRulesetChanged += OnGameRulesetChanged;
            OnlineManager.Client.OnGamePlayerNoMap += OnGamePlayerNoMap;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnGameHostChanged -= OnGameHostChanged;
            OnlineManager.Client.OnPlayerReady -= OnPlayerReady;
            OnlineManager.Client.OnPlayerNotReady -= OnPlayerNotReady;
            OnlineManager.Client.OnGameCountdownStart -= OnCountdownStart;
            OnlineManager.Client.OnGameCountdownStop -= OnCountdownStop;
            OnlineManager.Client.OnGameRulesetChanged -= OnGameRulesetChanged;
            OnlineManager.Client.OnGamePlayerNoMap -= OnGamePlayerNoMap;
            ReadyUp.Destroy();
            SelectMap.Destroy();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateReadyUpButton()
        {
            ReadyUp = new ButtonText(FontManager.GetWobbleFont(Fonts.LatoBlack),
                OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser ? MultiplayerLocalization.Get("StartMatch") : MultiplayerLocalization.Get("ReadyUp"), 14, (o, e) =>
                {
                    if (OnlineManager.CurrentGame.InProgress)
                    {
                        NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("WaitUntilMatchFinishes"));
                        return;
                    }

                    // We're host, so start/stop the match countdown
                    if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                    {
                        if (OnlineManager.CurrentGame.CountdownStartTime == -1)
                        {
                            OnlineManager.Client?.MultiplayerGameStartCountdown();
                            OnlineManager.Client?.MultiplayerGameIsReady();
                        }
                        else
                        {
                            OnlineManager.Client?.MultiplayerGameStopCountdown();
                            OnlineManager.Client?.MultiplayerGameIsNotReady();
                        }
                    }
                    // Not host, so the button should be a ready toggle.
                    else if (!OnlineManager.CurrentGame.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                    {
                        if (OnlineManager.CurrentGame.InProgress)
                        {
                            NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("WaitUntilMatchFinishesBeforeReadying"));
                            return;
                        }

                        if (OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
                            NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("CannotReadyWithoutMap"));
                        else
                            OnlineManager.Client?.MultiplayerGameIsReady();
                    }
                    else
                        OnlineManager.Client?.MultiplayerGameIsNotReady();
                })
            {
                DestroyIfParentIsNull = false
            };

            RightAligned.Add(ReadyUp);
        }

        /// <summary>
        /// </summary>
        private void CreateSelectMapButton()
        {
            SelectMap = new ButtonText(FontManager.GetWobbleFont(Fonts.LatoBlack), MultiplayerLocalization.Get("SelectMap"), 14, (o, e) =>
            {
                if (OnlineManager.CurrentGame.InProgress)
                {
                    NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("WaitUntilMatchFinishesBeforeSelectingMap"));
                    return;
                }

                if (OnlineManager.CurrentGame.Host != OnlineManager.Self.OnlineUser)
                {
                    NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("CannotSelectMapIfNotHost"));
                    return;
                }

                var game = (QuaverGame)GameBase.Game;
                var screen = game.CurrentScreen as MultiplayerScreen;
                screen?.Exit(() => new SelectionScreen(), 0);
            })
            {
                DestroyIfParentIsNull = false
            };

            if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                RightAligned.Add(SelectMap);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameHostChanged(object sender, GameHostChangedEventArgs e)
        {
            if (e.UserId == OnlineManager.Self.OnlineUser.Id)
            {
                ReadyUp.ChangeText(MultiplayerLocalization.Get("StartMatch"));

                if (!RightAligned.Contains(SelectMap))
                    RightAligned.Add(SelectMap);

                if (!RightAligned.Contains(SelectModifiers))
                    RightAligned.Add(SelectModifiers);
            }
            else
            {
                if (OnlineManager.CurrentGame.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                    ReadyUp.ChangeText(MultiplayerLocalization.Get("Unready"));
                else
                    ReadyUp.ChangeText(MultiplayerLocalization.Get("Ready"));

                RightAligned.Remove(SelectMap);
                SelectMap.Parent = null;
            }

            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerReady(object sender, PlayerReadyEventArgs e)
        {
            if (e.UserId != OnlineManager.Self.OnlineUser.Id || OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                return;

            ReadyUp.ChangeText(MultiplayerLocalization.Get("NotReady"));
            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerNotReady(object sender, PlayerNotReadyEventArgs e)
        {
            if (e.UserId != OnlineManager.Self.OnlineUser.Id || OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                return;

            ReadyUp.ChangeText(MultiplayerLocalization.Get("ReadyUp"));
            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountdownStart(object sender, StartCountdownEventArgs e)
        {
            if (OnlineManager.CurrentGame.Host != OnlineManager.Self.OnlineUser)
                return;

            ReadyUp.ChangeText(MultiplayerLocalization.Get("CancelMatchStart"));
            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCountdownStop(object sender, StopCountdownEventArgs e)
        {
            if (OnlineManager.CurrentGame.Host != OnlineManager.Self.OnlineUser)
                return;

            ReadyUp.ChangeText(MultiplayerLocalization.Get("StartMatch"));
            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGamePlayerNoMap(object sender, PlayerGameNoMapEventArgs e)
        {
            if (e.UserId != OnlineManager.Self.OnlineUser.Id)
                return;

            if (OnlineManager.CurrentGame.Host != OnlineManager.Self.OnlineUser)
                return;

            ReadyUp.ChangeText(MultiplayerLocalization.Get("ReadyUp"));
            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameRulesetChanged(object sender, RulesetChangedEventArgs e)
        {
            if (e.Ruleset == MultiplayerGameRuleset.Team)
            {
                if (!RightAligned.Contains(ChangeTeam)) ;
                RightAligned.Add(ChangeTeam);
            }
            else
            {
                RightAligned.Remove(ChangeTeam);
                ChangeTeam.Parent = null;
            }

            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnFreeModTypeChanged(object sender, FreeModTypeChangedEventArgs e)
        {
            OnlineManager.CurrentGame.FreeModType = e.Type;

            switch (e.Type)
            {
                case MultiplayerFreeModType.None:
                    if (RightAligned.Contains(SelectModifiers) && OnlineManager.CurrentGame.Host != OnlineManager.Self.OnlineUser)
                    {
                        RightAligned.Remove(SelectModifiers);
                        SelectModifiers.Parent = null;
                    }
                    break;
                default:
                    if (!RightAligned.Contains(SelectModifiers))
                        RightAligned.Add(SelectModifiers);
                    break;
            }

            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        private void CreateModifiersButton()
        {
            SelectModifiers = new ButtonText(FontManager.GetWobbleFont(Fonts.LatoBlack), MultiplayerLocalization.Get("Modifiers"), 14, (o, e) =>
            {
                if (OnlineManager.CurrentGame.InProgress)
                {
                    NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("WaitUntilMatchFinishesBeforeSelectingMods"));
                    return;
                }

                // DialogManager.Show(new ModifiersDialog());
            })
            {
                DestroyIfParentIsNull = false
            };

            if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser || OnlineManager.CurrentGame.FreeModType != MultiplayerFreeModType.None)
                RightAligned.Add(SelectModifiers);
        }

        private void CreateChangeTeamButton()
        {
            ChangeTeam = new ButtonText(FontManager.GetWobbleFont(Fonts.LatoBlack), MultiplayerLocalization.Get("ChangeTeam"), 14, (o, e) =>
            {
                if (OnlineManager.CurrentGame.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                {
                    NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("MustUnreadyToSwitchTeams"));
                    return;
                }

                if (OnlineManager.CurrentGame.CountdownStartTime != -1)
                {
                    NotificationManager.Show(NotificationLevel.Error, MultiplayerLocalization.Get("CannotSwitchTeamsDuringCountdown"));
                    return;
                }

                lock (OnlineManager.CurrentGame.BlueTeamPlayers)
                    lock (OnlineManager.CurrentGame.RedTeamPlayers)
                    {
                        if (OnlineManager.CurrentGame.BlueTeamPlayers.Contains(OnlineManager.Self.OnlineUser.Id))
                            OnlineManager.Client?.ChangeGameTeam(MultiplayerTeam.Red);
                        else if (OnlineManager.CurrentGame.RedTeamPlayers.Contains(OnlineManager.Self.OnlineUser.Id))
                            OnlineManager.Client?.ChangeGameTeam(MultiplayerTeam.Blue);
                    }
            })
            {
                DestroyIfParentIsNull = false
            };

            if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                RightAligned.Add(ChangeTeam);
        }
    }
}
