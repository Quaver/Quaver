using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.News;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.V2.Main;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Main.UI
{
    /// <summary>
    ///     Compact latest-news card used by the replacement main menu.
    /// </summary>
    internal sealed class MainMenuNewsCard : Container
    {
        private static APIResponseNewsFeed CachedNews { get; set; }

        private SkinV2MainNewsConfig Config { get; }

        private Microsoft.Xna.Framework.Graphics.Texture2D FallbackImage { get; }

        private RoundedButton Card { get; }

        private RoundedImage Banner { get; }

        private Sprite BannerHoverOverlay { get; }

        private TooltipOptions TitleTooltip { get; }

        private IDisposable TitleTooltipRegistration { get; }

        private string PostUrl { get; set; }

        private string FullTitle { get; set; }

        public MainMenuNewsCard(float width, SkinStoreV2Lease skin, SkinV2MainNewsConfig config)
        {
            Config = config;
            FallbackImage = skin.LoadTexture(Config.FallbackImage, UserInterface.NoPreviewImage);
            FullTitle = LocalizationManager.Get("Screen_Main_NewsLoading");
            TitleTooltip = new TooltipOptions(FullTitle)
            {
                Anchor = TooltipAnchor.TopCenter,
                MaximumWidth = Config.MaximumWidth
            };
            Size = new ScalableVector2(width, Config.BannerHeight);

            Card = new RoundedButton((sender, args) => OpenPost())
            {
                Parent = this,
                Size = Size,
                Tint = Color.Transparent,
                CornerRadius = Config.CornerRadius,
                PerformHoverFade = false,
                IsClickable = false
            };

            Banner = new RoundedImage(width, Config.BannerHeight, Config.CornerRadius, FallbackImage)
            {
                Parent = Card,
                Alignment = Alignment.TopCenter
            };

            BannerHoverOverlay = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Size = Banner.Size,
                Image = RoundedRectTextureCache.Get(width, Config.BannerHeight, Config.CornerRadius),
                Tint = SkinV2Color.Parse(Config.HoverOverlayColor),
                Alpha = 0
            };

            TitleTooltipRegistration = Card.AddTooltip(TitleTooltip);

            ApplyWidth(width);
            Load();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var progress = (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds /
                                            Config.HoverTransitionMilliseconds, 1);
            var targetAlpha = Card.IsHovered ? Config.HoverOverlayOpacity : 0;
            BannerHoverOverlay.Alpha = MathHelper.Lerp(BannerHoverOverlay.Alpha, targetAlpha, progress);
        }

        public override void Destroy()
        {
            TitleTooltipRegistration.Dispose();
            base.Destroy();
        }

        public void ApplyWidth(float width)
        {
            width = Math.Max(Config.MinimumWidth, width);
            var bannerHeight = Banner.ApplyWidth(width);

            Size = new ScalableVector2(width, bannerHeight);
            Card.Size = Size;
            BannerHoverOverlay.Size = Size;
            BannerHoverOverlay.Image =
                RoundedRectTextureCache.Get(width, bannerHeight, Config.CornerRadius);
            TitleTooltip.MaximumWidth = width;
            ApplyTitle();
        }

        private void Load()
        {
            if (CachedNews != null)
            {
                Initialize(CachedNews);
                return;
            }

            ThreadScheduler.Run(() =>
            {
                var news = new APIRequestNewsFeed(NewsThumbnailType.Website).ExecuteRequest();
                ScheduleUpdate(() =>
                {
                    if (news != null)
                        CachedNews = news;

                    Initialize(news);
                });
            });
        }

        private void Initialize(APIResponseNewsFeed response)
        {
            var post = response?.Items?.FirstOrDefault();

            if (post == null)
            {
                FullTitle = LocalizationManager.Get("Screen_Main_NewsUnavailable");
                Banner.Content.Image = FallbackImage;
                PostUrl = null;
                Card.IsClickable = false;
                ApplyWidth(Width);
                ApplyTitle();
                return;
            }

            FullTitle = post.Title;
            Banner.Content.Image = response.RecentPostBanner ?? FallbackImage;
            PostUrl = post.Url;
            Card.IsClickable = !string.IsNullOrWhiteSpace(PostUrl);
            ApplyWidth(Width);
            ApplyTitle();
        }

        private void ApplyTitle()
        {
            TitleTooltip.Text = FullTitle ?? string.Empty;
        }

        private void OpenPost()
        {
            if (!string.IsNullOrWhiteSpace(PostUrl))
                BrowserHelper.OpenURL(PostUrl);
        }

        private sealed class RoundedImage : SpriteMaskContainer
        {
            private float HeightValue { get; }

            private float CornerRadiusValue { get; }

            public Sprite Content { get; }

            public RoundedImage(float width, float height, float cornerRadius, Microsoft.Xna.Framework.Graphics.Texture2D image)
            {
                HeightValue = height;
                CornerRadiusValue = cornerRadius;
                Size = new ScalableVector2(width, height);
                Image = RoundedRectTextureCache.Get(width, height, cornerRadius);

                Content = new Sprite
                {
                    Alignment = Alignment.TopLeft,
                    Size = Size,
                    Image = image
                };

                AddContainedSprite(Content);
            }

            public float ApplyWidth(float width)
            {
                var height = Content.Image?.Width > 0 && Content.Image.Height > 0
                    ? width * Content.Image.Height / Content.Image.Width
                    : HeightValue;

                Size = new ScalableVector2(width, height);
                Image = RoundedRectTextureCache.Get(width, height, CornerRadiusValue);
                Content.Size = Size;
                return height;
            }
        }
    }
}
