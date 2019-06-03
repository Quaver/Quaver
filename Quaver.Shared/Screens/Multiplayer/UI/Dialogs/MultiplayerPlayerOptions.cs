using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Steamworks;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Multiplayer.UI.Dialogs
{
    public class MultiplayerPlayerOptions : Sprite
    {
        /// <summary>
        /// </summary>
        private OnlineUser User { get; }

        /// <summary>
        /// </summary>
        private MultiplayerPlayerOptionsContainer Container { get; set; }

        /// <summary>
        /// </summary>
        private MultiplayerPlayerOptionsDialog Dialog { get; }

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="user"></param>
        public MultiplayerPlayerOptions(MultiplayerPlayerOptionsDialog dialog, OnlineUser user)
        {
            Dialog = dialog;
            User = user;

            Image = UserInterface.PlayerOptionsPanel;
            Size = new ScalableVector2(450, 192);

            CreateContainer();
        }

        private void CreateContainer()
        {
            var options = new List<IMultiplayerPlayerOption>
            {
                new MultiplayerPlayerOption("View Profile", () => BrowserHelper.OpenURL($"https://quavergame.com/profile/{User.Id}")),
                new MultiplayerPlayerOption("Steam Profile", () => BrowserHelper.OpenURL($"https://steamcommunity.com/profiles/{User.SteamId}")),
            };

            // Other Player Actions
            if (User.Id != OnlineManager.Self.OnlineUser.Id)
            {
                options.Add(new MultiplayerPlayerOption("Private Chat", () =>
                {
                    var list = new List<string>()
                    {
                        // Have to add a BS element in the beginning since the method assumes that its a chat command
                        // and removes the first element
                        "chat"
                    };

                    QuaverBot.ExecuteChatCommand(list.Concat(User.Username.Split(" ")));
                    ChatManager.ToggleChatOverlay(true);
                }));

                // We're the host, so add some host actions
                if (OnlineManager.Self.OnlineUser.Id == OnlineManager.CurrentGame.HostId)
                {
                    options = options.Concat(new List<IMultiplayerPlayerOption>
                    {
                        new MultiplayerPlayerOption("Kick Player", () => OnlineManager.Client.KickMultiplayerGamePlayer(User.Id)),
                        new MultiplayerPlayerOption("Give Host", () => OnlineManager.Client.TransferMultiplayerGameHost(User.Id)),
                    }).ToList();

                    if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
                    {
                        options.Add(new MultiplayerPlayerOption("Change Team", () =>
                        {
                            var team = OnlineManager.GetTeam(User.Id);

                            switch (team)
                            {
                                case MultiplayerTeam.Red:
                                    team = MultiplayerTeam.Blue;
                                    break;
                                case MultiplayerTeam.Blue:
                                    team = MultiplayerTeam.Red;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            OnlineManager.Client.ChangeOtherPlayerTeam(User.Id, team);
                        }));
                    }
                }
            }

            options.Add(new MultiplayerPlayerOption("Close", () => DialogManager.Dismiss(Dialog)));

            Container = new MultiplayerPlayerOptionsContainer(Dialog, options)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
        }
    }
}