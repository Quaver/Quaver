using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.News;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Main.UI
{
    /// <summary>
    ///     Compact latest-news card used by the replacement main menu.
    /// </summary>
    internal sealed class MainMenuNewsCard : Container
    {
        private const float CardHeight = 190;
        private const float BannerHeight = 128;
        private const float PanelGap = 4;
        private const float CornerRadius = 7;

        private static APIResponseNewsFeed CachedNews { get; set; }

        private RoundedButton Card { get; }

        private RoundedImage Banner { get; }

        private RoundedButton TitleBackground { get; }

        private SpriteTextPlus Title { get; }

        private string PostUrl { get; set; }

        private string FullTitle { get; set; } = "Loading news…";

        public MainMenuNewsCard(float width)
        {
            Size = new ScalableVector2(width, CardHeight);

            Card = new RoundedButton((sender, args) => OpenPost())
            {
                Parent = this,
                Size = new ScalableVector2(width, CardHeight),
                Tint = Color.Transparent,
                CornerRadius = CornerRadius,
                PerformHoverFade = true,
                IsClickable = false
            };

            Banner = new RoundedImage(width, BannerHeight, CornerRadius, UserInterface.NoPreviewImage)
            {
                Parent = Card,
                Alignment = Alignment.TopCenter
            };

            TitleBackground = new RoundedButton
            {
                Parent = Card,
                Position = new ScalableVector2(0, BannerHeight + PanelGap),
                Size = new ScalableVector2(width, CardHeight - BannerHeight - PanelGap),
                Tint = new Color(47, 58, 68, 245),
                CornerRadius = CornerRadius,
                PerformHoverFade = false,
                IsClickable = false,
                IsInteractionEnabled = false
            };

            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), FullTitle, 19)
            {
                Parent = TitleBackground,
                Alignment = Alignment.MidLeft,
                X = 16,
                Tint = Color.White
            };

            ApplyWidth(width);
            Load();
        }

        public void ApplyWidth(float width)
        {
            width = Math.Max(320, width);

            if (Math.Abs(Width - width) < 0.001f)
                return;

            Width = width;
            Card.Width = width;
            Banner.ApplyWidth(width);
            TitleBackground.Width = width;
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
                FullTitle = "News is currently unavailable";
                Banner.Content.Image = UserInterface.NoPreviewImage;
                PostUrl = null;
                Card.IsClickable = false;
                ApplyTitle();
                return;
            }

            FullTitle = post.Title;
            Banner.Content.Image = response.RecentPostBanner ?? UserInterface.NoPreviewImage;
            PostUrl = post.Url;
            Card.IsClickable = !string.IsNullOrWhiteSpace(PostUrl);
            ApplyTitle();
        }

        private void ApplyTitle()
        {
            Title.Text = FullTitle ?? string.Empty;
            Title.TruncateWithEllipsis((int) Math.Max(40, TitleBackground.Width - 32));
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

            public void ApplyWidth(float width)
            {
                Width = width;
                Image = RoundedRectTextureCache.Get(width, HeightValue, CornerRadiusValue);
                Content.Width = width;
            }
        }
    }
}
