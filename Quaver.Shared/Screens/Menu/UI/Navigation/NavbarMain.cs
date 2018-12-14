/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. 
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Quaver.Server.Client;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Menu.UI.Navigation
{
    public class NavbarMain : Navbar
    {
        /// <summary>
        ///     The button used to open the in-game chat
        /// </summary>
        private NavbarItem OpenChatButton { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="leftAlignedItems"></param>
        /// <param name="rightAlignedItems"></param>
        /// <param name="isUpsideDown"></param>
        public NavbarMain(List<NavbarItem> leftAlignedItems, List<NavbarItem> rightAlignedItems, bool isUpsideDown = false)
            : base(leftAlignedItems, rightAlignedItems, isUpsideDown)
        {
            // Add community chat button
            OpenChatButton = new NavbarItem("Community Chat") { DestroyIfParentIsNull = false };
            OpenChatButton.Clicked += (o, e) => ChatManager.ToggleChatOverlay(true);

            // Make sure all online buttons are there if applicable
            if (OnlineManager.Status.Value == ConnectionStatus.Connected ||
                OnlineManager.Status.Value == ConnectionStatus.Reconnecting)
            {
                LeftAlignedItems.Add(OpenChatButton);
                AlignLeftItems();
            }

            OnlineManager.Status.ValueChanged += OnOnlineStatusChanged;
        }



        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;
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
    }
}
