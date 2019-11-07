using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Quaver.Server.Common.Objects.Twitch;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Maps.Components;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Hub.SongRequests.Scrolling
{
    public class DrawableSongRequest : PoolableSprite<SongRequest>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override int HEIGHT { get; } = 89;

        /// <summary>
        /// </summary>
        private ImageButton Button { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ArtistTitle { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Difficulty { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Username { get; set; }

        /// <summary>
        /// </summary>
        private Sprite CreatorDividerLine { get; set; }

        /// <summary>
        /// </summary>
        private Sprite DifficultyDividerLine { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ByText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Creator { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public DrawableSongRequest(PoolableScrollContainer<SongRequest> container, SongRequest item, int index) : base(container, item, index)
        {
            Tint = index % 2 == 0 ? Colors.DarkGray : Colors.BlueishDarkGray;
            Alpha = 0;
            Size = new ScalableVector2(container.Width, HEIGHT);

            CreateButton();
            CreateArtistTitle();
            CreateCreator();
            CreateCreatorDividerLine();
            CreateIcon();
            CreateUsername();
            CreateDifficultyRating();
            CreateDifficultyDividerLine();

            // Bottom divider line
            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 2),
                Alpha = 0.45f,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Alpha = Button.IsHovered ? 0.40f : 0;

            var container = (SongRequestScrollContainer) Container;
            Button.Depth =  container.ActiveRightClickOptions!= null && container.ActiveRightClickOptions.Opened ? 1 : 0;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Marks the map has having been played
        /// </summary>
        public void MarkAsPlayed() => ChangeAlpha(0.70f);

        /// <summary>
        ///     Marks the map as having not been played
        /// </summary>
        public void MarkAsUnplayed() => ChangeAlpha(1f);

        /// <summary>
        /// </summary>
        /// <param name="alpha"></param>
        private void ChangeAlpha(float alpha)
        {
            ArtistTitle.Alpha = alpha;
            ByText.Alpha = alpha;
            Creator.Alpha = alpha;
            Icon.Alpha = alpha;
            Username.Alpha = alpha;
            Difficulty.Alpha = alpha;
            CreatorDividerLine.Alpha = alpha;
            DifficultyDividerLine.Alpha = alpha;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(SongRequest item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                ArtistTitle.Text = $"{Item.Artist} - {Item.Title}";
                ArtistTitle.TruncateWithEllipsis((int) Width - 50);

                const int spacing = 8;

                Creator.Text = Item.Creator;
                CreatorDividerLine.X = Creator.X + Creator.Width + spacing;

                Icon.X = CreatorDividerLine.X + CreatorDividerLine.Width + spacing;
                Username.Text = Item.TwitchUsername;

                if (Item.MapId != -1)
                {
                    Difficulty.Text = $"{StringHelper.RatingToString(Item.DifficultyRating)}";

                    switch ((MapGame) Item.Game)
                    {
                        case MapGame.Quaver:
                            Difficulty.Tint = ColorHelper.DifficultyToColor((float) Item.DifficultyRating);
                            break;
                        case MapGame.Osu:
                            Difficulty.Text += "*";
                            Difficulty.Tint = ColorHelper.OsuStarRatingToColor((float) Item.DifficultyRating);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    Difficulty.Text += $" - {Item.DifficultyName}";
                    DifficultyDividerLine.Visible = true;
                }
                else
                {
                    Difficulty.Text = "";
                    Difficulty.Tint = Color.White;
                    DifficultyDividerLine.Visible = false;
                }

                DifficultyDividerLine.X = Username.X + Username.Width + spacing;
                Difficulty.X = DifficultyDividerLine.X + DifficultyDividerLine.Width + spacing;
            });
        }

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new DrawableSongRequestButton(UserInterface.BlankBox, Container)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true,
                Size = Size,
                Alpha = 0
            };

            var container = (SongRequestScrollContainer) Container;

            Button.Clicked += (sender, args) => container.ActivateRightClickOptions(new SongRequestRightClickOptions(container, Item));
            Button.RightClicked += (sender, args) => container.ActivateRightClickOptions(new SongRequestRightClickOptions(container, Item));
        }

        /// <summary>
        /// </summary>
        private void CreateArtistTitle()
        {
            ArtistTitle = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), " ", 22)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(16, 18),
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRating()
        {
            Difficulty = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                Y = Username.Y,
                X = ArtistTitle.X,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyDividerLine()
        {
            DifficultyDividerLine = new Sprite
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                Size = CreatorDividerLine.Size,
                Tint = CreatorDividerLine.Tint,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateIcon()
        {
            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Image = UserInterface.TwitchIcon,
                Size = new ScalableVector2(22, 24),
                Y = Creator.Y - 2,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateUsername()
        {
            Username = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                Tint = ColorHelper.HexToColor("#757575"),
                X = Icon.Width + 6,
                Y = 1,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCreatorDividerLine()
        {
            CreatorDividerLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(2, 14),
                Tint = ColorHelper.HexToColor("#808080"),
                Y = Creator.Y + 2,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCreator()
        {
            ByText = new SpriteTextPlus(ArtistTitle.Font, "By:", 20)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = ArtistTitle.Y + ArtistTitle.Height + 10,
                X = ArtistTitle.X,
                Tint = ColorHelper.HexToColor("#757575"),
                UsePreviousSpriteBatchOptions = true
            };

            Creator = new SpriteTextPlus(ArtistTitle.Font, "Creator", ByText.FontSize)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = ByText.Y,
                X = ByText.X + ByText.Width + 4,
                Tint = ColorHelper.HexToColor("#0587e5"),
                UsePreviousSpriteBatchOptions = true
            };
        }
    }
}