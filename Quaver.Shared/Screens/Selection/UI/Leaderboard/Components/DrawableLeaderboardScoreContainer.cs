using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Rating;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using TimeAgo;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;

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
        private float UsernameY { get; } = 8;

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
        /// </summary>
        /// <param name="score"></param>
        public DrawableLeaderboardScoreContainer(DrawableLeaderboardScore score)
        {
            Score = score;
            Size = Score.Size;

            if (Score.Item.IsEmptyScore)
                return;

            CreateButton();

            CreateAvatar();

            if (!Score.IsPersonalBest)
                CreateRankText();

            CreateGrade();
            CreateUsername();
            CreatePerformanceRating();
            CreateAccuracyMaxCombo();
            CreateMods();
            CreateCantBeatAlert();
            CreateTime();
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

            if (Score.Item.IsEmptyScore)
                return;

            if (!Score.IsPersonalBest)
                Rank.Text = $"{Score.Index + 1}.";

            Username.Text = $"{score.Item.Name}";
            Username.Tint = score.Item.Name == ConfigManager.Username.Value ? Colors.MainAccent : ColorHelper.HexToColor("#FBFFB6");

            UpdateTime();
            PerformanceRating.Text = StringHelper.RatingToString(score.Item.PerformanceRating);

            // Handle if it is impossible to beat this score with the currently activated mods
            var processor = new RatingProcessorKeys(MapManager.Selected.Value.DifficultyFromMods(ModManager.Mods));

            if (processor.CalculateRating(100) >= score.Item.PerformanceRating || score.IsPersonalBest)
            {
                CantBeatAlert.Visible = false;
            }
            else
            {
                CantBeatAlert.X = PerformanceRating.X - PerformanceRating.Width - 10;
                CantBeatAlert.Visible = true;
            }

            AccuracyMaxCombo.Text = $"{score.Item.MaxCombo:N0}x | {StringHelper.AccuracyToString((float) score.Item.Accuracy)}";
            Grade.Image = SkinManager.Skin?.Grades[GradeHelper.GetGradeFromAccuracy((float) score.Item.Accuracy)] ?? UserInterface.Logo;
            UpdateModifiers();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            UnbeatableTooltip?.Destroy();

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
            Rank = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "10.", 24)
            {
                Parent = Avatar,
                Alignment = Alignment.MidCenter,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        ///     Creates <see cref="Avatar"/>
        /// </summary>
        private void CreateAvatar()
        {
            Avatar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = PaddingLeft,
                Size = new ScalableVector2(45, 45),
                UsePreviousSpriteBatchOptions = true,
                Image = UserInterface.YouAvatar
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
                Size = new ScalableVector2(45, 45),
                X = Avatar.X + Avatar.Width + 15,
                UsePreviousSpriteBatchOptions = true,
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
                Position = new ScalableVector2(Grade.X + Grade.Width + PaddingLeft / 2f, UsernameY),
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

            Time = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 16)
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
            if (timeDifference.TotalDays > 365)
                Time.Text = $"{(int) (timeDifference.TotalDays / 365)}y";
            // Months
            else if (timeDifference.TotalDays > 30)
                Time.Text = $"{(int) (timeDifference.TotalDays / 30)}mo";
            // Weeks
            else if (timeDifference.TotalDays > 7)
                Time.Text = $"{(int) (timeDifference.TotalDays / 7)}w";
            // Days
            else if (timeDifference.TotalDays > 0)
                Time.Text = $"{(int) timeDifference.TotalDays}d";
            // Hours
            else if (timeDifference.TotalHours > 0)
                Time.Text = $"{(int) timeDifference.TotalHours}h";
            // Minutes
            else if (timeDifference.TotalMinutes > 0)
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

            if (Score.IsPersonalBest)
                return;

            if (Button.IsHovered || CantBeatAlert.IsHovered)
            {
                Avatar.Alpha = 0;
                Rank.Alpha = 1;
            }
            else
            {
                Avatar.Alpha = 1;
                Rank.Alpha = 0;
            }
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
                    const int width = 60;
                    const int height = 30;

                    var mod = new DrawableModifier(modsList[i])
                    {
                        Parent = this,
                        Alignment = Alignment.BotLeft,
                        X = Username.X + width * Modifiers.Count - 4,
                        Y = AccuracyMaxCombo.Y + 3,
                        UsePreviousSpriteBatchOptions = true,
                        Size = new ScalableVector2(width, height)
                    };

                    Modifiers.Add(mod);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }
    }
}