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
        public Dictionary<User, DrawableOnlineUser> DrawableUsers { get; } = new Dictionary<User, DrawableOnlineUser>();

        /// <summary>
        ///     The list of currently shown drawable users.
        /// </summary>
        public List<DrawableOnlineUser> ShownUsers { get; private set; } = new List<DrawableOnlineUser>();


        private List<User> FakeUsers = new List<User>();

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

            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
            {
                lock (FakeUsers)
                {
                    for (var i = 0; i < 100; i++)
                    {
                        var fakeUser = new User(new OnlineUser(1, 281, i.ToString(), UserGroups.Normal, 0, "CA"));
                        FakeUsers.Add(fakeUser);
                        OnUserConnected(this, new UserConnectedEventArgs(fakeUser));
                    }
                }

            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.B))
            {
                lock (FakeUsers)
                {
                    for (var i = FakeUsers.Count - 1; i >= 0; i--)
                    {
                        var x = FakeUsers[i];
                        FakeUsers.Remove(x);
                        OnUserDisconnected(this, new UserDisconnectedEventArgs(x.OnlineUser.Id));
                    }
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
            lock (ShownUsers)
            {
                var shownUsers = new List<DrawableOnlineUser>();

                var users = DrawableUsers.Keys.ToList().OrderBy(x => x.OnlineUser.Username).ToList();

                for (var i = 0; i < users.Count; i++)
                {
                    var user = DrawableUsers[users[i]];

                    var shouldDraw = CheckIfUserShouldBeDrawn(user);

                    if (!shouldDraw)
                    {
                        RemoveContainedDrawable(user);
                    }
                    else
                    {
                        // Get the y position of the user.
                        user.Y = user.Height * shownUsers.Count;

                        if (shownUsers.All(x => x.User.OnlineUser.Username != user.User.OnlineUser.Username))
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
        ///     Called when a user connects to the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnUserConnected(object sender, UserConnectedEventArgs e)
        {
            if (DrawableUsers.ContainsKey(e.User))
                return;

            // Create a new drawable user for them.
            DrawableUsers[e.User] = new DrawableOnlineUser(Overlay, this, e.User);
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
                    var user = DrawableUsers.ToList().Find(x => x.Key.OnlineUser.Id == e.UserId);

                    if (user.Key != null)
                    {
                        user.Value.SetChildrenVisibility = true;
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