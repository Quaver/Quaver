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
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Quaver.Graphics.Overlays.Chat.Components.Users
{
    public class OnlineUserList : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     The buffer of drawable online users that are to be shown.
        /// </summary>
        public List<DrawableOnlineUser> ShownUsers { get; set; } = new List<DrawableOnlineUser>();

        /// <summary>
        ///     The y position of the content container in the previous frame.
        /// </summary>
        public float PreviousContentContainerY { get; set; }

        /// <summary>
        ///     The maximum amount of users to be shown at once.
        /// </summary>
        public const int MAX_USERS_SHOWN = 15;

        /// <summary>
        ///     The index at which the messages are starting to be shown.
        /// </summary>
        public int PoolStartingIndex { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="overlay"></param>
        public OnlineUserList(ChatOverlay overlay) : base(
            new ScalableVector2(overlay.OnlineUsersContainer.Width, overlay.OnlineUsersContainer.Height - overlay.OnlineUsersHeader.Height
                                                                                                        - overlay.OnlineUserFilters.Height),
            new ScalableVector2(overlay.OnlineUsersContainer.Width, overlay.OnlineUsersContainer.Height - overlay.OnlineUsersHeader.Height
                                                                                                        - overlay.OnlineUserFilters.Height))
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

            // Handle pool shifting when scrolling up or down.
            if (ContentContainer.Y < PreviousContentContainerY)
                HandlePoolShifting(Direction.Forward);
            else if (ContentContainer.Y > PreviousContentContainerY)
                HandlePoolShifting(Direction.Backward);

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;

            base.Update(gameTime);
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
                case OnlineUserFilterType.Friends:
                    return false;
                case OnlineUserFilterType.Country:
                    return u.User.OnlineUser.CountryFlag == OnlineManager.Self.OnlineUser.CountryFlag;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Initializes more users at the bottom of the buffer.
        /// </summary>
        public void InitializeUsers()
        {
            for (var i = PoolStartingIndex; i < PoolStartingIndex + MAX_USERS_SHOWN; i++)
            {
                var user = OnlineManager.OnlineUsers.ToList()[i];

                if (ShownUsers.Any(x => x.User.OnlineUser.Id == user.Value.OnlineUser.Id))
                    continue;

                var drawableUser = new DrawableOnlineUser(Overlay, this, user.Value)
                {
                    Y = (PoolStartingIndex + i) * DrawableOnlineUser.HEIGHT
                };

                AddContainedDrawable(drawableUser);
                ShownUsers.Add(drawableUser);
            }
        }
        
        /// <summary>
        ///     Calculates the height of the container based on the amount of users.
        /// </summary>
        public void RecalculateContainerHeight()
        {
            var totalUserHeight = DrawableOnlineUser.HEIGHT * OnlineManager.OnlineUsers.Count;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
        }

        /// <summary>
        ///     Handles shifting the pool whenever the container is scrolled.
        ///     Also initializing any new objects that need it.
        /// </summary>
        /// <param name="direction"></param>
        private void HandlePoolShifting(Direction direction)
        {
            DrawableOnlineUser user;

            switch (direction)
            {
                case Direction.Forward:
                    // First run a check to see if we even have a message in this position.
                    if (ShownUsers.ElementAtOrDefault(PoolStartingIndex) == null)
                        return;

                    // Check the top message at the pool starting index to see if it is still in range
                    user = ShownUsers[PoolStartingIndex];

                    var newRect = Rectangle.Intersect(user.ScreenRectangle.ToRectangle(), ScreenRectangle.ToRectangle());

                    if (!newRect.IsEmpty)
                        return;

                    // Since we're shifting forward, we can safely remove the button that has gone off-screen.
                    RemoveContainedDrawable(ShownUsers[PoolStartingIndex]);

                    // Now add the button that is forward.
                    if (ShownUsers.ElementAtOrDefault(PoolStartingIndex + MAX_USERS_SHOWN) != null)
                        AddContainedDrawable(ShownUsers[PoolStartingIndex + MAX_USERS_SHOWN]);
                    // Initialize the new user because it's null.
                    else
                    {
                        var users = OnlineManager.OnlineUsers.Values.ToList();

                        if (users.ElementAtOrDefault(PoolStartingIndex + MAX_USERS_SHOWN) == null)
                            return;

                        var newUser = OnlineManager.OnlineUsers.ToList()[PoolStartingIndex + MAX_USERS_SHOWN];

                        if (ShownUsers.Any(x => x.User.OnlineUser.Id == newUser.Value.OnlineUser.Id))
                            return;

                        var drawableUser = new DrawableOnlineUser(Overlay, this, newUser.Value)
                        {
                            Y = (PoolStartingIndex + MAX_USERS_SHOWN) * DrawableOnlineUser.HEIGHT
                        };

                        AddContainedDrawable(drawableUser);
                        ShownUsers.Add(drawableUser);
                    }

                    // Increment the starting index to shift it.
                    PoolStartingIndex++;
                    break;
                case Direction.Backward:
                    // First run a check to see if we even have a message in this position.
                    if (ShownUsers.ElementAtOrDefault(PoolStartingIndex - 1) == null
                        || ShownUsers.ElementAtOrDefault(PoolStartingIndex + MAX_USERS_SHOWN - 1) == null)
                        return;

                    user = ShownUsers[PoolStartingIndex + MAX_USERS_SHOWN - 1];

                    var rect = Rectangle.Intersect(user.ScreenRectangle.ToRectangle(), ScreenRectangle.ToRectangle());

                    if (!rect.IsEmpty)
                        return;

                    // Since we're scrolling up, we need to shift backwards.
                    // Remove the drawable from the bottom one.
                    RemoveContainedDrawable(ShownUsers[PoolStartingIndex + MAX_USERS_SHOWN - 1]);
                    AddContainedDrawable(ShownUsers[PoolStartingIndex - 1]);

                    PoolStartingIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }
}