using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using SQLite;
using TimeAgo;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Discord.RPC;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Window;
using MathHelper = Quaver.API.Helpers.MathHelper;

namespace Quaver.Shared.Screens.Selection.UI.Leaderboard.Components
{
    public class DrawableLeaderboardScoreContainer : Sprite
    {
        /// <summary>
        ///     The parent leaderboard score
        /// </summary>
        private DrawableLeaderboardScore Score { get; set; }

        /// <summary>
        ///     The amount of padding from the left side that the elements will begin
        /// </summary>
        private int PaddingLeft { get; } = 20;

        /// <summary>
        ///     Makes the leaderboard score clickable/hoverable
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        ///     Displays the rank of the score
        /// </summary>
        private SpriteTextPlus Rank { get; set; }

        /// <summary>
        ///     The grade the user achieved on the score
        /// </summary>
        private Sprite Grade { get; set; }

        /// <summary>
        ///     The user's avatar from the score
        /// </summary>
        private Sprite Avatar { get; set; }

        /// <summary>
        ///     Displays the username of the player
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        ///     Displays the performance rating of the score
        /// </summary>
        private SpriteTextPlus PerformanceRating { get; set; }

        /// <summary>
        ///     Displays the accuracy% and max combo the user achieved on the score
        /// </summary>
        private SpriteTextPlus AccuracyMaxCombo { get; set; }

        /// <summary>
        ///     Displays the mods the player used in the play
        /// </summary>
        private SpriteTextPlus Mods { get; set; }

        /// <summary>
        ///     The y position of the username
        /// </summary>
        private float UsernameY { get; } = 6;

        /// <summary>
        ///     A sprite displayed when the score can't be beaten with the activated mods
        /// </summary>
        private IconButton CantBeatAlert { get; set; }

        /// <summary>
        ///     The x position of <see cref="PerformanceRating"/> when the score is able to be beaten
        /// </summary>
        private int BeatablePerformanceRatingX { get; } = -12;

        /// <summary>
        ///     Returns the background color of the table
        /// </summary>
        private Color BackgroundColor => Score.Index % 2 == 0 ? ColorHelper.HexToColor("#363636") : ColorHelper.HexToColor("#242424");

        /// <summary>
        ///     Tooltip that displays when hovering over <see cref="CantBeatAlert"/>
        /// </summary>
        private Tooltip UnbeatableTooltip { get; set; }

        /// <summary>
        ///     An icon to represent the time the score was played at
        /// </summary>
        private Sprite Clock { get; set; }

        /// <summary>
        ///     How much time ago the score was submitted
        /// </summary>
        private SpriteTextPlus Time { get; set; }

        /// <summary>
        ///     The modifiers that the player is using on the score
        /// </summary>
        private List<DrawableModifier> Modifiers { get; set; }

        /// <summary>
        ///     Displays the flag of the user
        /// </summary>
        private Sprite Flag { get; set; }

        /// <summary>
        ///     A blank texture2D image
        /// </summary>
        private Texture2D BlankImage { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        public DrawableLeaderboardScoreContainer(DrawableLeaderboardScore score)
        {
            Score = score;
            Size = Score.Size;

            if (Score.Item.IsEmptyScore)
                return;

            CreateButton();
            CreateGrade();
            CreateAvatar();

            if (!Score.IsPersonalBest)
                CreateRankText();

            CreateFlag();
            CreateUsername();
            CreatePerformanceRating();
            CreateAccuracyMaxCombo();
            CreateMods();
            CreateCantBeatAlert();
            CreateTime();

            SteamManager.SteamUserAvatarLoaded += OnSteamAvatarLoaded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="score"></param>
        public void UpdateContent(DrawableLeaderboardScore score)
        {
            Score = score;
            Tint = BackgroundColor;

            // Empty scores don't need to update its state
            if (Score.Item.IsEmptyScore)
                return;

            Tint = Button.IsHovered || CantBeatAlert.IsHovered ? ColorHelper.HexToColor("#575757"): BackgroundColor;

            // Ranks don't show on PB scores.
            if (!Score.IsPersonalBest)
                Rank.Text = $"{Score.Index + 1}.";

            Username.Text = $"{score.Item.Name}";
            Username.Tint = score.Item.Name == ConfigManager.Username.Value ? Colors.MainAccent : ColorHelper.HexToColor("#FBFFB6");

            PerformanceRating.Text = StringHelper.RatingToString(score.Item.PerformanceRating);
            AccuracyMaxCombo.Text = $"{score.Item.MaxCombo:N0}x | {StringHelper.AccuracyToString((float) score.Item.Accuracy)}";
            Grade.Image = SkinManager.Skin?.Grades[GradeHelper.GetGradeFromAccuracy((float) score.Item.Accuracy)] ?? UserInterface.Logo;

            UpdateTime();
            UpdateModifiers();
            UpdateAvatar();
            UpdateCantBeatAlert();
            UpdateFlag();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            UnbeatableTooltip?.Destroy();
            BlankImage?.Dispose();

            // ReSharper disable once DelegateSubtraction
            SteamManager.SteamUserAvatarLoaded -= OnSteamAvatarLoaded;

            base.Destroy();
        }

        /// <summary>
        ///     Creates <see cref="Button"/>
        /// </summary>
        private void CreateButton()
        {
            Button = new ImageButton(WobbleAssets.WhiteBox)
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Depth = 1,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="Rank"/>
        /// </summary>
        private void CreateRankText()
        {
            Rank = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "10.", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = PaddingLeft,
                UsePreviousSpriteBatchOptions = true,
                Alpha = 0
            };
        }

        /// <summary>
        ///     Creates <see cref="Avatar"/>
        /// </summary>
        private void CreateAvatar()
        {
            BlankImage = new Texture2D(GameBase.Game.GraphicsDevice, 1, 1);

            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Grade.X + Grade.Width + 15,
                Size = new ScalableVector2(45, 45),
                UsePreviousSpriteBatchOptions = true,
                Image = BlankImage,
                Alpha = 0
            };
        }

        /// <summary>
        ///     Creates <see cref="Grade"/>
        /// </summary>
        private void CreateGrade()
        {
            Grade = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(40, 40),
                X = Score.IsPersonalBest ? PaddingLeft : 60,
                UsePreviousSpriteBatchOptions = true,
            };
        }

        /// <summary>
        ///     Creates <see cref="Flag"/>
        /// </summary>
        private void CreateFlag()
        {
            Flag = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(Avatar.X + Avatar.Width + PaddingLeft / 2f, UsernameY + 4),
                UsePreviousSpriteBatchOptions = true,
                Size = new ScalableVector2(24, 24),
                Image = Flags.Get("XX")
            };
        }

        /// <summary>
        ///     Creates <see cref="Username"/>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Player", 24)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(Flag.X + Flag.Width +PaddingLeft / 4f, UsernameY + 4),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="PerformanceRating"/>
        /// </summary>
        private void CreatePerformanceRating()
        {
            PerformanceRating = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "00.00", 28)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = 6,
                X = BeatablePerformanceRatingX,
                Tint = ColorHelper.HexToColor("#E9B736"),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="AccuracyMaxCombo"/>
        /// </summary>
        private void CreateAccuracyMaxCombo()
        {
            AccuracyMaxCombo = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "00.00% | 0,000x", 21)
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                X = PerformanceRating.X,
                Y = -PerformanceRating.Y,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="Mods"/>
        /// </summary>
        private void CreateMods()
        {
            Mods = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 18)
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                X = Username.X,
                Y = -Username.Y,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="CantBeatAlert"/>
        /// </summary>
        private void CreateCantBeatAlert()
        {
            CantBeatAlert = new IconButton(UserInterface.WarningRed)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(20, 20),
                UsePreviousSpriteBatchOptions = true,
                Y = PerformanceRating.Y + 5,
                X = BeatablePerformanceRatingX
            };

            UnbeatableTooltip = new Tooltip("You cannot beat this score with your currently activated modifiers!", Color.Crimson);

            CantBeatAlert.Hovered += (sender, args) =>
            {
                var container = Score.Container as LeaderboardScoresContainer;
                container?.ActivateTooltip(UnbeatableTooltip);
            };

            CantBeatAlert.LeftHover += (sender, args) => UnbeatableTooltip.Parent = null;
        }

        /// <summary>
        ///     Creates <see cref="Time"/>
        /// </summary>
        private void CreateTime()
        {
            Clock = new Sprite
            {
                Parent = Username,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                Image = UserInterface.Clock,
                Size = new ScalableVector2(12, 12)
            };

            Time = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 18)
            {
                Parent = Clock,
                Alignment = Alignment.MidLeft,
                X = Clock.Width + 2,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Sets <see cref="Time"/>'s text to the correct time ago
        /// </summary>
        private void UpdateTime()
        {
            if (Time == null)
                return;

            var date = DateTime.Parse(Score.Item.DateTime);
            var timeDifference = DateTime.Now - date;

            // Years
            if ((int) timeDifference.TotalDays > 365)
                Time.Text = $"{(int) (timeDifference.TotalDays / 365)}y";
            // Months
            else if ((int) timeDifference.TotalDays > 30)
                Time.Text = $"{(int) (timeDifference.TotalDays / 30)}mo";
            // Weeks
            else if ((int) timeDifference.TotalDays > 7)
                Time.Text = $"{(int) (timeDifference.TotalDays / 7)}w";
            // Days
            else if ((int) timeDifference.TotalDays > 0)
                Time.Text = $"{(int) timeDifference.TotalDays}d";
            // Hours
            else if ((int) timeDifference.TotalHours > 0)
                Time.Text = $"{(int) timeDifference.TotalHours}h";
            // Minutes
            else if ((int) timeDifference.TotalMinutes > 0)
                Time.Text = $"{(int) timeDifference.TotalMinutes}m";
            // Seconds
            else
                Time.Text = $"{(int) timeDifference.TotalSeconds}s";

            Clock.Parent = Username;
            Clock.Alignment = Alignment.MidLeft;
            Clock.X = Username.Width + 8;

            Time.Parent = Clock;
            Time.Alignment = Alignment.MidLeft;
            Time.X = Clock.Width + 2;

            Time.Tint = timeDifference.TotalMilliseconds < 86400000 ? ColorHelper.HexToColor("#6EF7F7") : ColorHelper.HexToColor("#808080");
            Clock.Tint = Time.Tint;
        }

        /// <summary>
        ///     Performs an animation when hovered over the button
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimation(GameTime gameTime)
        {
            if (Button == null)
                return;

            var color = Button.IsHovered || CantBeatAlert.IsHovered ? ColorHelper.HexToColor("#575757"): BackgroundColor;
            FadeToColor(color, gameTime.ElapsedGameTime.TotalMilliseconds, 30);
        }

        /// <summary>
        ///     Creates and updates <see cref="Modifiers"/>
        /// </summary>
        private void UpdateModifiers()
        {
            Modifiers?.ForEach(x => x.Destroy());
            Modifiers?.Clear();

            Modifiers = new List<DrawableModifier>();

            var modsList = ModManager.GetModsList((ModIdentifier) Score.Item.Mods);

            if (modsList.Count == 0)
                modsList.Add(ModIdentifier.None);

            for (var i = 0; i < modsList.Count; i++)
            {
                try
                {
                    const int width = 52;
                    const int height = 26;

                    var mod = new DrawableModifier(modsList[i])
                    {
                        Parent = this,
                        Alignment = Alignment.BotLeft,
                        X = Flag.X + width * Modifiers.Count - 4,
                        Y = AccuracyMaxCombo.Y,
                        UsePreviousSpriteBatchOptions = true,
                        Size = new ScalableVector2(width, height),
                        Alpha = 1
                    };

                    Modifiers.Add(mod);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        /// </summary>
        private void UpdateAvatar()
        {
            var steamId = (ulong) Score.Item.SteamId;

            lock (Avatar)
            lock (Avatar.Image)
            {
                if (Score.IsPersonalBest && !Score.Item.IsOnline)
                {
                    Avatar.Image = UserInterface.UnknownAvatar;
                    Avatar.Alpha = 1;
                    return;
                }

                if (SteamManager.UserAvatars.ContainsKey(steamId))
                {
                    if (Avatar.Image == SteamManager.UserAvatars[steamId])
                        return;

                    Avatar.Alpha = 0;
                    Avatar.ClearAnimations();
                    Avatar.FadeTo(1, Easing.Linear, 400);

                    Avatar.Image = SteamManager.UserAvatars[steamId];
                    return;
                }

                Avatar.Image = UserInterface.UnknownAvatar;
                Avatar.ClearAnimations();
                Avatar.Alpha = 0;
            }

            SteamManager.SendAvatarRetrievalRequest(steamId);
        }

        /// <summary>
        ///     Updates the state of <see cref="CantBeatAlert"/>
        /// </summary>
        private void UpdateCantBeatAlert()
        {
            // Handle if it is impossible to beat this score with the currently activated mods
            if (new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods)).CalculateRating(100) >=
                Score.Item.PerformanceRating || Score.IsPersonalBest)
            {
                CantBeatAlert.Visible = false;
            }
            else
            {
                CantBeatAlert.X = PerformanceRating.X - PerformanceRating.Width - 10;
                CantBeatAlert.Visible = true;
            }
        }

        /// <summary>
        ///     Updates the state of <see cref="Flag"/>
        /// </summary>
        private void UpdateFlag()
        {
            // Get user's current country
            if (!Score.Item.IsOnline)
            {
                try
                {
                    var regionInfo = RegionInfo.CurrentRegion;
                    var name = regionInfo.Name;

                    Flag.Image = Flags.Get(name);
                }
                catch (Exception)
                {
                    Flag.Image = Flags.Get("XX");
                }
                return;
            }

            try
            {
                Flag.Image = Flags.Get(Score.Item.Country);
            }
            catch (Exception)
            {
                Flag.Image = Flags.Get("XX");
            }
        }

        /// <summary>
        ///     Called when a user's steam avatar has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSteamAvatarLoaded(object sender, SteamAvatarLoadedEventArgs e)
        {
            if (e.SteamId != (ulong) Score.Item.SteamId)
                return;

            lock (Avatar)
            lock (Avatar.Image)
            {
                Avatar.Alpha = 0;
                Avatar.ClearAnimations();
                Avatar.FadeTo(1, Easing.Linear, 400);
                Avatar.Image = e.Texture;
            }
        }

        /// <summary>
        ///     Fades all of the objects in from zero
        /// </summary>
        public void FadeIn()
        {
            const int targetAlpha = 1;
            var time = 200;
            const Easing easing = Easing.Linear;

            Alpha = 0;

            if (!Score.Item.IsEmptyScore)
            {
                Username.Alpha = 0;
                Grade.Alpha = 0;
                Time.Alpha = 0;
                Clock.Alpha = 0;
                PerformanceRating.Alpha = 0;
                AccuracyMaxCombo.Alpha = 0;
                Flag.Alpha = 0;
                Rank.Alpha = 0;
            }

            FadeTo(targetAlpha, easing, time);

            if (!Score.Item.IsEmptyScore)
            {
                Username.FadeTo(targetAlpha, easing, time);
                Grade.FadeTo(targetAlpha, easing, time);
                Time.FadeTo(targetAlpha, easing, time);
                Clock.FadeTo(targetAlpha, easing, time);
                PerformanceRating.FadeTo(targetAlpha, easing, time);
                AccuracyMaxCombo.FadeTo(targetAlpha, easing, time);
                Flag.FadeTo(targetAlpha, easing, time);
                Rank.FadeTo(targetAlpha, easing, time);

                Modifiers.ForEach(x =>
                {
                    x.Alpha = 0;
                    x.FadeTo(targetAlpha, easing, time);
                });
            }
        }
    }
}