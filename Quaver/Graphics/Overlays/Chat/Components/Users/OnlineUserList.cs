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
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();
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