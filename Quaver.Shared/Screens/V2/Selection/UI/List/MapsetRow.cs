using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.List
{
    /// <summary>
    ///     Displays one mapset with its banner background, metadata and source
    /// </summary>
    internal sealed class MapsetRow : SelectionListRow
    {
        private const float TextGap = 12;
        private const int BannerLoadDelayMilliseconds = 200;

        private MarqueeSpriteText Title { get; }
        private MarqueeSpriteText Artist { get; }
        private MarqueeSpriteText Creator { get; }
        private FlexContainer PillLayout { get; }
        private FlexContainer TopPillLayout { get; }
        private FlexContainer BottomPillLayout { get; }
        private RoundedButton ModePill { get; }
        private RoundedButton StatusPill { get; }
        private RoundedButton BannerDurationPill { get; }
        private RoundedButton BannerBpmPill { get; }
        private MapsetRowSurface Surface { get; }
        private Texture2D BannerSource { get; set; }
        private double TimeSinceBannerBound { get; set; }
        private bool BannerRequested { get; set; }
        private bool BannerEnabled { get; set; }
        private bool ShowsBanner => ConfigManager.DisplaySongSelectBanners?.Value ?? true;
        protected override bool DrawsCustomBackground => Surface.Visible;
        protected override bool UsesInteractionOverlay => true;
        protected override bool ChangesWidthWhenSelected => false;

        internal MapsetRow(MapsetList list, SelectionListEntry item, int index) : base(list, item, index, list.MapsetRowConfig)
        {
            Surface = new MapsetRowSurface(list.MapsetSurfaceCache, this) { Size = Size };
            SizeChanged += OnMapsetSizeChanged;

            Title = CreateText(Config.TitleFont, Config.TitleFontSize, Config.TextColor);
            Artist = CreateText(Config.Font, Config.ArtistFontSize, Config.SecondaryTextColor);
            Creator = CreateText(Config.Font, Config.CreatorFontSize, Config.SecondaryTextColor);

            PillLayout = new FlexContainer
            {
                Parent = this,
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.FlexEnd,
                RowGap = Config.PillGap
            };

            TopPillLayout = new FlexContainer
            {
                Parent = PillLayout,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.PillGap
            };
            BottomPillLayout = new FlexContainer
            {
                Parent = PillLayout,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.PillGap
            };

            ModePill = CreateMetadataPill(TopPillLayout);
            StatusPill = CreateMetadataPill(TopPillLayout);
            BannerDurationPill = CreateBannerStatPill(BottomPillLayout, CapsuleIcons.Get(CapsuleIcon.Time));
            BannerBpmPill = CreateBannerStatPill(BottomPillLayout, UserInterface.SelectionBpm);
        }

        protected override void UpdateRowContent(bool sameItem)
        {
            Title.TextSprite.Font = FontManager.GetWobbleFont(Config.TitleFont);
            Title.TextSprite.FontSize = Config.TitleFontSize;
            Title.TextSprite.Tint = SkinV2Color.Parse(Config.TextColor);

            Artist.TextSprite.Font = Creator.TextSprite.Font = FontManager.GetWobbleFont(Config.Font);
            Artist.TextSprite.FontSize = Config.ArtistFontSize;
            Artist.TextSprite.Tint = SkinV2Color.Parse(Config.SecondaryTextColor);

            Creator.TextSprite.FontSize = Config.CreatorFontSize;
            Creator.TextSprite.Tint = SkinV2Color.Parse(Config.SecondaryTextColor);

            UpdatePillContent();

            if (!sameItem)
            {
                Surface.Reset();
                UpdateBannerContent();
            }

            UpdateBannerStatsContent();
            UpdateTimingPillVisibility();
            UpdatePillLayout();
        }

        protected override void UpdateRowLayout(float rowWidth)
        {
            var padding = Math.Min(Config.Padding, rowWidth / 4f);
            var pillSpace = PillLayout.Visible ? PillLayout.Width + Config.PillGap : 0;
            var width = (int) Math.Max(1, rowWidth - padding * 2 - pillSpace);

            SetText(Title, Item.Mapset.Title ?? "", width);

            SetText(Artist, Item.Mapset.Artist ?? "", width);
            Artist.Visible = !string.IsNullOrEmpty(Artist.TextSprite.Text);

            var creator = Item.Mapset.Creator;
            SetText(Creator, string.IsNullOrEmpty(creator) ? "" : $"By: {creator}", width);
            Creator.Visible = !string.IsNullOrEmpty(Creator.TextSprite.Text);

            var textHeight = Title.TextSprite.CapHeight +
                (Artist.Visible ? Artist.TextSprite.CapHeight + TextGap : 0) + 
                (Creator.Visible ? Creator.TextSprite.CapHeight + TextGap : 0);

            var y = Math.Max(0, (Height - textHeight) / 2f);
            Title.Position = new ScalableVector2(padding, y - Title.TextSprite.CapTopOffset);

            y += Title.TextSprite.CapHeight;
            if (Artist.Visible)
            {
                y += TextGap;
                Artist.Position = new ScalableVector2(padding, y - Artist.TextSprite.CapTopOffset);
                y += Artist.TextSprite.CapHeight;
            }

            if (Creator.Visible)
                Creator.Position = new ScalableVector2(padding, y + TextGap - Creator.TextSprite.CapTopOffset);

            UpdatePillLayout();
        }

        protected override void UpdateRow(GameTime gameTime)
        {
            if (ShowsBanner != BannerEnabled)
            {
                UpdateBannerContent();
                UpdateRowLayout(Width);
            }

            if (ShowsBanner && !BannerRequested)
            {
                TimeSinceBannerBound += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (TimeSinceBannerBound >= BannerLoadDelayMilliseconds)
                {
                    BannerRequested = true;
                    var banner = List.RequestBanner(Item.Mapset);
                    if (banner != null)
                        SetBanner(Item.Mapset.Directory, banner);
                }
            }

            var selected = List.IsSelected(Item);
            Surface.Set(ShowsBanner ? BannerSource : null, IsHovered, selected);

            UpdateTimingPillVisibility();
        }

        internal override void RefreshDynamicContent()
        {
            UpdateBannerStatsContent();
            UpdatePillLayout();
        }

        private RoundedButton CreateMetadataPill(Container parent) => new()
        {
            Parent = parent,
            CornerRadius = 15,
            WidthMode = ButtonSizeMode.Fixed,
            AutoSizePadding = new Vector2(Config.PillHorizontalPadding, 0),
            Height = 30,
            IsClickable = false,
            IsInteractionEnabled = false,
            PerformHoverFade = false,
            UsePreviousSpriteBatchOptions = true
        };

        private void UpdatePillContent()
        {
            PillLayout.Visible = Item.Mapset?.Maps?.Count > 0;
            if (!PillLayout.Visible)
                return;

            ModePill.Height = StatusPill.Height = Config.PillHeight;
            ModePill.AutoSizePadding = StatusPill.AutoSizePadding = new Vector2(Config.PillHorizontalPadding, 0);
            ModePill.CornerRadius = StatusPill.CornerRadius = Config.PillHeight / 2f;
            ModePill.SetLabel(FontManager.GetWobbleFont(Config.Font), GetModeText(), Config.PillFontSize, SkinV2Color.Parse(Config.PillTextColor));
            ModePill.Width = Math.Max(Config.PillMinimumWidth, ModePill.Label.Width + Config.PillHorizontalPadding);
            ModePill.Tint = GetModePillColor();
            
            StatusPill.SetLabel(FontManager.GetWobbleFont(Config.Font), GetStatusText(), Config.PillFontSize, SkinV2Color.Parse(Config.PillTextColor));
            StatusPill.Width = Math.Max(Config.PillMinimumWidth, StatusPill.Label.Width + Config.PillHorizontalPadding);
            StatusPill.Tint = GetStatusPillColor();
        }

        private void UpdatePillLayout()
        {
            if (!PillLayout.Visible)
                return;

            TopPillLayout.ColumnGap = Config.PillGap;
            TopPillLayout.Size = new ScalableVector2(
                ModePill.Width + Config.PillGap + StatusPill.Width,
                Math.Max(ModePill.Height, StatusPill.Height));

            TopPillLayout.SetItemOptions(ModePill, new FlexItemOptions { Basis = ModePill.Width, Shrink = 0 });
            TopPillLayout.SetItemOptions(StatusPill, new FlexItemOptions { Basis = StatusPill.Width, Shrink = 0 });
            TopPillLayout.RefreshLayout();

            BottomPillLayout.ColumnGap = Config.PillGap;
            BottomPillLayout.Size = new ScalableVector2(
                BannerDurationPill.Width + Config.PillGap + BannerBpmPill.Width,
                Math.Max(BannerDurationPill.Height, BannerBpmPill.Height));

            BottomPillLayout.SetItemOptions(BannerDurationPill, new FlexItemOptions { Basis = BannerDurationPill.Width, Shrink = 0 });
            BottomPillLayout.SetItemOptions(BannerBpmPill, new FlexItemOptions { Basis = BannerBpmPill.Width, Shrink = 0 });
            BottomPillLayout.RefreshLayout();

            var blockHeight = TopPillLayout.Height + Config.PillGap + BottomPillLayout.Height;
            PillLayout.RowGap = Config.PillGap;
            PillLayout.Size = new ScalableVector2(Math.Max(TopPillLayout.Width, BottomPillLayout.Width), blockHeight);
            PillLayout.SetItemOptions(TopPillLayout, new FlexItemOptions { Basis = TopPillLayout.Height, Shrink = 0 });
            PillLayout.SetItemOptions(BottomPillLayout, new FlexItemOptions { Basis = BottomPillLayout.Height, Shrink = 0 });
            PillLayout.Position = new ScalableVector2(Width - Config.Padding - PillLayout.Width, (Height - blockHeight) / 2f);
            PillLayout.RefreshLayout();
        }

        private void UpdateTimingPillVisibility() =>
            BottomPillLayout.Visible = List.IsSelected(Item) || IsHovered;

        private string GetModeText() => string.Join("/", Item.Mapset.Maps.Select(map => ModeHelper.ToShortHand(map.Mode)).Distinct());

        private Color GetModePillColor()
        {
            var modes = Item.Mapset.Maps.Select(map => map.Mode).Distinct().ToList();
            if (modes.Count == 1)
                return GameModeHelper.GetColor(modes[0]);

            if (modes.Count == 2 && modes.Contains(GameMode.Keys4) && modes.Contains(GameMode.Keys7))
                return SkinV2Color.Parse("#5969E2FF");

            return SkinV2Color.Parse("#808080FF");
        }

        private string GetStatusText()
        {
            var games = Item.Mapset.Maps.Select(map => map.Game).Distinct().ToList();
            if (games.Count > 1)
                return "VARIOUS";

            if (games[0] == MapGame.Osu)
                return "OSU!";

            if (games[0] == MapGame.Etterna)
                return "ETTERNA";

            return Item.Mapset.Maps.Max(map => map.RankedStatus) switch
            {
                RankedStatus.Ranked => "RANKED",
                RankedStatus.Unranked => "UNRANKED",
                RankedStatus.DanCourse => "DAN COURSE",
                _ => "NOT SUBMITTED"
            };
        }

        private Color GetStatusPillColor()
        {
            var games = Item.Mapset.Maps.Select(map => map.Game).Distinct().ToList();
            if (games.Count > 1)
                return SkinV2Color.Parse("#0FBAE5FF");

            if (games[0] == MapGame.Osu)
                return SkinV2Color.Parse("#EE5FACFF");

            if (games[0] == MapGame.Etterna)
                return SkinV2Color.Parse("#60407FFF");

            return Item.Mapset.Maps.Max(map => map.RankedStatus) switch
            {
                RankedStatus.Ranked => SkinV2Color.Parse("#27B06EFF"),
                RankedStatus.Unranked => SkinV2Color.Parse("#F9645DFF"),
                _ => SkinV2Color.Parse("#808080FF")
            };
        }

        private RoundedButton CreateBannerStatPill(Container parent, TextureRegion icon)
        {
            var pill = new RoundedButton
            {
                Parent = parent,
                WidthMode = ButtonSizeMode.Fixed,
                Height = Config.BannerStatPillHeight,
                CornerRadius = Config.BannerStatPillHeight / 2f,
                Tint = SkinV2Color.Parse(Config.BannerStatPillColor),
                IsClickable = false,
                IsInteractionEnabled = false,
                PerformHoverFade = false,
                UsePreviousSpriteBatchOptions = true
            };
            pill.SetIcon(icon, new Vector2(Config.BannerStatIconSize));
            pill.Icon.Tint = SkinV2Color.Parse(Config.DifficultyStatColor);

            return pill;
        }

        private void UpdateBannerStatsContent()
        {
            if (Item.Mapset?.Maps?.Count == 0)
                return;

            var various = Item.Mapset.Maps.Select(map => map.SongLength).Distinct().Count() > 1 ||
                          Item.Mapset.Maps.Select(map => map.Bpm).Distinct().Count() > 1;

            var duration = various ? "VARIOUS" : DifficultyRow.GetDurationStatText(Item.Mapset.Maps[0]);
            SetBannerStatLabel(BannerDurationPill, duration);

            var bpm = various ? "VARIOUS" : $"{DifficultyRow.GetBpmStatText(Item.Mapset.Maps[0])} BPM";
            SetBannerStatLabel(BannerBpmPill, bpm);
        }

        private void SetBannerStatLabel(RoundedButton pill, string text)
        {
            pill.SetLabel(FontManager.GetWobbleFont(Config.Font), text, Config.BannerStatFontSize, SkinV2Color.Parse(Config.DifficultyStatColor));
            pill.Width = pill.Icon.Width + 8 + pill.Label.Width + Config.BannerStatPillPadding;
        }

        private void UpdateBannerContent()
        {
            TimeSinceBannerBound = 0;
            BannerRequested = false;

            BannerEnabled = ShowsBanner;
            if (!BannerEnabled)
            {
                BannerSource = null;
                return;
            }

            var image = List.GetCachedBanner(Item.Mapset);
            BannerSource = image ?? UserInterface.DefaultBanner;
            BannerRequested = image != null;
        }

        internal void SetBanner(string directory, Texture2D image)
        {
            if (IsDisposed || !ShowsBanner || Item.Mapset.Directory != directory ||
                image == null || image.IsDisposed)
                return;

            BannerSource = image;
            BannerRequested = true;
        }

        private void OnMapsetSizeChanged(object sender, ScalableVector2 size)
        {
            Surface.Size = size;
        }

        public override void Destroy()
        {
            SizeChanged -= OnMapsetSizeChanged;
            Surface.Dispose();

            base.Destroy();
        }
    }
}
