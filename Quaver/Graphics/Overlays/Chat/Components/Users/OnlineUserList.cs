using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using osu_database_reader;
using Quaver.Config;
using Quaver.Online;
using Quaver.Online.Chat;
using Quaver.Scheduling;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Input;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Graphics.Overlays.Chat.Components.Users
{
    public class OnlineUserList : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     Dictionary containing all of our drawable users.
        /// </summary>
        public Dictionary<int, DrawableOnlineUser> DrawableUsers { get; } = new Dictionary<int, DrawableOnlineUser>();

        /// <summary>
        ///     The list of currently shown drawable users.
        /// </summary>
        public List<DrawableOnlineUser> ShownUsers { get; private set; } = new List<DrawableOnlineUser>();

        /// <summary>
        ///     The pool index at which the shown users are actually drawn.
        /// </summary>
        private int PoolStartingIndex { get; set; }

        /// <summary>
        ///     The amount of users that are able to be shown.
        /// </summary>
        private const int POOL_SIZE = 20;

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="overlay"></param>
        public OnlineUserList(ChatOverlay overlay) : base(
            new ScalableVector2(overlay.OnlineUsersContainer.Width, overlay.OnlineUsersContainer.Height - overlay.OnlineUsersHeader.Height
                                                                                                        - overlay.OnlineUserFilters.Height),
            new ScalableVector2(overlay.OnlineUsersContainer.Width, overlay.OnlineUsersContainer.Height - overlay.OnlineUsersHeader.Height
                                                                                                        - overlay.OnlineUserFilters.Height + 1))
        {
            Overlay = overlay;
            Parent = Overlay.OnlineUsersContainer;
            X = -1;
            Y = Overlay.OnlineUsersHeader.Height + Overlay.OnlineUserFilters.Height;
            Tint = Color.Black;
            Alpha = 0.85f;

            InputEnabled = true;

            // Scrolling Options.
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 3;
            ScrollSpeed = 150;
            EasingType = Easing.EaseOutQuint;
            TimeToCompleteScroll = 1500;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Only allow the container to be scrollable if the mouse is actually on top of the area.
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position);

            lock (ShownUsers)
            {
                foreach (var u in ShownUsers)
                {
                    u.Visible = ScreenRectangle.ToRectangle().Intersects(u.ScreenRectangle.ToRectangle());
                }
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            OnlineManager.Client.OnUserConnected -= OnUserConnected;

            base.Destroy();
        }

        /// <summary>
        ///     Reorganizes the list of users and shows them alphabetized/only the ones that matter.
        /// </summary>
        public void ReorganizeList()
        {
            return;

            lock (ShownUsers)
            {
                var shownUsers = new List<DrawableOnlineUser>();

                var users = DrawableUsers.Values.ToList().OrderByDescending(x => x.User.HasUserInfo).ThenBy(x => x.Username.Text).ToList();

                foreach (var u in users)
                {
                    var user = DrawableUsers[u.User.OnlineUser.Id];

                    var shouldDraw = CheckIfUserShouldBeDrawn(user);

                    if (!shouldDraw)
                    {
                        RemoveContainedDrawable(user);
                    }
                    else
                    {
                        // Get the y position of the user.
                        user.Y = user.Height * shownUsers.Count;

                        if (shownUsers.All(x => x.User.OnlineUser.Id != user.User.OnlineUser.Id))
                        {
                            shownUsers.Add(user);
                            user.Avatar.UsePreviousSpriteBatchOptions = true;
                            AddContainedDrawable(user);
                        }
                    }

                    ShownUsers = shownUsers;
                    var totalHeight = ShownUsers.Count > 0 ? ShownUsers.First().Height * ShownUsers.Count : 0;

                    // Calculate the new height of the container based on how many users there are
                    ContentContainer.Height = totalHeight > Height
                        ? totalHeight
                        : Overlay.ChannelContainer.Height - Overlay.ChannelHeader.Height;
                }
            }
        }

        /// <summary>
        ///     Called when we've received the list of users that are currently online.
        /// </summary>
        public void InitializeOnlineUsers()
        {
            /*foreach (var user in OnlineManager.OnlineUsers)
            {
                if (DrawableUsers.ContainsKey(user.Value.OnlineUser.Id))
                    continue;

                DrawableUsers[user.Value.OnlineUser.Id] = new DrawableOnlineUser(Overlay, this, user.Value);
            }*/
        }

        /// <summary>
        ///     Called when a user connects to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUserConnected(object sender, UserConnectedEventArgs e)
        {
            if (DrawableUsers.ContainsKey(e.User.OnlineUser.Id))
                return;

            // Create a new drawable user for them.
            DrawableUsers[e.User.OnlineUser.Id] = new DrawableOnlineUser(Overlay, this, e.User);
        }

        /// <summary>
        ///     Called when a user disconnects from the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUserDisconnected(object sender, UserDisconnectedEventArgs e)
        {
            lock (ShownUsers)
            {
                lock (DrawableUsers)
                {
                    var user = DrawableUsers.ToList().Find(x => x.Value.User.OnlineUser.Id == e.UserId);

                    if (user.Value != null)
                    {
                        user.Value.SetChildrenVisibility = true;
                        user.Value.Visible = false;
                        DrawableUsers.Remove(user.Key);
                    }
                }
            }

            ReorganizeList();
        }

        /// <summary>
        ///     Determines if the user should actually be drawn.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static bool CheckIfUserShouldBeDrawn(DrawableOnlineUser u)
        {
            switch (ConfigManager.SelectedOnlineUserFilterType.Value)
            {
                case OnlineUserFilterType.All:
                    return true;
                    break;
                case OnlineUserFilterType.Friends:
                    return false;
                    break;
                case OnlineUserFilterType.Country:
                    return u.User.OnlineUser.CountryFlag ==
                                 OnlineManager.Self.OnlineUser.CountryFlag;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}