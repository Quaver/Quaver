using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Quaver.Shared.Screens.Downloading.UI.Mapsets;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Quaver.Shared.Screens.Selection.UI.Playlists;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Filter
{
    public class DownloadFilterBanner : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private Bindable<DownloadableMapset> SelectedMapset { get; }

        /// <summary>
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Brightness { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus NoMapHeader { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus NoMapSubHeader { get; set; }

        /// <summary>
        /// </summary>
        private Sprite RankedStatus { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Mode { get; set; }

        /// <summary>
        /// </summary>
        private PlaylistDifficultyDisplay DifficultyRange { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Creator { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataBpm Bpm { get; set; }

        /// <summary>
        /// </summary>
        private FilterMetadataLength Length { get; set; }

        /// <summary>
        /// </summary>
        private DownloadFilterLongNotePercentage LongNotePercentage { get; set; }

        /// <summary>
        /// </summary>
        private DownloadFilterMapCount MapCount { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="selectedMapset"></param>
        /// <param name="size"></param>
        public DownloadFilterBanner(Bindable<DownloadableMapset> selectedMapset, ScalableVector2 size) : base(size, size)
        {
            SelectedMapset = selectedMapset;
            Alpha = 0;

            CreateBackground();
            CreateNoMapHeader();
            CreateRankedStatus();
            CreateMode();
            CreateDifficultyRange();
            CreateTitle();
            CreateCreator();
            CreateMapCount();
            CreateBpm();
            CreateLength();
            CreateLongNotePercentage();

            SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
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

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotRight,
                Y = 200,
                X = 300,
                Image = UserInterface.MenuBackgroundClear,
                Size = new ScalableVector2(1600, 900)
            };

            AddContainedDrawable(Background);

            Brightness = new Sprite()
            {
                Parent = Background,
                UsePreviousSpriteBatchOptions = true,
                Size = Background.Size,
                Tint = Color.Black,
                Alpha = 0.77f
            };
        }

        /// <summary>
        /// </summary>
        private void CreateNoMapHeader()
        {
            NoMapHeader = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "No Song Selected", 24)
            {
                Parent = this,
                Y = 32,
                X = 18,
                Tint = ColorHelper.HexToColor("#F9645D")
            };

            NoMapSubHeader = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "To begin downloading, double-click on a song.\n" +
                "Clicking once will allow you to preview the audio.\n" +
                "You can use the filters below to narrow down your search.", 20)
            {
                Parent = this,
                X = NoMapHeader.X,
                Y = NoMapHeader.Y + NoMapHeader.Height + 8
            };
        }

        /// <summary>
        /// </summary>
        private void CreateRankedStatus()
        {
            RankedStatus = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = 12,
                X = -18,
                Size = new ScalableVector2(88, 23),
                Image = UserInterface.StatusRanked,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMode()
        {
            Mode = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = RankedStatus.Y,
                X = RankedStatus.X - RankedStatus.Width - 12,
                Size = new ScalableVector2(54, 23),
                Image = UserInterface.Mode4K7KSmall,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDifficultyRange()
        {
            DifficultyRange = new PlaylistDifficultyDisplay()
            {
                Parent = this,
                Position = new ScalableVector2(-RankedStatus.X, RankedStatus.Y),
                Key =
                {
                    Tint = Color.White,
                    FontSize = 21,
                    Alpha = 0
                },
                Dash =
                {
                    FontSize = 21
                },
                Value =
                {
                    FontSize = 21
                },
                MaxDifficulty =
                {
                    FontSize = 21
                },
                X = -63,
                Visible = false
            };

            DifficultyRange.ChangeValue(0, 0);
        }

        /// <summary>
        /// </summary>
        private void CreateCreator()
        {
            Creator = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 21)
            {
                Parent = this,
                Y = Title.Y + Title.Height + 14,
                X = -RankedStatus.X,
                Tint = Colors.MainAccent,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateBpm()
        {
            Bpm = new FilterMetadataBpm(new Bindable<Map>(null))
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Creator.Y + Creator.Height + 12,
                X = Creator.X,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLength()
        {
            Length = new FilterMetadataLength(new Bindable<Map>(null))
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Creator.Y + Creator.Height + 12,
                X = Bpm.X + Bpm.Width + 20,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateLongNotePercentage()
        {
            LongNotePercentage = new DownloadFilterLongNotePercentage(SelectedMapset)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Creator.Y + Creator.Height + 12,
                X = MapCount.X + MapCount.Width + 20,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateMapCount()
        {
            MapCount = new DownloadFilterMapCount(SelectedMapset)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Creator.Y + Creator.Height + 14,
                X = Creator.X,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = this,
                Y = DifficultyRange.Y + DifficultyRange.Height + 20,
                X = -RankedStatus.X,
                Visible = false
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
        {
            var mapset = e.Value;

            NoMapHeader.Visible = false;
            NoMapSubHeader.Visible = false;

            RankedStatus.Visible = true;
            Mode.Visible = true;
            DifficultyRange.Visible = true;
            Title.Visible = true;
            Creator.Visible = true;
            Bpm.Visible = true;
            Length.Visible = true;
            LongNotePercentage.Visible = true;
            MapCount.Visible = true;

            Title.Text = $"{e.Value.Artist} - {e.Value.Title}";
            Title.TruncateWithEllipsis(450);

            Creator.Text = e.Value.CreatorUsername;
            Creator.TruncateWithEllipsis(450);

            var minDiff = e.Value.Maps.Min(x => x.DifficultyRating);
            var maxDiff = e.Value.Maps.Max(x => x.DifficultyRating);

            DifficultyRange.ChangeValue(minDiff, maxDiff);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (minDiff == maxDiff)
            {
                DifficultyRange.Dash.Visible = false;
                DifficultyRange.MaxDifficulty.Visible = false;
            }
            else
            {
                DifficultyRange.Dash.Visible = true;
                DifficultyRange.MaxDifficulty.Visible = true;
            }

            // Temporary map to use for the "BPM" and "Length" values
            var tempMap = new Map()
            {
                SongLength = (int) e.Value.Maps.Max(x => x.Length),
                Bpm = e.Value.Maps.Max(x => x.Bpm)
            };

            Bpm.Map.Value = tempMap;
            Length.Map.Value = tempMap;

            Mode.Image = DrawableDownloadableMapset.GetModeIcon(e.Value);
            RankedStatus.Image = DrawableDownloadableMapset.GetRankedStatusTexture(e.Value);

            const int spacing = 30;

            Bpm.X = MapCount.X + MapCount.Width + spacing;
            Length.X = Bpm.X + Bpm.Width + spacing;
            LongNotePercentage.X = Length.X + Length.Width + spacing;

            // Handle animation for brightness
            Brightness.Alpha = 1;
            Brightness.ClearAnimations();

            ImageDownloader.DownloadMapsetBanner(mapset.Id).ContinueWith(x =>
            {
                if (SelectedMapset.Value != mapset)
                    return;

                // Reset the position/size of the banner
                Background.Alignment = Alignment.MidCenter;
                Background.Position = new ScalableVector2(0, 0);
                Background.Size = new ScalableVector2(733, 204);

                // Change the image and perform animation
                Background.Image = x.Result;
                Brightness.FadeTo(0.75f, Easing.Linear, 250);
            });
        }
    }
}