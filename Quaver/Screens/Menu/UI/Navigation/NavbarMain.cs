using System;
using System.Collections.Generic;
using Quaver.Online;
using Quaver.Online.Chat;
using Quaver.Server.Client;
using Wobble.Bindables;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Screens.Menu.UI.Navigation
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