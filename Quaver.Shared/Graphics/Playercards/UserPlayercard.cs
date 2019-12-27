using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using SQLite;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Playercards
{
    public class UserPlayercard : Sprite
    {
        /// <summary>
        /// </summary>
        private User User { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Status { get; set; }

        /// <summary>
        /// </summary>
        public IconButton ModeButton { get; private set; }

        /// <summary>
        /// </summary>
        public IconButton LogoutButton { get; private set; }

        /// <summary>
        /// </summary>
        private TextKeyValue GlobalRanking { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue OverallRating { get; set; }

        /// <summary>
        /// </summary>
        private TextKeyValue OverallAccuracy { get; set; }

        /// <summary>
        /// </summary>
        private GameMode ActiveMode { get; set; } = GameMode.Keys4;

        /// <summary>
        /// </summary>
        /// <param name="user"></param>
        public UserPlayercard(User user)
        {
            User = user;
            Size = new ScalableVector2(436, 215);
            Image = UserInterface.UserPlayercardPanel;

            CreateAvatar();
            CreateFlag();
            CreateUsername();
            CreateStatus();
            CreateModeButton();
            CreateLogoutButton();
            CreateGlobalRanking();
            CreateOverallRating();
            CreateOverallAccuracy();

            if (ConfigManager.SelectedGameMode != null)
                ActiveMode = ConfigManager.SelectedGameMode.Value;

            UpdateState();

            SubscribeToEvents();
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (OnlineManager.Self != User)
            {
                User = OnlineManager.Self;
                UpdateState();
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            UnsubscribeFromEvents();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (e.Value == ConnectionStatus.Connected)
            {
                UnsubscribeFromEvents();
                SubscribeToEvents();
            }
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(46, 46),
                Position = new ScalableVector2(20, 16)
            };

            Avatar.AddBorder(Colors.GetUserChatColor(User?.OnlineUser?.UserGroups ?? UserGroups.Normal), 2);
        }

        /// <summary>
        /// </summary>
        private void CreateFlag()
        {
            Flag = new Sprite()
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(22, 22),
                Position = new ScalableVector2(Avatar.X + Avatar.Width + 10, Avatar.Y)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Flag.X + Flag.Width + 6,
                Y = Flag.Y
            };
        }

        /// <summary>
        /// </summary>
        private void CreateStatus()
        {
            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Flag.X,
                Y = Flag.Y + Avatar.Height - 20,
                Tint = ColorHelper.HexToColor("#808080")
            };
        }

        /// <summary>
        /// </summary>
        private void CreateModeButton()
        {
            ModeButton = new IconButton(UserInterface.BlankBox)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Avatar.X,
                Y = Avatar.Y + Avatar.Height + 18,
                Size = new ScalableVector2(78 * 1.1f, 20 * 1.1f)
            };

            ModeButton.Clicked += (sender, args) =>
            {
                switch (ActiveMode)
                {
                    case GameMode.Keys4:
                        ActiveMode = GameMode.Keys7;
                        break;
                    case GameMode.Keys7:
                        ActiveMode = GameMode.Keys4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (ConfigManager.SelectedGameMode != null && User == OnlineManager.Self)
                    ConfigManager.SelectedGameMode.Value = ActiveMode;

                UpdateState();
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLogoutButton()
        {
            LogoutButton = new IconButton(UserInterface.LogoutButtonPlayercard)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                UsePreviousSpriteBatchOptions = true,
                X = -Avatar.X,
                Y = ModeButton.Y,
                Size = new ScalableVector2(61 * 1.1f, 20 * 1.1f),
            };

            LogoutButton.Clicked += (sender, args) =>
            {
                ThreadScheduler.Run(() => OnlineManager.Client?.Disconnect());
            };
        }

        /// <summary>
        /// </summary>
        private void CreateGlobalRanking()
        {
            GlobalRanking = new TextKeyValue("Global Rank", "#1", 20, Color.White)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Key = {UsePreviousSpriteBatchOptions = true},
                Value =
                {
                    Parent = this,
                    Alignment = Alignment.TopRight,
                    X = -Avatar.X,
                    UsePreviousSpriteBatchOptions = true,
                },
                X = Avatar.X,
                Y = ModeButton.Y + ModeButton.Height + 26
            };

            GlobalRanking.Value.Y = GlobalRanking.Y;
        }

        /// <summary>
        /// </summary>
        private void CreateOverallRating()
        {
            OverallRating = new TextKeyValue("Overall Rating", "0.00", 20, Color.White)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Key = {UsePreviousSpriteBatchOptions = true},
                Value =
                {
                    Parent = this,
                    Alignment = Alignment.TopRight,
                    X = -Avatar.X,
                    UsePreviousSpriteBatchOptions = true,
                },
                X = Avatar.X,
                Y = GlobalRanking.Y + GlobalRanking.Height + 8
            };

            OverallRating.Value.Y = OverallRating.Y;
        }

        /// <summary>
        /// </summary>
        private void CreateOverallAccuracy()
        {
            OverallAccuracy = new TextKeyValue("Overall Accuracy", "99.99%", 20, Color.White)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Key = {UsePreviousSpriteBatchOptions = true},
                Value =
                {
                    Parent = this,
                    Alignment = Alignment.TopRight,
                    X = -Avatar.X,
                    UsePreviousSpriteBatchOptions = true,
                },
                X = Avatar.X,
                Y = OverallRating.Y + OverallRating.Height + 8
            };

            OverallAccuracy.Value.Y = OverallAccuracy.Y;
        }

        /// <summary>
        /// </summary>
        private void UpdateState() => ScheduleUpdate(() =>
        {
            Avatar.Image = UserInterface.UnknownAvatar;

            if (User != null && SteamManager.UserAvatars != null && SteamManager.UserAvatars.ContainsKey((ulong) User.OnlineUser.SteamId))
                Avatar.Image = SteamManager.UserAvatars[(ulong) User.OnlineUser.SteamId];

            Avatar.Border.Tint = Colors.GetUserChatColor(User?.OnlineUser?.UserGroups ?? UserGroups.Normal);

            Flag.Image = User != null ? Flags.Get(User?.OnlineUser?.CountryFlag) : Flags.Get("XX");

            Username.Text = User?.OnlineUser?.Username ?? "Player";
            Username.Tint = Avatar.Border.Tint;
            Username.TruncateWithEllipsis((int) Width - 30);

            Status.Text = "Online";
            ModeButton.Image = GetModeImage();

            if (User != null && User.Stats.ContainsKey(ActiveMode))
            {
                GlobalRanking.Value.Text = $"#{User.Stats[ActiveMode].Rank:n0}";
                OverallRating.Value.Text = StringHelper.RatingToString(User.Stats[ActiveMode].OverallPerformanceRating);
                OverallAccuracy.Value.Text = StringHelper.AccuracyToString((float) User.Stats[ActiveMode].OverallAccuracy);
            }
        });

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Texture2D GetModeImage()
        {
            switch (ActiveMode)
            {
                case GameMode.Keys4:
                    return UserInterface.Mode4KOn;
                case GameMode.Keys7:
                    return UserInterface.Mode7KOn;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private string GetStatusText()
        {
            if (User?.CurrentStatus == null)
                return "Idle";

            switch (User?.CurrentStatus?.Status)
            {
                case ClientStatus.InMenus:
                    return "Idle";
                case ClientStatus.Selecting:
                    return "Selecting a song";
                case ClientStatus.Playing:
                case ClientStatus.Paused:
                    return "Playing";
                case ClientStatus.Watching:
                    return "Watching a replay";
                case ClientStatus.Editing:
                    return "Editing";
                case ClientStatus.InLobby:
                    return "Multiplayer Lobby";
                case ClientStatus.Multiplayer:
                    return "Playing multiplayer";
                case ClientStatus.Listening:
                    return "Listening";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoginSuccess(object sender, LoginReplyEventArgs e)
        {
            User = e.Self;
            UpdateState();
        }

        /// <summary>
        /// </summary>
        private void SubscribeToEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnLoginSuccess += OnLoginSuccess;
            OnlineManager.Client.OnUserStatusReceived += OnUserStatusReceived;
        }

        /// <summary>
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (OnlineManager.Client == null)
                return;

            OnlineManager.Client.OnLoginSuccess -= OnLoginSuccess;
            OnlineManager.Client.OnUserStatusReceived -= OnUserStatusReceived;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserStatusReceived(object sender, UserStatusEventArgs e) => UpdateState();
    }
}