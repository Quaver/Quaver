using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
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
            Size = new ScalableVector2(450, 354);

            CreateContainer();
        }

        private void CreateContainer()
        {
            var options = new List<IMenuDialogOption>
            {
                new MenuDialogOption("View Profile", () => BrowserHelper.OpenURL($"https://quavergame.com/profile/{User.Id}")),
                new MenuDialogOption("Steam Profile", () => BrowserHelper.OpenURL($"https://steamcommunity.com/profiles/{User.SteamId}")),
            };

            // Other Player Actions
            if (User.Id != OnlineManager.Self.OnlineUser.Id)
            {
                // We're the host, so add some host actions
                if (OnlineManager.Self.OnlineUser.Id == OnlineManager.CurrentGame.HostId)
                {
                    options = options.Concat(new List<IMenuDialogOption>
                    {
                        new MenuDialogOption("Kick Player", () => OnlineManager.Client.KickMultiplayerGamePlayer(User.Id), Color.Crimson),
                        new MenuDialogOption("Give Host", () => OnlineManager.Client.TransferMultiplayerGameHost(User.Id), Color.Lime),
                    }).ToList();

                    if (OnlineManager.CurrentGame.Ruleset == MultiplayerGameRuleset.Team)
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

                        var color = team == MultiplayerTeam.Red ? Color.Crimson : new Color(25, 104, 249);
                        options.Add(new MenuDialogOption($"Change Team ({team})", () =>  OnlineManager.Client.ChangeOtherPlayerTeam(User.Id, team), color));
                    }
                }
            }

            options.Add(new MenuDialogOption("Close", () => DialogManager.Dismiss(Dialog)));

            Container = new MultiplayerPlayerOptionsContainer(Dialog, options)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };
        }
    }
}