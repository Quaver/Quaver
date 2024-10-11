using System;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Download;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Selection.UI.Maps.Components;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.Playlists;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;
using Color = Microsoft.Xna.Framework.Color;
using RectangleF = MonoGame.Extended.RectangleF;

namespace Quaver.Shared.Screens.Downloading.UI.Mapsets
{
    public sealed class DrawableDownloadableMapset : PoolableSprite<DownloadableMapset>
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override int HEIGHT { get; } = DrawableMapset.MapsetHeight;

        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        public bool IsSelected => SelectedMapset.Value == Item;

        /// <summary>
        /// </summary>
        private Sprite ContentContainer { get; set; }

        /// <summary>
        /// </summary>
        private ContainedButton Button { get; set; }

        /// <summary>
        /// </summary>
        private DownloadableMapsetBanner Banner { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ArtistTitle { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus ByText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Creator { get; set; }

        /// <summary>
        /// </summary>
        private Sprite RankedStatusIcon { get; set; }

        /// <summary>
        /// </summary>
        private Sprite GameModeIcon { get; set; }

        /// <summary>
        /// </summary>
        private PlaylistDifficultyDisplay DifficultyRange { get; set; }

        /// <summary>
        /// </summary>
        private ProgressBar DownloadProgressBar { get; set; }

        /// <summary>
        /// </summary>
        private ImageButton DifficultyHoverInvisibleButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <param name="selectedMapset"></param>
        public DrawableDownloadableMapset(PoolableScrollContainer<DownloadableMapset> container, DownloadableMapset item,
            int index, Bindable<DownloadableMapset> selectedMapset) : base(container, item, index)
        {
            SelectedMapset = selectedMapset;
            Alpha = 0;

            Size = new ScalableVector2(DrawableMapset.WIDTH, HEIGHT);

            CreateContainer();
            CreateButton();
            CreateBanner();
            CreateDownloadProgressBar();
            CreateArtistTitle();
            CreateRankedStatus();
            CreateGameModeIcon();
            CreateByText();
            CreateDifficultyRange();
            CreateCreator();

            SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Only perform animations if the drawable isn't visible
            if (RectangleF.Intersection(ScreenRectangle, Container.ScreenRectangle).IsEmpty)
            {
                Button.Update(gameTime);
                PerformTransformations(gameTime);
                return;
            }

            Button.Size = ContentContainer.Size;

            if (Button.IsHovered)
                Button.Alpha = 0.35f;
            else
                Button.Alpha = 0;

            DownloadProgressBar.Bindable.Value = 0;

            for (var i = 0; i < MapsetDownloadManager.CurrentDownloads.Count; i++)
            {
                var download = MapsetDownloadManager.CurrentDownloads[i];

                if (download.MapsetId == Item.Id)
                    DownloadProgressBar.Bindable.Value = download?.Progress?.Value?.ProgressPercentage ?? 0;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedMapset.ValueChanged -= OnSelectedMapsetChanged;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void UpdateContent(DownloadableMapset item, int index)
        {
            Item = item;
            Index = index;

            ScheduleUpdate(() =>
            {
                Banner.UpdateMapset(Item);

                ArtistTitle.Text = $"{Item.Artist} - {Item.Title}";
                ArtistTitle.TruncateWithEllipsis(450);
                ArtistTitle.Tint = Item.IsOwned ? ColorHelper.HexToColor("#808080") : Color.White;

                Creator.Text = Item.CreatorUsername;

                RankedStatusIcon.Image = GetRankedStatusTexture(Item);
                GameModeIcon.Image = GetModeIcon(Item);

                DifficultyRange.ChangeValue(Item.Maps.Min(x => x.DifficultyRating),
                    Item.Maps.Max(x => x.DifficultyRating));
                DifficultyRange.UpdateSize();
                Creator.X = ByText.X + ByText.Width + 4;

                if (IsSelected)
                    Select(true);
                else
                    Deselect(true);
            });
        }

        /// <summary>
        /// </summary>
        private void CreateContainer() => ContentContainer = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            Size = new ScalableVector2(Width - 50, 86),
            Image = UserInterface.DeselectedMapset,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        /// </summary>
        private void CreateButton()
        {
            Button = new ContainedButton(Container, UserInterface.BlankBox)
            {
                Parent = ContentContainer,
                Size = ContentContainer.Size,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                if (SelectedMapset.Value == Item)
                {
                    Logger.Important($"Initiating downloading request for mapset: {Item.Id}...", LogType.Network);

                    if (MapsetDownloadManager.CurrentDownloads.Any(x => x.MapsetId == SelectedMapset.Value.Id))
                    {
                        NotificationManager.Show(NotificationLevel.Warning, $"This mapset is already downloading!");
                        return;
                    }

                    MapsetDownloadManager.Download(SelectedMapset.Value.Id, SelectedMapset.Value.Artist,
                        SelectedMapset.Value.Title);
                    return;
                }

                SelectedMapset.Value = Item;
            };

            Button.RightClicked += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                game?.CurrentScreen?.ActivateRightClickOptions(new DownloadableMapsetRightClickOptions(Item, SelectedMapset));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new DownloadableMapsetBanner(Item, new ScalableVector2(421, ContentContainer.Height - 4))
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidRight,
                X = -2,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDownloadProgressBar()
        {
            DownloadProgressBar = new ProgressBar(new Vector2(ContentContainer.Width - Banner.Width - 4,
                ContentContainer.Height - 4), 0, 100, 0, Color.Transparent, Colors.MainBlue)
            {
                Parent = ContentContainer,
                Alignment = Alignment.MidLeft,
                X = 2,
                UsePreviousSpriteBatchOptions = true,
                ActiveBar =
                {
                    UsePreviousSpriteBatchOptions = true,
                    Alpha = 0.45f
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateArtistTitle()
        {
            ArtistTitle = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = ContentContainer,
                Position = new ScalableVector2(22, 18),
                Alpha = 0.85f,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateByText()
        {
            ByText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "By:", 20)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                X = ArtistTitle.X,
                Y = ArtistTitle.Y + ArtistTitle.Height + 8,
                Tint = ColorHelper.HexToColor("#757575"),
                Alpha = ArtistTitle.Alpha,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCreator()
        {
            Creator = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                Y = ByText.Y,
                Tint = ColorHelper.HexToColor("#0587e5"),
                Alpha = ArtistTitle.Alpha,
                UsePreviousSpriteBatchOptions = true
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRankedStatus() => RankedStatusIcon = new Sprite
        {
            Parent = ContentContainer,
            Alignment = Alignment.TopRight,
            Y = 14,
            X = -Banner.X - Banner.Width - 18,
            Alpha = 0.85f,
            Size = new ScalableVector2(94, 24),
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        /// </summary>
        private void CreateGameModeIcon() => GameModeIcon = new Sprite
        {
            Parent = ContentContainer,
            Alignment = Alignment.TopRight,
            Y = RankedStatusIcon.Y,
            X = RankedStatusIcon.X - RankedStatusIcon.Width - 12,
            Alpha = 0.85f,
            Size = new ScalableVector2(61, 24),
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Texture2D GetRankedStatusTexture(DownloadableMapset item)
        {
            switch (item.Maps.First().RankedStatus)
            {
                case RankedStatus.NotSubmitted:
                    return UserInterface.StatusNotSubmitted;
                case RankedStatus.Unranked:
                    return UserInterface.StatusUnranked;
                case RankedStatus.Ranked:
                    return UserInterface.StatusRanked;
                case RankedStatus.DanCourse:
                    return UserInterface.StatusNotSubmitted;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static Texture2D GetModeIcon(DownloadableMapset item)
        {
            var has4K = item.Maps.Any(x => x.GameMode == GameMode.Keys4);
            var has7K = item.Maps.Any(x => x.GameMode == GameMode.Keys7);
            
            if (has4K && has7K)
                return UserInterface.Mode4K7KSmall;

            if (has4K)
                return UserInterface.Mode4KSmall;

            if (has7K)
                return UserInterface.Mode7KSmall;

            throw new ArgumentException();
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRange()
        {
            DifficultyRange = new PlaylistDifficultyDisplay("Difficulty: ")
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopRight,
                Y = ByText.Y + 2,
                X = RankedStatusIcon.X - 56,
                UsePreviousSpriteBatchOptions = true,
            };

            DifficultyRange.Key.UsePreviousSpriteBatchOptions = true;
            DifficultyRange.Dash.UsePreviousSpriteBatchOptions = true;
            DifficultyRange.Value.UsePreviousSpriteBatchOptions = true;

            DifficultyRange.Key.Alpha = 0.85f;
            DifficultyRange.Value.Alpha = 0.85f;
            DifficultyRange.Dash.Alpha = 0.85f;
            DifficultyRange.MaxDifficulty.Alpha = 0.85f;
            DifficultyRange.ChangeValue(0, 0);

            DifficultyHoverInvisibleButton = new ContainedButton(Container, UserInterface.BlankBox)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(180, 60),
                X = RankedStatusIcon.X,
                Y = ByText.Y + 3,
                Alpha = 0,
                UsePreviousSpriteBatchOptions = true,
            };

            DifficultyHoverInvisibleButton.Y -= DifficultyHoverInvisibleButton.Height / 2f;

            DifficultyHoverInvisibleButton.Hovered += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                game?.CurrentScreen?.ActivateTooltip(new DownloadableMapsetTooltip(Item));
            };

            DifficultyHoverInvisibleButton.LeftHover += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;
                game?.CurrentScreen?.DeactivateTooltip();
            };
        }

        /// <summary>
        /// </summary>
        public void Select(bool changeWidthInstantly = false)
        {
            ContentContainer.Image = Item.IsOwned ? UserInterface.GrayedMapset : UserInterface.SelectedMapset;

            const int time = 200;
            AnimateSprites(1, 200);
            DifficultyRange.Fade(1, 200);

            if (changeWidthInstantly)
                ContentContainer.Width = Width;
            else
            {
                ContentContainer.ClearAnimations();
                ContentContainer.ChangeWidthTo((int) Width, Easing.OutQuint, time + 400);
            }
        }

        /// <summary>
        /// </summary>
        public void Deselect(bool changeWidthInstantly = false)
        {
            ContentContainer.Image = Item.IsOwned ? UserInterface.GrayedMapset : UserInterface.DeselectedMapset;

            const int time = 200;

            var alpha = !Item.IsOwned ? 0.85f : 0.70f;

            AnimateSprites(alpha, 200);
            DifficultyRange.Fade(alpha, 200);

            if (changeWidthInstantly)
                ContentContainer.Width = Width - 50;
            else
            {
                ContentContainer.ClearAnimations();
                ContentContainer.ChangeWidthTo((int) Width - 50, Easing.OutQuint, time + 400);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
        {
            if (IsSelected)
                Select();
            else
                Deselect();
        }

        /// <summary>
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="time"></param>
        private void AnimateSprites(float alpha, int time)
        {
            foreach (var child in ContentContainer.Children)
            {
                if (!(child is Sprite s))
                    continue;

                if (s == Button || s == Banner || s == DownloadProgressBar)
                    continue;

                s.SetChildrenAlpha = true;
                s.ClearAnimations();
                s.FadeTo(alpha, Easing.Linear, time);
            }
        }
    }
}