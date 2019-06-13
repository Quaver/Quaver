using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Dialogs.Menu;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Steamworks;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using MathHelper = Microsoft.Xna.Framework.MathHelper;

namespace Quaver.Shared.Graphics.Online
{
    public class OnlinePlayercard : Sprite
    {
        /// <summary>
        /// </summary>
        private Sprite Background { get; }

        /// <summary>
        /// </summary>
        private Sprite Avatar { get; }

        /// <summary>
        /// </summary>
        private Sprite Flag { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Username { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap GameMode { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Status { get; }

        /// <summary>
        /// </summary>
        private Sprite LoadingWheel { get; }

        /// <summary>
        /// </summary>
        private ImageButton Button { get; }

        /// <summary>
        /// </summary>
        public OnlinePlayercard()
        {
            Size = new ScalableVector2(520, 66);
            Image = UserInterface.PlayercardBackground;

            Background = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width - 4, Height - 4),
                Image = UserInterface.PlayercardCoverDefault,
                Alpha = 0.65f
            };

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Width - 4, Height - 4),
                Alpha = 0
            };

            Button.Clicked += OnButtonClicked;
            Avatar = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(Height * 0.75f, Height * 0.75f),
                X = 12,
                Image = SteamManager.UserAvatars[SteamUser.GetSteamID().m_SteamID]
            };

            Avatar.AddBorder(Color.White, 2);

            Flag = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = -12,
                Size = new ScalableVector2(24, 24),
                X = Avatar.X + Avatar.Width + 10,
                Image = Flags.Get("XX")
            };

            Username = new SpriteTextBitmap(FontsBitmap.GothamRegular, ConfigManager.Username.Value)
            {
                Parent = Flag,
                Alignment = Alignment.MidLeft,
                X = Flag.Width + 8,
                FontSize = 20
            };

            GameMode = new SpriteTextBitmap(FontsBitmap.GothamRegular, ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value))
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-10, -8),
                FontSize = 16,
                Tint = Colors.SecondaryAccent,
            };

            Status = new SpriteTextBitmap(FontsBitmap.GothamRegular, "Offline")
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Flag.X,
                Y = Flag.Y + Flag.Height,
                FontSize = 16
            };

            LoadingWheel = new Sprite()
            {
                Parent = Status,
                Size = new ScalableVector2(16, 16),
                Image = UserInterface.LoadingWheel,
                Alignment = Alignment.MidLeft,
                X = Status.Width + 10
            };

            UpdateText();

            OnlineManager.Status.ValueChanged += OnOnlineStatusChanged;
            ConfigManager.SelectedGameMode.ValueChanged += OnSelectedGameModeChanged;
        }

        public override void Update(GameTime gameTime)
        {
            PerformLoadingWheelRotation();

            Button.Alpha = MathHelper.Lerp(Button.Alpha, Button.IsHovered ? 0.4f : 0f,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 60, 1));

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;
            ConfigManager.SelectedGameMode.ValueChanged -= OnSelectedGameModeChanged;

            base.Destroy();
        }

        private void UpdateText()
        {
            Username.Text = OnlineManager.Status.Value != ConnectionStatus.Connected
                ? ConfigManager.Username.Value
                : OnlineManager.Self.OnlineUser.Username;

            GameMode.Text = ModeHelper.ToShortHand(ConfigManager.SelectedGameMode.Value);

            switch (OnlineManager.Status.Value)
            {
                case ConnectionStatus.Disconnected:
                    Status.Text = "Offline";
                    LoadingWheel.Visible = false;
                    break;
                case ConnectionStatus.Connecting:
                    Status.Text = "Connecting. Please Wait!";
                    LoadingWheel.Visible = true;
                    break;
                case ConnectionStatus.Connected:
                    Flag.Image = Flags.Get(OnlineManager.Self.OnlineUser.CountryFlag);
                    Username.Tint = Colors.GetUserChatColor(OnlineManager.Self.OnlineUser.UserGroups);
                    Avatar.Border.Tint = Username.Tint;

                    if (OnlineManager.Self.Stats.ContainsKey(ConfigManager.SelectedGameMode.Value))
                    {
                        var stats = OnlineManager.Self.Stats[ConfigManager.SelectedGameMode.Value];
                        Status.Text = $"#{stats.Rank:n0} - {stats.OverallPerformanceRating:00.00} ({StringHelper.AccuracyToString((float) stats.OverallAccuracy)})";
                    }

                    LoadingWheel.Visible = false;
                    break;
                case ConnectionStatus.Reconnecting:
                    Status.Text = "Reconnecting. Please Wait!";
                    LoadingWheel.Visible = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
                    break;
            }

            GameMode.Visible = OnlineManager.Status.Value == ConnectionStatus.Connected;
            LoadingWheel.X = Status.Width + 10;
        }

        /// <summary>
        ///     Rotates the loading wheel endlessly
        /// </summary>
        private void PerformLoadingWheelRotation()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }

        private void OnOnlineStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
            => UpdateText();

        private void OnSelectedGameModeChanged(object sender, BindableValueChangedEventArgs<GameMode> e)
            => UpdateText();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnButtonClicked(object sender, EventArgs e)
        {
            if (OnlineManager.Status.Value != ConnectionStatus.Connected &&
                OnlineManager.Status.Value != ConnectionStatus.Disconnected)
                return;

            var options = new List<IMenuDialogOption>();

            if (OnlineManager.Status.Value == ConnectionStatus.Connected)
            {
                options.Add(new MenuDialogOption("View Profile", () =>
                    BrowserHelper.OpenURL($"https://quavergame.com/profile/{OnlineManager.Self.OnlineUser.Id}")));

                options.Add(new MenuDialogOption("Steam Profile", () => BrowserHelper.OpenURL($"https://steamcommunity.com/profiles/{OnlineManager.Self.OnlineUser.SteamId}")));
            }

            if (OnlineManager.Status.Value == ConnectionStatus.Disconnected)
                options.Add(new MenuDialogOption("Log into the server", OnlineManager.Login));

            if (OnlineManager.Status.Value == ConnectionStatus.Connected)
            {
                switch (ConfigManager.SelectedGameMode.Value)
                {
                    case API.Enums.GameMode.Keys4:
                        options.Add(new MenuDialogOption("Change game mode to 7 Keys", () => ConfigManager.SelectedGameMode.Value = API.Enums.GameMode.Keys7));
                        break;
                    case API.Enums.GameMode.Keys7:
                        options.Add(new MenuDialogOption("Change game mode to 4 Keys", () => ConfigManager.SelectedGameMode.Value = API.Enums.GameMode.Keys4));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                options.Add(new MenuDialogOption("Logout", () => OnlineManager.Client?.Disconnect()));
            }

            options.Add(new MenuDialogOption("Close", () => {}));

            var dialog = new MenuDialog("Options", options);

            DialogManager.Show(dialog);
        }
    }
}