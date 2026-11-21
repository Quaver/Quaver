using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI
{
    /// <summary>
    ///     Reusable row that displays a mapset or difficulty and handles its visuals and interactions
    /// </summary>
    internal sealed class SelectionListRow : PoolableSprite<SelectionListEntry>
    {
        private const int SelectionAnimationMilliseconds = 180;
        private const float TextGap = 12;
        private const int BannerRightPadding = 10;
        private const int BannerLoadDelayMilliseconds = 200;

        private SelectionMapsetList List { get; }
        private SkinV2SelectionRowConfig Config => List.GetRowConfig(Item);
        private NineSliceSprite Background { get; }
        private Sprite Banner { get; set; }
        private RowButton Button { get; }
        private MarqueeSpriteText Title { get; }
        private MarqueeSpriteText Artist { get; }
        private MarqueeSpriteText Creator { get; }
        private Color NormalColor { get; set; }
        private Color HoverColor { get; set; }
        private Color SelectedColor { get; set; }
        private int TargetWidth { get; set; }
        private float TextLayoutWidth { get; set; }
        private float BoundContentWidth { get; set; }
        private bool ContentBound { get; set; }
        private double TimeSinceBannerBound { get; set; }
        private bool BannerRequested { get; set; }
        private bool BannerEnabled { get; set; }
        private bool ShowsBanner => !Item.IsDifficulty && (ConfigManager.DisplaySongSelectBanners?.Value ?? true);

        public override int HEIGHT => Config.Height + List.RowGap;

        internal SelectionListRow(SelectionMapsetList list, SelectionListEntry item, int index) : base(list, item, index)
        {
            List = list;
            SetChildrenAlpha = true;

            Background = new NineSliceSprite
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };

            Button = new RowButton(list, this)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };
            Button.Clicked += OnClicked;

            Title = CreateText(Config.TitleFont, Config.TitleFontSize, Config.TextColor);
            Artist = CreateText(Config.Font, Config.ArtistFontSize, Config.SecondaryTextColor);
            Creator = CreateText(Config.Font, Config.CreatorFontSize, Config.SecondaryTextColor);

            SizeChanged += OnSizeChanged;
        }

        public override void UpdateContent(SelectionListEntry item, int index)
        {
            var sameItem = ContentBound && Item.HasSameItem(item);
            Item = item;
            Index = index;
            List.AttachRow(this);
            var contentWidth = V2Scrollbar.GetContentWidth(List);

            if (sameItem && BoundContentWidth == contentWidth && Height == Config.Height)
            {
                UpdateTextLayout(System.Math.Min(Width, GetRowWidth()));
                return;
            }

            ContentBound = true;
            BoundContentWidth = contentWidth;
            ClearAnimations();
            Button.ResetInteractionState();

            NormalColor = SkinV2Color.Parse(Config.BackgroundColor);
            HoverColor = SkinV2Color.Parse(Config.HoveredBackgroundColor);
            SelectedColor = SkinV2Color.Parse(Config.SelectedBackgroundColor);

            var margin = (int)System.Math.Ceiling(Config.CornerRadius);
            Background.Image = List.GetRowBackground(item);
            Background.Margins = new SliceMargins(margin);

            Title.TextSprite.Font = FontManager.GetWobbleFont(Config.TitleFont);
            Title.TextSprite.FontSize = Config.TitleFontSize;
            Title.TextSprite.Tint = SkinV2Color.Parse(Config.TextColor);

            Artist.TextSprite.Font = Creator.TextSprite.Font = FontManager.GetWobbleFont(Config.Font);
            Artist.TextSprite.FontSize = Config.ArtistFontSize;
            Artist.TextSprite.Tint = SkinV2Color.Parse(Config.SecondaryTextColor);

            Creator.TextSprite.FontSize = Config.CreatorFontSize;
            Creator.TextSprite.Tint = SkinV2Color.Parse(Config.SecondaryTextColor);

            if (!sameItem)
                UpdateBannerContent();

            TargetWidth = GetRowWidth();
            TextLayoutWidth = TargetWidth;

            Size = new ScalableVector2(TargetWidth, Config.Height);
            UpdateTextLayout(TargetWidth);
        }

        private int GetRowWidth() => System.Math.Min(List.IsSelected(Item) ? Config.SelectedWidth : Config.Width, (int)V2Scrollbar.GetContentWidth(List));

        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            if (IsDisposed || Button.IsDisposed)
                return;

            X = V2Scrollbar.GetContentWidth(List) - Width;
            Background.Size = Size;
            Button.Size = Size;

            if (Width == TargetWidth && TextLayoutWidth != TargetWidth)
                UpdateTextLayout(TargetWidth);
        }

        private void UpdateTextLayout(float rowWidth)
        {
            TextLayoutWidth = rowWidth;
            var padding = System.Math.Min(Config.Padding, rowWidth / 4f);
            var bannerSpace = ShowsBanner ? List.BannerCache.Width + BannerRightPadding : 0;
            var width = (int)System.Math.Max(1, rowWidth - padding * 2 - bannerSpace);

            SetText(Title, (Item.IsDifficulty ? Item.Difficulty.DifficultyName : Item.Mapset.Title) ?? "", width);

            SetText(Artist, Item.IsDifficulty ? "" : Item.Mapset.Artist ?? "", width);
            Artist.Visible = !string.IsNullOrEmpty(Artist.TextSprite.Text);

            SetText(Creator, Item.IsDifficulty ? "" : Item.Mapset.Creator ?? "", width);
            Creator.Visible = !string.IsNullOrEmpty(Creator.TextSprite.Text);

            var textHeight = Title.TextSprite.CapHeight + (Artist.Visible ? Artist.TextSprite.CapHeight + TextGap : 0) +
                (Creator.Visible ? Creator.TextSprite.CapHeight + TextGap : 0);
            var y = System.Math.Max(0, (Height - textHeight) / 2f);
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
        }

        private static void SetText(MarqueeSpriteText text, string value, int width)
        {
            var changed = text.TextSprite.Text != value || text.Width != width;
            text.TextSprite.Text = value;
            text.Size = new ScalableVector2(width, text.TextSprite.Height);
            if (changed)
                text.ResetPosition();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
                return;

            if (ShowsBanner != BannerEnabled)
            {
                UpdateBannerContent();
                UpdateTextLayout(System.Math.Min(Width, GetRowWidth()));
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

            var targetWidth = GetRowWidth();
            if (TargetWidth != targetWidth)
            {
                TargetWidth = targetWidth;
                ClearAnimations();

                // Avoid rebuilding text for every intermediate animation width
                UpdateTextLayout(System.Math.Min(Width, TargetWidth));
                if (Width != TargetWidth)
                    ChangeWidthTo(TargetWidth, Easing.OutCubic, SelectionAnimationMilliseconds);
            }

            Button.IsInteractionEnabled = List.CanInteract && List.IsInteractive(Item);
            Background.Tint = List.IsSelected(Item) ? SelectedColor : Button.IsHovered ? HoverColor : NormalColor;

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
        }

        private MarqueeSpriteText CreateText(string font, int size, string color)
        {
            var text = new MarqueeSpriteText(FontManager.GetWobbleFont(font), "", size, 1)
            {
                Parent = this,
                IsActive = true,
                UsePreviousSpriteBatchOptions = true
            };
            text.TextSprite.Tint = SkinV2Color.Parse(color);
            return text;
        }

        private void OnClicked(object sender, System.EventArgs args) => List.Select(Item);

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
            if (IsDisposed || !ShowsBanner || Banner == null || Item.Mapset.Directory != directory || image == null || image.IsDisposed)
                return;

            List.BannerCache.Bind(Banner, image);
            BannerRequested = true;
        }

        internal void SetActive(bool active)
        {
            if (!active)
                Button.ResetInteractionState();

            Visible = active;
        }

        /// <summary>
        ///     Restricts row click handling to the active Song Select list viewport
        /// </summary>
        private sealed class RowButton(SelectionMapsetList list, SelectionListRow row) : ImageButton(WobbleAssets.WhiteBox)
        {
            protected override bool IsMouseInClickArea() => list.CanInteract && list.IsInteractive(row.Item) && list.IsHovered() && base.IsMouseInClickArea();

            public override void DrawToSpriteBatch()
            {
            }
        }

        public override void Destroy()
        {
            SizeChanged -= OnSizeChanged;
            Button.Clicked -= OnClicked;
            if (Banner != null)
                List.BannerCache.Unbind(Banner);

            base.Destroy();
        }
    }
}
