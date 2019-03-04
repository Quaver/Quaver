/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Online;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Logging;
using Color = Microsoft.Xna.Framework.Color;
using Logger = Wobble.Logging.Logger;

namespace Quaver.Shared.Graphics.Overlays.Chat.Components.Users
{
    public class OnlineUserList : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent chat overlay.
        /// </summary>
        public ChatOverlay Overlay { get; }

        /// <summary>
        ///     The y position of the content container in the previous frame.
        /// </summary>
        public float PreviousContentContainerY { get; set; }

        /// <summary>
        ///     The list of available filtered users to display
        /// </summary>
        public List<User> AvailableUsers { get; private set; } = new List<User>();

        /// <summary>
        ///     The buffer of users that are used for the scroll container.
        /// </summary>
        public LinkedList<DrawableOnlineUser> UserBuffer { get; }

        /// <summary>
        ///     The amount of <see cref="UserBuffer"/> objects that are currently in use.
        ///
        ///     This is to keep count of if we have anymore drawables in the buffer that are available
        ///     to be contained or if we should reuse them.
        /// </summary>
        private int UserBufferObjectsUsed { get; set; }

        /// <summary>
        ///     The total amount of users that are shown.
        /// </summary>
        public const int MAX_USERS_SHOWN = 15;

        /// <summary>
        ///     The position of <see cref="AvailableUsers"/> in which
        ///     the users will be shown.
        /// </summary>
        public int PoolStartingIndex { get; set; }

        /// <summary>
        ///     The time the last status request update occurred.
        /// </summary>
        public long LastStatusRequestTime { get; set; }

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
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            UserBuffer = new LinkedList<DrawableOnlineUser>();

            // Create MAX_USERS_SHOWN amount of DrawableOnlineUsers
            for (var i = 0; i < MAX_USERS_SHOWN; i++)
            {
                var user = new DrawableOnlineUser(Overlay, this) {Y = i * DrawableOnlineUser.HEIGHT};
                UserBuffer.AddLast(user);

                if (i < AvailableUsers.Count)
                    AddContainedDrawable(user);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Only allow the container to be scrollable if the mouse is actually on top of the area.
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && Overlay.IsOnTop;;

            // Handle pool shifting when scrolling up or down.
            if (ContentContainer.Y < PreviousContentContainerY)
                HandlePoolShifting(Direction.Forward);
            else if (ContentContainer.Y > PreviousContentContainerY)
                HandlePoolShifting(Direction.Backward);

            // Update the previous y, AFTER checking and handling the pool shifting.
            PreviousContentContainerY = ContentContainer.Y;

            PeriodicallyRequestClientStatuses();

            base.Update(gameTime);
        }

        /// <summary>
        ///     Determines if the user should actually be drawn.
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static bool CheckIfUserShouldBeDrawn(User u)
        {
            switch (ConfigManager.SelectedOnlineUserFilterType.Value)
            {
                case OnlineUserFilterType.All:
                    return true;
                case OnlineUserFilterType.Friends:
                    return false;
                case OnlineUserFilterType.Country:
                    return u.OnlineUser.CountryFlag == OnlineManager.Self.OnlineUser.CountryFlag;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Calculates the height of the container based on the amount of users.
        /// </summary>
        private void RecalculateContainerHeight()
        {
            var totalUserHeight = DrawableOnlineUser.HEIGHT * AvailableUsers.Count;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;
        }

        /// <summary>
        ///     Handles shifting the pool whenever the container is scrolled.
        ///     Also initializing any new objects that need it.
        /// </summary>
        /// <param name="direction"></param>
        private void HandlePoolShifting(Direction direction)
        {
            switch (direction)
            {
                case Direction.Forward:
                    // If there are no available users then there's no need to do anything.
                    if (AvailableUsers.ElementAtOrDefault(PoolStartingIndex) == null
                        || AvailableUsers.ElementAtOrDefault(PoolStartingIndex + MAX_USERS_SHOWN) == null)
                        return;

                    var firstUser = UserBuffer.First();

                    // Check if the object is in the rect of the ScrollContainer.
                    // If it is, then there's no updating that needs to happen.
                    if (!Rectangle.Intersect(firstUser.ScreenRectangle, ScreenRectangle).IsEmpty)
                        return;

                    // Update the user's information and y position.
                    firstUser.Y = (PoolStartingIndex + MAX_USERS_SHOWN) * DrawableOnlineUser.HEIGHT;

                    lock (AvailableUsers)
                        firstUser.UpdateUser(AvailableUsers[PoolStartingIndex + MAX_USERS_SHOWN]);

                    // Circuluarly Shift the list forward one.
                    UserBuffer.RemoveFirst();
                    UserBuffer.AddLast(firstUser);

                    // Take this user, and place them at the bottom.
                    PoolStartingIndex++;
                    break;
                case Direction.Backward:
                    // If there are no previous available user then there's no need to shift.
                    if (AvailableUsers.ElementAtOrDefault(PoolStartingIndex - 1) == null)
                        return;

                    var lastUser = UserBuffer.Last();

                    // Check if the object is in the rect of the ScrollContainer.
                    // If it is, then there's no updating that needs to happen.
                    if (!Rectangle.Intersect(lastUser.ScreenRectangle, ScreenRectangle).IsEmpty)
                        return;

                    lastUser.Y = (PoolStartingIndex - 1) * DrawableOnlineUser.HEIGHT;

                    lock (AvailableUsers)
                        lastUser.UpdateUser(AvailableUsers[PoolStartingIndex - 1]);

                    UserBuffer.RemoveLast();
                    UserBuffer.AddFirst(lastUser);

                    PoolStartingIndex--;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

         /// <summary>
        ///     Handles new incoming online users and dictates if they are available to display
        ///     <see cref="AvailableUsers"/>
        /// </summary>
        /// <param name="users"></param>
        public void HandleNewOnlineUsers(IEnumerable<User> users)
        {
            lock (AvailableUsers)
            {
                var incomingAvailableUsers = users.Where(CheckIfUserShouldBeDrawn).ToList();
                var allAvailableUsers = new List<User>(AvailableUsers);

                // Concatenate the old list of available users with the new one, and order it properly.
                AvailableUsers = allAvailableUsers
                    .Concat(incomingAvailableUsers)
                    .ToList();

                SortUsers();
                RecalculateContainerHeight();

                Logger.Debug($"There are now {AvailableUsers.Count} total available users.", LogType.Runtime);

                // If we already have enough buffered objects, then just update all of the current buffered users.
                if (UserBufferObjectsUsed == MAX_USERS_SHOWN)
                {
                    UpdateBufferUsers();
                    return;
                }

                // Based on how many new available users we have, we can add that many new contained drawables.
                for (var i = 0; i < incomingAvailableUsers.Count && UserBufferObjectsUsed != MAX_USERS_SHOWN; i++)
                    UserBufferObjectsUsed++;

                UpdateBufferUsers();
            }
        }

        /// <summary>
        ///     Clears every single user in the list - used when successfully logging into the server.
        /// </summary>
        public void ClearAllUsers()
        {
            lock (AvailableUsers)
            {
                AvailableUsers.Clear();

                SortUsers();
                RecalculateContainerHeight();
                UpdateBufferUsers();
            }
        }

        /// <summary>
        ///     Handles when a user disconnects from the server.
        /// </summary>
        public void HandleDisconnectingUser(int userId)
        {
            lock (AvailableUsers)
            {
                AvailableUsers.RemoveAll(x => x.OnlineUser.Id == userId);

                SortUsers();
                RecalculateContainerHeight();
                UpdateBufferUsers();
            }
        }

        /// <summary>
        ///     Updates all of the current buffer users.
        /// </summary>
        public void UpdateBufferUsers()
        {
            for (var i = 0; i < UserBuffer.Count; i++)
            {
                var bufferObject = UserBuffer.ElementAt(i);

                // In the event that there aren't any available users left, we want to remove these unused
                // contained drawable objects.
                if (AvailableUsers.ElementAtOrDefault(PoolStartingIndex + i) == null)
                {
                    RemoveContainedDrawable(bufferObject);
                    UserBufferObjectsUsed--;
                    continue;
                }

                bufferObject.Y = (PoolStartingIndex + i) * DrawableOnlineUser.HEIGHT;
                bufferObject.UpdateUser(AvailableUsers[PoolStartingIndex + i]);

                // Make sure the object is contained if it isn't already.
                if (bufferObject.Parent != ContentContainer)
                    AddContainedDrawable(bufferObject);
            }
        }

        /// <summary>
        ///     Updates all the user info for the current buffer.
        /// </summary>
        public void UpdateUserInfo(User user)
        {
            lock (UserBuffer)
            {
                for (var i = 0; i < UserBuffer.Count; i++)
                {
                    var item = UserBuffer.ElementAt(i);

                    if (item.User == null || item.User.OnlineUser.Id != user.OnlineUser.Id)
                        continue;

                    item.UpdateUser(OnlineManager.OnlineUsers[user.OnlineUser.Id]);

                    var index = AvailableUsers.FindIndex(x => x.OnlineUser.Id == item.User.OnlineUser.Id);

                    if (index == -1)
                        continue;

                    AvailableUsers[index] = item.User;

                    SortUsers();
                    RecalculateContainerHeight();
                    UpdateBufferUsers();
                }
            }
        }

        /// <summary>
        ///     Orders the available users in the list.
        /// </summary>
        private void SortUsers()
        {
            // ReSharper disable once ArrangeMethodOrOperatorBody
            AvailableUsers = AvailableUsers
                .OrderBy(x => !x.HasUserInfo)
                .ThenBy(x => x.OnlineUser.Username)
                .ToList();
        }

        /// <summary>
        ///     Filters the online users by whichever filter the user has selected.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void FilterUsers()
        {
            lock (AvailableUsers)
            lock (OnlineManager.OnlineUsers)
            {
                switch (ConfigManager.SelectedOnlineUserFilterType.Value)
                {
                    case OnlineUserFilterType.All:
                        AvailableUsers = OnlineManager.OnlineUsers.Values.ToList();
                        break;
                    case OnlineUserFilterType.Friends:
                        AvailableUsers = new List<User>();
                        break;
                    case OnlineUserFilterType.Country:
                        AvailableUsers = OnlineManager.OnlineUsers.Values.ToList()
                            .Where(x => x.OnlineUser.CountryFlag == OnlineManager.Self.OnlineUser.CountryFlag)
                            .ToList();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                SortUsers();
                RecalculateContainerHeight();
                UpdateBufferUsers();
            }
        }

        /// <summary>
        ///     Filters users by name.
        /// </summary>
        /// <param name="text"></param>
        public void FilterUsers(string text)
        {
            lock (AvailableUsers)
            lock (OnlineManager.OnlineUsers)
            {
                // If the user searches for nothing, re-filter the user's by what's in config.
                if (string.IsNullOrEmpty(text))
                {
                    FilterUsers();
                    return;
                }

                AvailableUsers = OnlineManager.OnlineUsers.Values.ToList()
                    .Where(x => x.HasUserInfo && x.OnlineUser.Username.ToLower().Contains(text.ToLower()))
                    .ToList();

                SortUsers();
                RecalculateContainerHeight();
                UpdateBufferUsers();
            }
        }

        /// <summary>
        ///     Periodically requests the client statuses for all shown users in the buffer.
        /// </summary>
        private void PeriodicallyRequestClientStatuses()
        {
            if (GameBase.Game.TimeRunning - LastStatusRequestTime < 15000)
                return;

            // Get all the users in the buffer.
            var userIds = new List<int>();

            for (var i = 0; i < UserBuffer.Count; i++)
            {
                var user = UserBuffer.ElementAt(i);

                if (user.User != null && AvailableUsers.Contains(user.User) && !userIds.Contains(user.User.OnlineUser.Id))
                    userIds.Add(user.User.OnlineUser.Id);
            }

            LastStatusRequestTime = GameBase.Game.TimeRunning;

            if (userIds.Count == 0)
                return;

            OnlineManager.Client?.RequestUserStatuses(userIds);
        }
    }
}
