/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.Download;
using Wobble;
using Wobble.Bindables;
using Wobble.Logging;
using AudioEngine = Quaver.Shared.Audio.AudioEngine;

namespace Quaver.Shared.Screens.Menu.UI.Navigation
{
    public class NavbarMain : Navbar
    {
        /// <summary>
        ///     The button used to open the in-game chat
        /// </summary>
        private NavbarItem OpenChatButton { get; }

        /// <summary>
        ///     The button used to download maps ingame
        /// </summary>
        private NavbarItem DownloadMapsButton { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="leftAlignedItems"></param>
        /// <param name="rightAlignedItems"></param>
        /// <param name="isUpsideDown"></param>
        public NavbarMain(QuaverScreen screen, List<NavbarItem> leftAlignedItems, List<NavbarItem> rightAlignedItems, bool isUpsideDown = false)
            : base(leftAlignedItems, rightAlignedItems, isUpsideDown)
        {
            // Add community chat button
            DownloadMapsButton = new NavbarItem("Download", screen.Type == QuaverScreenType.Download) { DestroyIfParentIsNull = false };
            DownloadMapsButton.Clicked += (o, e) => OnDownloadMapsButtonClicked();

            // Add community chat button
            OpenChatButton = new NavbarItem("Community Chat") { DestroyIfParentIsNull = false };
            OpenChatButton.Clicked += (o, e) => ChatManager.ToggleChatOverlay(true);

            // Make sure all online buttons are there if applicable
            if (OnlineManager.Status.Value == ConnectionStatus.Connected ||
                OnlineManager.Status.Value == ConnectionStatus.Reconnecting)
            {
                LeftAlignedItems.Add(DownloadMapsButton);
                LeftAlignedItems.Add(OpenChatButton);
                AlignLeftItems();
            }

            OnlineManager.Status.ValueChanged += OnOnlineStatusChanged;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnLoginSuccess += OnLoginSuccess;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;

            if (OnlineManager.Client != null)
                OnlineManager.Client.OnLoginSuccess -= OnLoginSuccess;

            base.Destroy();
        }

        /// <summary>
        ///     Called when the user's online status has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOnlineStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            try
            {
                switch (e.Value)
                {
                    case ConnectionStatus.Disconnected:
                        if (LeftAlignedItems.Contains(DownloadMapsButton))
                        {
                            DownloadMapsButton.Parent = null;
                            LeftAlignedItems.Remove(DownloadMapsButton);
                        }

                        if (LeftAlignedItems.Contains(OpenChatButton))
                        {
                            OpenChatButton.Parent = null;
                            LeftAlignedItems.Remove(OpenChatButton);
                        }

                        AlignLeftItems();
                        break;
                    case ConnectionStatus.Connecting:
                        break;
                    case ConnectionStatus.Connected:
                        if (!LeftAlignedItems.Contains(DownloadMapsButton))
                            LeftAlignedItems.Add(DownloadMapsButton);

                        if (!LeftAlignedItems.Contains(OpenChatButton))
                            LeftAlignedItems.Add(OpenChatButton);

                        AlignLeftItems();
                        break;
                    case ConnectionStatus.Reconnecting:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, LogType.Runtime);

            }
        }

        /// <summary>
        ///     Realign the nav items upon login success
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginSuccess(object sender, LoginReplyEventArgs e)
        {
            AlignLeftItems();
            AlignRightItems();
        }

        /// <summary>
        ///     Called when the user wants to go and download maps.
        /// </summary>
        private void OnDownloadMapsButtonClicked()
        {
            var game = GameBase.Game as QuaverGame;

            game?.CurrentScreen?.Exit(() =>
            {
                AudioEngine.Track?.Fade(10, 300);
                return new DownloadScreen();
            });
        }
    }
}
