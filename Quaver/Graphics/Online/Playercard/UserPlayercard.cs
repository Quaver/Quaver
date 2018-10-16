using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using osu_database_reader;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Helpers;
using Quaver.Online;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Graphics.Online.Playercard
{
    public class UserPlayercard : Button
    {
        /// <summary>
        ///     The type of playercard this is.
        /// </summary>
        public PlayercardType Type { get; }

        /// <summary>
        ///     The user this playercard is for.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        ///     The user's selected title.
        /// </summary>
        private Title _selectedTitle = Title.OfflinePlayer;
        public Title SelectedTitle
        {
            get => _selectedTitle;
            set
            {
                _selectedTitle = value;
                UpdateTitle(_selectedTitle);
            }
        }

        /// <summary>
        ///     The user's selected competitive badge.
        /// </summary>
        public CompetitveBadge _selectedCompetitiveBadge = CompetitveBadge.Unranked;
        public CompetitveBadge SelectedCompetitveBadge
        {
            get => _selectedCompetitiveBadge;
            set
            {
                _selectedCompetitiveBadge = value;
                UpdateCompetitiveBadge(_selectedCompetitiveBadge);
            }
        }

        /// <summary>
        ///     The user's currently selected title
        /// </summary>
        private Sprite TitleSprite { get; set; }

        /// <summary>
        ///     The user's avatar
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        ///     The user's username.
        /// </summary>
        private SpriteTextBitmap TextUsername { get; set; }

        /// <summary>
        ///     The value of the user's overall rating.
        /// </summary>
        private float _overallRating;
        public float OverallRating
        {
            get => _overallRating;
            set
            {
                _overallRating = value;
                TextOverallRating.UpdateValue(StringHelper.AccuracyToString(_overallRating).Replace("%", ""));
            }
        }

        /// <summary>
        ///     The value of the user's overall accuracy
        /// </summary>
        private float _overallAccuracy;
        public float OverallAccuracy
        {
            get => _overallAccuracy;
            set
            {
                _overallAccuracy = value;
                TextOverallAccuracy.UpdateValue(StringHelper.AccuracyToString(_overallAccuracy));
            }
        }

        /// <summary>
        ///     The value of the user's country rank for the current selected game mode.
        /// </summary>
        private int _countryRank;
        public int CountryRank
        {
            get => _countryRank;
            set
            {
                _countryRank = value;
                TextCountryRank.UpdateValue($"#{_countryRank}");
            }
        }

        /// <summary>
        ///     The value of the user's global rank for the current game mode.
        /// </summary>
        private int _globalRank;
        public int GlobalRank
        {
            get => _globalRank;
            set
            {
                _globalRank = value;
                TextGlobalRank.UpdateValue($"#{_globalRank}");
            }
        }

        /// <summary>
        ///     The user's current playcount value.
        /// </summary>
        private int _playCount;
        public int PlayCount
        {
            get => _playCount;
            set
            {
                _playCount = value;
                TextPlayCount.UpdateValue(_playCount.ToString());
            }
        }

        /// <summary>
        ///     The user's current amount of competitive wins.
        /// </summary>
        private int _competitiveMatchesWon;
        public int CompetitiveMatchesWon
        {
            get => _competitiveMatchesWon;
            set
            {
                _competitiveMatchesWon = value;
                TextCompetitiveMatchesWon.UpdateValue(_competitiveMatchesWon.ToString());
            }
        }

        /// <summary>
        ///     The game mode the user currently has selected.
        /// </summary>
        private GameMode _gameMode;
        public GameMode GameMode
        {
            get => _gameMode;
            set
            {
                _gameMode = value;

                switch (GameMode)
                {
                    case GameMode.Keys4:
                        TextGlobalRank.Icon.Image = FontAwesome.Comments;
                        break;
                    case GameMode.Keys7:
                        TextGlobalRank.Icon.Image = FontAwesome.ArrowLeft;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        ///     A badge that symbolizes the user's competitive rank.
        /// </summary>
        private Sprite CompetitiveRankBadge { get; set; }

        /// <summary>
        ///     Displays the user's overall rating.
        /// </summary>
        private IconedText TextOverallRating { get; set; }

        /// <summary>
        ///     Displays the user's overall accuracy.
        /// </summary>
        private IconedText TextOverallAccuracy { get; set; }

        /// <summary>
        ///     Displays the user's country rank.
        /// </summary>
        private IconedText TextCountryRank { get; set; }

        /// <summary>
        ///     Displays the user's global rank for the current game mode.
        /// </summary>
        private IconedText TextGlobalRank { get; set; }

        /// <summary>
        ///     Displays the user's current play count.
        /// </summary>
        private IconedText TextPlayCount { get; set; }

        /// <summary>
        ///     Displays the amount of competitive matches the user has won.
        /// </summary>
        private IconedText TextCompetitiveMatchesWon { get; set; }

        /// <summary>
        ///     Dictates if this playercard is the full thing.
        /// </summary>
        private bool FullCard { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Create a playercard from a user.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="user"></param>
        /// <param name="fullCard"></param>
        public UserPlayercard(PlayercardType type, User user, bool fullCard)
        {
            Type = type;
            User = user;

            FullCard = fullCard;
            Tint = Colors.DarkGray;

            Size = new ScalableVector2(426, FullCard ? 154 : 96);
            Image = AssetLoader.LoadTexture2D(GameBase.Game.Resources.GetStream("Textures/UI/Playercard/playercard-bg.png"));

            CreateTitle();
            CreateAvatar();
            CreateUsername(User != null ? User.OnlineUser?.Username : ConfigManager.Username.Value);
            CreateCompetitiveRankBadge();

            SelectedTitle = Title.OfflinePlayer;
            SelectedCompetitveBadge = CompetitveBadge.Unranked;

            if (FullCard)
                CreateStats();

            ConfigManager.SelectedGameMode.ValueChanged += OnSelectedGameModeChange;
            OnlineManager.Status.ValueChanged += OnOnlineStatusChanged;

            AddBorder(Color.White, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.SelectedGameMode.ValueChanged -= OnSelectedGameModeChange;

            // ReSharper disable once DelegateSubtraction
            OnlineManager.Status.ValueChanged -= OnOnlineStatusChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the sprite with the user's currently selected title.
        /// </summary>
        private void CreateTitle() => TitleSprite = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(300, 40),
            X = 10,
            Y = 10,
            Tint = Color.White,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///     Creates the sprite for the user's avatar.
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(26, 26),
                Y = TitleSprite.Y + TitleSprite.Height + 8,
                X = TitleSprite.X,
                Image = UserInterface.UnknownAvatar,
                UsePreviousSpriteBatchOptions = true
            };

            Avatar.AddBorder(Color.LightGray, 2);
        }

        /// <summary>
        ///     Updates the title on the playercard with a new texture.
        /// </summary>
        /// <param name="title"></param>
        public void UpdateTitle(Title title)
        {
            TitleSprite.Transformations.Clear();
            TitleSprite.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 500));
            TitleSprite.Image = TitleHelper.Get(title);
        }

        /// <summary>
        ///     Creates the sprite that shows the user's username.
        /// </summary>
        private void CreateUsername(string username)
        {
            TextUsername = new SpriteTextBitmap(BitmapFonts.Exo2Bold, " ", 24,
                Color.White, Alignment.MidCenter, int.MaxValue)
            {
                Parent = Avatar,
                X = Avatar.Width + 5,
                Y = 1,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
            };

            UpdateUsername(username);
        }

        /// <summary>
        ///     Creates the sprite to show the user's rank badge.
        /// </summary>
        private void CreateCompetitiveRankBadge() => CompetitiveRankBadge = new Sprite()
        {
            Parent = this,
            UsePreviousSpriteBatchOptions = true,
            X = TitleSprite.X + TitleSprite.Width + 24,
            Y = TitleSprite.Y,
            Size = new ScalableVector2(70, 70),
        };

         /// <summary>
        ///     Creates the user stats w/ icons.
        /// </summary>
        private void CreateStats()
        {
            TextOverallRating = new IconedText(FontAwesome.BarGraph, "00.00")
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Avatar.X,
                Y = Avatar.Y + Avatar.Height + 10
            };

            TextCountryRank = new IconedText(Flags.Get(User.CountryName), "#9,999,999")
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.TopCenter,
                X = -10,
                Y = TextOverallRating.Y,
            };

            TextGlobalRank = new IconedText(FontAwesome.Desktop, "#9,999,999")
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                UsePreviousSpriteBatchOptions = true,
                X = -Avatar.X,
                Y = TextOverallRating.Y
            };

            TextOverallAccuracy = new IconedText(FontAwesome.Clock, "100.00%")
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                X = Avatar.X,
                Y = TextOverallRating.Y + TextOverallRating.Height + 10
            };

            TextPlayCount = new IconedText(FontAwesome.GamePad, "1,000,000")
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.TopCenter,
                X = -10,
                Y = TextOverallAccuracy.Y
            };

            TextCompetitiveMatchesWon = new IconedText(FontAwesome.Trophy, "1,000,000")
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.TopRight,
                X = -Avatar.X,
                Y = TextOverallAccuracy.Y
            };

            TextCompetitiveMatchesWon.Icon.Tint = Color.Gold;
            SetStats();
        }

        /// <summary>
        ///     Updates the avatar and performs an animation
        /// </summary>
        public void UpdateAvatar(Texture2D tex)
        {
            Avatar.Transformations.Clear();
            Avatar.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 500));
            Avatar.Image = tex;
        }

        /// <summary>
        ///     Updates the username text.
        /// </summary>
        /// <param name="username"></param>
        public void UpdateUsername(string username)
        {
            TextUsername.Text = username;
            TextUsername.Size = new ScalableVector2(TextUsername.Width * 0.60f, TextUsername.Height * 0.60f);
        }

        /// <summary>
        ///     Updates the competitive badge for the user.
        /// </summary>
        private void UpdateCompetitiveBadge(CompetitveBadge badge)
        {
            CompetitiveRankBadge.Transformations.Clear();
            CompetitiveRankBadge.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 500));
            CompetitiveRankBadge.Image = CompetitiveBadgeHelper.Get(badge);
        }

        /// <summary>
        ///     Updates the country flag for the user.
        /// </summary>
        public void UpdateFlag(string countryName)
        {
            TextCountryRank.Icon.Transformations.Clear();
            TextCountryRank.Icon.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 500));
            TextCountryRank.Icon.Image = Flags.Get(countryName);
        }

        /// <summary>
        ///     Handles the animation that occurs when the playercard is hovered.
        /// </summary>
        private void HandleHoverAnimation(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;
            var targetColor = IsHovered ? Colors.MainAccent : Color.White;

            Border.FadeToColor(targetColor, dt, 60);
        }

        /// <summary>
        ///     Called when the selected game mode has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedGameModeChange(object sender, BindableValueChangedEventArgs<GameMode> e)
        {
            // Only update when it's self.
            if (TextUsername.Text != ConfigManager.Username.Value || !FullCard)
                return;

            SetStats();
        }

        /// <summary>
        ///     Sets the users stats on the playercard.
        /// </summary>
        private void SetStats()
        {
            GameMode = ConfigManager.SelectedGameMode.Value;
            OverallRating = (float) User.Stats[GameMode].OverallPerformanceRating;
            OverallAccuracy = (float) User.Stats[GameMode].OverallAccuracy;
            CountryRank = User.Stats[GameMode].CountryRank;
            GlobalRank = User.Stats[GameMode].Rank;
            PlayCount = User.Stats[GameMode].PlayCount;
            CompetitiveMatchesWon = 0;
        }

        /// <summary>
        ///     Called when the user's online status has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnOnlineStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> e)
        {
            if (Type != PlayercardType.Self)
                return;

            switch (e.Value)
            {
                case ConnectionStatus.Connected:
                    User = OnlineManager.Self;

                    FullCard = true;

                    if (TextOverallRating == null)
                        CreateStats();

                    SetStats();
                    ShowStats();

                    Transformations.Clear();
                    Transformations.Add(new Transformation(TransformationProperty.Height, Easing.EaseOutQuint, Height, 154, 150));
                    break;
                case ConnectionStatus.Disconnected:
                    FullCard = false;

                    if (TextOverallRating == null)
                        return;

                    HideStats();

                    Transformations.Clear();
                    Transformations.Add(new Transformation(TransformationProperty.Height, Easing.EaseOutQuint, Height, 96, 150));
                    break;
            }
        }

        /// <summary>
        ///     Hides the stats portion of the playercard.
        /// </summary>
        private void HideStats()
        {
            TextOverallRating.Visible = false;
            TextOverallAccuracy.Visible = false;
            TextCountryRank.Visible = false;
            TextGlobalRank.Visible = false;
            TextPlayCount.Visible = false;
            TextCompetitiveMatchesWon.Visible = false;
        }

        /// <summary>
        ///     Shows the stats portion of the playercard.
        /// </summary>
        private void ShowStats()
        {
            TextOverallRating.Visible = true;
            TextOverallAccuracy.Visible = true;
            TextCountryRank.Visible = true;
            TextGlobalRank.Visible = true;
            TextPlayCount.Visible = true;
            TextCompetitiveMatchesWon.Visible = true;
        }
    }

    public enum PlayercardType
    {
        Self,
        Other
    }
}