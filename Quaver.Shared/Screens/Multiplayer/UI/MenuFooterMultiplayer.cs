using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Select.UI.Modifiers;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Graphics.UI.Dialogs;

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
            ReadyUp.Destroy();
            SelectMap.Destroy();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateReadyUpButton()
        {
            ReadyUp = new ButtonText(FontsBitmap.GothamRegular,
                OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser ? "start match" : "ready up", 14, (o, e) =>
                {
                    // We're host, so start/stop the match countdown
                    if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser)
                    {
                        if (OnlineManager.CurrentGame.CountdownStartTime == -1)
                            OnlineManager.Client?.MultiplayerGameStartCountdown();
                        else
                            OnlineManager.Client?.MultiplayerGameStopCountdown();
                    }
                    // Not host, so the button should be a ready toggle.
                    else if (!OnlineManager.CurrentGame.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                    {
                        if (OnlineManager.CurrentGame.PlayersWithoutMap.Contains(OnlineManager.Self.OnlineUser.Id))
                            NotificationManager.Show(NotificationLevel.Error, "You cannot ready up if you do not have the map!");
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
            SelectMap = new ButtonText(FontsBitmap.GothamRegular, "select map", 14, (o, e) =>
            {
                if (OnlineManager.CurrentGame.Host != OnlineManager.Self.OnlineUser)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You cannot select the map if you aren't host!");
                    return;
                }

                var game = (QuaverGame) GameBase.Game;
                var screen = game.CurrentScreen as MultiplayerScreen;
                screen?.Exit(() => new SelectScreen(screen), 0, QuaverScreenChangeType.AddToStack);
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
                ReadyUp.ChangeText("START MATCH");

                if (!RightAligned.Contains(SelectMap))
                    RightAligned.Add(SelectMap);

                if (!RightAligned.Contains(SelectModifiers))
                    RightAligned.Add(SelectModifiers);
            }
            else
            {
                if (OnlineManager.CurrentGame.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                    ReadyUp.ChangeText("UNREADY");
                else
                    ReadyUp.ChangeText("READY");

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

            ReadyUp.ChangeText("NOT READY");
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

            ReadyUp.ChangeText("READY UP");
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

            ReadyUp.ChangeText("CANCEL MATCH START");
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

            ReadyUp.ChangeText("START MATCH");
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
                case MultiplayerFreeModType.Regular:
                case MultiplayerFreeModType.Rate:
                    if (!RightAligned.Contains(SelectModifiers))
                        RightAligned.Add(SelectModifiers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AlignRightItems(RightAligned);
        }

        /// <summary>
        /// </summary>
        private void CreateModifiersButton()
        {
            SelectModifiers = new ButtonText(FontsBitmap.GothamRegular, "Select Modifiers", 14, (o, e) =>
            {
                DialogManager.Show(new ModifiersDialog());
            })
            {
                DestroyIfParentIsNull = false
            };

            if (OnlineManager.CurrentGame.Host == OnlineManager.Self.OnlineUser || OnlineManager.CurrentGame.FreeModType != MultiplayerFreeModType.None)
                RightAligned.Add(SelectModifiers);
        }

        private void CreateChangeTeamButton()
        {
            ChangeTeam = new ButtonText(FontsBitmap.GothamRegular, "Change Team", 14, (o, e) =>
            {
                if (OnlineManager.CurrentGame.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                {
                    NotificationManager.Show(NotificationLevel.Error, "You must unready in order to switch teams!");
                    return;
                }

                if (OnlineManager.CurrentGame.CountdownStartTime != -1)
                {
                    NotificationManager.Show(NotificationLevel.Error, "You cannot switch teams when the countdown timer is active");
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