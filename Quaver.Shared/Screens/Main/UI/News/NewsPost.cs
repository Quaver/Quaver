using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.News;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using TimeAgo;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI.News
{
    public class NewsPost : ImageButton
    {
        /// <summary>
        /// </summary>
        private static APIResponseNewsFeed NewsPosts { get; set; }

        private LoadingWheel Wheel { get; set; }

        /// <summary>
        /// </summary>
        private Container Container { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Banner { get; set; }

        private SpriteTextPlus Title { get; set; }

        private SpriteTextPlus TimeAgo { get; set; }

        private SpriteTextPlus ShortText { get; set; }

        private Sprite HoverEffect { get; set; }

        public NewsPost() : base(UserInterface.BlankBox)
        {
            Image = SkinManager.Skin?.MainMenu?.NewsPanel ?? UserInterface.NewsPanel;

            Size = new ScalableVector2(484, 261);

            SetChildrenAlpha = true;

            CreateContainer();
            CreateLoadingWheel();

            Load();
        }

        public override void Update(GameTime gameTime)
        {
            if (Banner != null && Banner.Animations.Count == 0)
            {
                Banner.Alpha = Alpha;
                Title.Alpha = Alpha;
                TimeAgo.Alpha = Alpha;
                ShortText.Alpha = Alpha;

                HoverEffect.Size = new ScalableVector2(Width - 4, Height - 4);
                HoverEffect.Alpha = IsHovered ? 0.35f : 0;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel()
        {
            Wheel = new LoadingWheel()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(40, 40),
                Alpha = 0
            };

            Wheel.FadeTo(1, Easing.Linear, 450);
        }

        private void CreateContainer() => Container = new Container()
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width - 4, Height - 4),
        };

        /// <summary>
        /// </summary>
        private void Load()
        {
            if (NewsPosts != null)
            {
                Initialize(NewsPosts);
                return;
            }

            ThreadScheduler.Run(() =>
            {
                try
                {
                    NewsPosts = new APIRequestNewsFeed().ExecuteRequest();
                    ScheduleUpdate(() => Initialize(NewsPosts));
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);

                    Wheel.ClearAnimations();
                    Wheel.FadeTo(0, Easing.Linear, 450);
                }
            });
        }

        private void Initialize(APIResponseNewsFeed newsFeed)
        {
            if (newsFeed.Items.Count == 0)
                return;

            var latestPost = newsFeed.Items.First();

            Wheel.ClearAnimations();
            Wheel.FadeTo(0, Easing.Linear, 450);

            CreateBanner(newsFeed.RecentPostBanner);
            CreateTitle(latestPost);
            CreateTimeAgo(latestPost);
            CreateShortText(latestPost);
            CreateHoverEffect();

            Clicked += (sender, args) => BrowserHelper.OpenURL(latestPost.Url);
            Hovered += (sender, args) => SkinManager.Skin?.SoundHover.CreateChannel().Play();
        }

        private void CreateBanner(Texture2D banner)
        {
            Banner = new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(Container.Width, 121),
                Alpha = 0,
                Image = banner ?? UserInterface.NoPreviewImage
            };

            Banner.FadeTo(1, Easing.Linear, 450);
        }

        private void CreateTitle(NewsFeedItem item)
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), item.Title, 22)
            {
                Parent = Container,
                X = 14,
                Y = Banner.Y + Banner.Height + 16,
                Tint = SkinManager.Skin?.MainMenu?.NewsTitleColor ?? ColorHelper.HexToColor("#45D6F5"),
                Alpha = 0
            };

            Title.TruncateWithEllipsis((int) Width - 60);
            Title.FadeTo(1, Easing.Linear, 450);
        }

        private void CreateTimeAgo(NewsFeedItem item)
        {
            TimeAgo = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"Published {item.DatePublished.TimeAgo()}",
                20)
            {
                Parent = Container,
                X = Title.X,
                Y = Title.Y + Title.Height + 12,
                Tint = SkinManager.Skin?.MainMenu?.NewsDateColor ?? ColorHelper.HexToColor("#808080"),
                Alpha = 0
            };

            TimeAgo.FadeTo(1, Easing.Linear, 450);
        }

        private void CreateShortText(NewsFeedItem item)
        {
            ShortText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), item.ShortText, 20)
            {
                Parent = Container,
                X = Title.X,
                Y = TimeAgo.Y + TimeAgo.Height + 12,
                MaxWidth = Container.Width - 20,
                Alpha = 0,
                Tint = SkinManager.Skin?.MainMenu?.NewsTextColor ?? Color.White
            };

            ShortText.TruncateWithEllipsis((int) ShortText.MaxWidth * 2 - 100);
            ShortText.FadeTo(1, Easing.Linear, 450);
        }

        private void CreateHoverEffect() => HoverEffect = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width - 4, Height - 4),
            Alpha = 0
        };
    }
}