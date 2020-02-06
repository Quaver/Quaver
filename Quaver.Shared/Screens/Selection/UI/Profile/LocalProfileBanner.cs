using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Profiles;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Steamworks;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Profile
{
    public class LocalProfileBanner : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private Bindable<UserProfile> Profile { get; }

        /// <summary>
        /// </summary>
        private Sprite BackgroundImage { get; set; }

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
        private SpriteTextPlus ProfileType { get; set; }

        /// <summary>
        /// </summary>
        private IconButton ModeButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton DeleteButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton ViewScoresButton { get; set; }

        /// <summary>
        /// </summary>
        private GameMode ActiveMode => ConfigManager.SelectedGameMode?.Value ?? GameMode.Keys4;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="size"></param>
        public LocalProfileBanner(Bindable<UserProfile> profile, ScalableVector2 size) : base(size, size)
        {
            Profile = profile;

            SetChildrenAlpha = true;
            Alpha = 0;

            CreateBackground();
            CreateAvatar();
            CreateFlag();
            CreateUsername();
            CreateProfileType();
            CreateModeButton();
            CreateDeleteButton();
            CreateViewScoresButton();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            BackgroundImage = new BackgroundImage(UserInterface.MenuBackgroundClear, 75, false)
            {
                Alignment = Alignment.BotLeft,
                X = -100,
                Size = new ScalableVector2(1152, 648),
            };

            AddContainedDrawable(BackgroundImage);
        }

        /// <summary>
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite()
            {
                Parent = this,
                Position = new ScalableVector2(18, 28),
                Size = new ScalableVector2(56, 56),
                Image = UserInterface.UnknownAvatar,
                Alpha = 0
            };

            if (SteamManager.UserAvatars != null)
            {
                var id = SteamUser.GetSteamID().m_SteamID;

                if (SteamManager.UserAvatars.ContainsKey(id))
                    Avatar.Image = SteamManager.UserAvatars[id];
            }

            Avatar.AddBorder(Color.Transparent, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateFlag()
        {
            Flag = new Sprite
            {
                Parent = this,
                X = Avatar.X + Avatar.Width + Avatar.Border.Thickness + 12,
                Y = Avatar.Y + 4,
                Size = new ScalableVector2(24, 24),
                Image = Flags.Get(OnlineManager.Self?.OnlineUser?.CountryFlag) ?? Flags.Get("XX"),
                Alpha = 0,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                Profile.Value.Username ?? ConfigManager.Username?.Value ?? "Player", 24)
            {
                Parent = this,
                X = Avatar.X + Avatar.Width + Avatar.Border.Thickness + 12,
                Y = Avatar.Y + 4,
                Alpha = 0
            };

            if (Profile.Value.IsOnline)
            {
                Flag.Visible = true;
                Username.X = Flag.X + Flag.Width + 8;
            }
        }

        /// <summary>
        /// </summary>
        private void CreateProfileType()
        {
            var typeStr = "Local Profile";

            if (Profile.Value.IsOnline)
                typeStr = "Online Profile";

            ProfileType = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), typeStr, 20)
            {
                Parent = this,
                Y = Flag.Y + Flag.Height + 6,
                X = Flag.X,
                Tint = ColorHelper.HexToColor($"#10C8F6")
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
                Y = Avatar.Y + Avatar.Height + 26,
                Image = GetModeImage(ActiveMode),
                Size = new ScalableVector2(89, 25)
            };

            ModeButton.Clicked += (sender, args) =>
            {
                GameMode mode;

                switch (ActiveMode)
                {
                    case GameMode.Keys4:
                        mode = GameMode.Keys7;
                        break;
                    case GameMode.Keys7:
                        mode = GameMode.Keys4;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (ConfigManager.SelectedGameMode != null)
                    ConfigManager.SelectedGameMode.Value = mode;

                ModeButton.Image = GetModeImage(mode);
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDeleteButton()
        {
            DeleteButton = new IconButton(UserInterface.DeleteButton)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(76, 25),
                Position = new ScalableVector2(-Avatar.X, ModeButton.Y),
            };

            DeleteButton.Clicked += (sender, args) =>
            {
                if (Profile.Value.Id == 0)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You cannot delete a default profile!");
                    return;
                }

                DialogManager.Show(new DeleteProfileDialog(Profile));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateViewScoresButton()
        {
            ViewScoresButton = new IconButton(UserInterface.ViewScoresButton)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(110, 25),
                Position = new ScalableVector2(DeleteButton.X - DeleteButton.Width - 12, DeleteButton.Y),
            };

            ViewScoresButton.Clicked += (sender, args) =>
            {
                if (!Profile.Value.IsOnline)
                {
                    NotificationManager.Show(NotificationLevel.Warning,
                        "Not implemented yet. You can only the scores the 'Online' profile at the moment!");
                    return;
                }

                BrowserHelper.OpenURL($"https://quavergame.com/profile/{OnlineManager.Self.OnlineUser.Id}" +
                                      $"?mode={(int) ConfigManager.SelectedGameMode.Value}");
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private Texture2D GetModeImage(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Keys4:
                    return UserInterface.Mode4KOn;
                case GameMode.Keys7:
                    return UserInterface.Mode7KOn;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}