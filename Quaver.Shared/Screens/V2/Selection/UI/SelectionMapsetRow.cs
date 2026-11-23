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
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI
{
    /// <summary>
    ///     Displays one mapset with its metadata, source pills, banner and selection-only banner statistics
    /// </summary>
    internal sealed class SelectionMapsetRow : SelectionListRow
    {
        private const float TextGap = 12;
        private const int BannerRightPadding = 10;
        private const int BannerLoadDelayMilliseconds = 200;

        private MarqueeSpriteText Title { get; }
        private MarqueeSpriteText Artist { get; }
        private MarqueeSpriteText Creator { get; }
        private FlexContainer PillLayout { get; }
        private RoundedButton ModePill { get; }
        private RoundedButton StatusPill { get; }
        private Container BannerStatsHost { get; }
        private FlexContainer BannerStatsLayout { get; }
        private RoundedButton BannerDurationPill { get; }
        private RoundedButton BannerBpmPill { get; }
        private Sprite Banner { get; set; }
        private bool BannerStatsSelected { get; set; }
        private double TimeSinceBannerBound { get; set; }
        private bool BannerRequested { get; set; }
        private bool BannerEnabled { get; set; }
        private bool ShowsBanner => ConfigManager.DisplaySongSelectBanners?.Value ?? true;

        internal SelectionMapsetRow(SelectionMapsetList list, SelectionListEntry item, int index) : base(list, item, index, list.MapsetRowConfig)
        {
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

            ModePill = CreateMetadataPill();
            StatusPill = CreateMetadataPill();

            PillLayout.SetItemOptions(ModePill, new FlexItemOptions { Basis = Config.PillHeight, Shrink = 0 });
            PillLayout.SetItemOptions(StatusPill, new FlexItemOptions { Basis = Config.PillHeight, Shrink = 0 });

            BannerStatsHost = new Container { Parent = this, Visible = false };
            BannerStatsLayout = new FlexContainer
            {
                Parent = BannerStatsHost,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                ColumnGap = Config.BannerStatPillGap
            };

            BannerDurationPill = CreateBannerStatPill(CapsuleIcons.Get(CapsuleIcon.Time));
            BannerBpmPill = CreateBannerStatPill(UserInterface.SelectionBpm);
            BannerStatsLayout.SetItemOptions(BannerDurationPill, new FlexItemOptions { Shrink = 0 });
            BannerStatsLayout.SetItemOptions(BannerBpmPill, new FlexItemOptions { Shrink = 0 });
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
                UpdateBannerContent();

            UpdateBannerStatsContent();
        }

        protected override void UpdateRowLayout(float rowWidth)
        {
            var padding = Math.Min(Config.Padding, rowWidth / 4f);
            var pillSpace = PillLayout.Visible ? PillLayout.Width + Config.PillGap : 0;
            var bannerSpace = ShowsBanner ? List.BannerCache.Width + BannerRightPadding : 0;
            var width = (int) Math.Max(1, rowWidth - padding * 2 - bannerSpace - pillSpace);

            SetText(Title, Item.Mapset.Title ?? "", width);

            SetText(Artist, Item.Mapset.Artist ?? "", width);
            Artist.Visible = !string.IsNullOrEmpty(Artist.TextSprite.Text);

            SetText(Creator, Item.Mapset.Creator ?? "", width);
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
            UpdateBannerStatsLayout();
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

            UpdateBannerStatsAnimation();
        }

        internal override void RefreshDynamicContent() => UpdateBannerStatsContent();

        private RoundedButton CreateMetadataPill() => new()
        {
            Parent = PillLayout,
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

            UpdatePillLayout();
        }

        private void UpdatePillLayout()
        {
            if (!PillLayout.Visible)
                return;

            var right = ShowsBanner
                ? Width - BannerRightPadding - List.BannerCache.Width - Config.PillGap
                : Width - Config.Padding;

            var blockHeight = Config.PillHeight * 2 + Config.PillGap;
            PillLayout.RowGap = Config.PillGap;
            PillLayout.Size = new ScalableVector2(Math.Max(ModePill.Width, StatusPill.Width), blockHeight);
            PillLayout.Position = new ScalableVector2(right - PillLayout.Width, (Height - blockHeight) / 2f);
            PillLayout.RefreshLayout();
        }

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

        private RoundedButton CreateBannerStatPill(TextureRegion icon)
        {
            var pill = new RoundedButton
            {
                Parent = BannerStatsLayout,
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
            {
                SetBannerStatsAlpha(0);
                BannerStatsSelected = false;
                BannerStatsHost.Visible = false;
                return;
            }

            var various = Item.Mapset.Maps.Select(map => map.SongLength).Distinct().Count() > 1 ||
                          Item.Mapset.Maps.Select(map => map.Bpm).Distinct().Count() > 1;

            var duration = various ? "VARIOUS" : SelectionDifficultyRow.GetDurationStatText(Item.Mapset.Maps[0]);
            SetBannerStatLabel(BannerDurationPill, duration);

            var bpm = various ? "VARIOUS" : $"{SelectionDifficultyRow.GetBpmStatText(Item.Mapset.Maps[0])} BPM";
            SetBannerStatLabel(BannerBpmPill, bpm);

            SetBannerStatsAlpha(0);

            BannerStatsSelected = false;
            BannerStatsHost.Visible = ShowsBanner;
            BannerStatsHost.Parent = this;

            UpdateBannerStatsLayout();
        }

        private void SetBannerStatLabel(RoundedButton pill, string text)
        {
            pill.SetLabel(FontManager.GetWobbleFont(Config.Font), text, Config.BannerStatFontSize, SkinV2Color.Parse(Config.DifficultyStatColor));
            pill.Width = pill.Icon.Width + 8 + pill.Label.Width + Config.BannerStatPillPadding;
        }

        private void UpdateBannerStatsLayout()
        {
            if (!BannerStatsHost.Visible)
                return;

            BannerStatsLayout.ColumnGap = Config.BannerStatPillGap;
            BannerStatsLayout.Size = new ScalableVector2(
                BannerDurationPill.Width + Config.BannerStatPillGap + BannerBpmPill.Width,
                Config.BannerStatPillHeight);

            var bannerX = Width - BannerRightPadding - List.BannerCache.Width;
            var bannerY = (Height - List.BannerCache.Height) / 2f;

            BannerStatsHost.Size = BannerStatsLayout.Size;
            BannerStatsHost.Position = new ScalableVector2(
                bannerX + List.BannerCache.Width - Config.BannerStatInset - BannerStatsLayout.Width,
                bannerY + List.BannerCache.Height - Config.BannerStatInset - BannerStatsLayout.Height);

            BannerStatsLayout.Position = new ScalableVector2(0, 0);

            BannerStatsLayout.RefreshLayout();
        }

        private void UpdateBannerStatsAnimation()
        {
            var selected = ShowsBanner && List.IsSelected(Item);
            BannerStatsHost.Visible = ShowsBanner;

            if (BannerStatsSelected == selected)
                return;

            BannerStatsSelected = selected;

            BannerDurationPill.ClearAnimations(AnimationProperty.Alpha);
            BannerDurationPill.FadeTo(selected ? 1 : 0, Easing.OutCubic, Config.BannerStatFadeMilliseconds);

            BannerBpmPill.ClearAnimations(AnimationProperty.Alpha);
            BannerBpmPill.FadeTo(selected ? 1 : 0, Easing.OutCubic, Config.BannerStatFadeMilliseconds);
        }

        private void SetBannerStatsAlpha(float alpha)
        {
            BannerDurationPill.ClearAnimations(AnimationProperty.Alpha);
            BannerDurationPill.Alpha = alpha;

            BannerBpmPill.ClearAnimations(AnimationProperty.Alpha);
            BannerBpmPill.Alpha = alpha;
        }

        private void UpdateBannerContent()
        {
            TimeSinceBannerBound = 0;
            BannerRequested = false;

            BannerEnabled = ShowsBanner;
            if (!BannerEnabled)
            {
                if (Banner != null)
                    List.BannerCache.Unbind(Banner);
                return;
            }
            if (Banner == null)
            {
                Banner = new Sprite
                {
                    Parent = this,
                    Size = new ScalableVector2(List.BannerCache.Width, List.BannerCache.Height),
                    Alignment = Alignment.MidRight,
                    X = -BannerRightPadding,
                    UsePreviousSpriteBatchOptions = true,
                    Visible = false
                };
            }

            var image = List.GetCachedBanner(Item.Mapset);
            List.BannerCache.Bind(Banner, image ?? UserInterface.DefaultBanner);
            BannerRequested = image != null;
        }

        internal void SetBanner(string directory, Texture2D image)
        {
            if (IsDisposed || !ShowsBanner || Banner == null || Item.Mapset.Directory != directory ||
                image == null || image.IsDisposed)
                return;

            List.BannerCache.Bind(Banner, image);
            BannerRequested = true;
        }

        public override void Destroy()
        {
            if (Banner != null)
                List.BannerCache.Unbind(Banner);

            base.Destroy();
        }
    }
}
