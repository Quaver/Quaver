using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;
using GlobalIconStore = Quaver.Shared.Assets.GlobalIcons;

namespace Quaver.Shared.Screens.Tests.GlobalIcons
{
    public sealed class GlobalIconsTestScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(17, 24, 32);

        private static readonly Color PanelColor = new Color(31, 41, 51);

        private static readonly Color BorderColor = new Color(56, 69, 82);

        private static readonly Color MutedTextColor = new Color(151, 164, 176);

        private static readonly Color ScrollbarColor = new Color(86, 97, 107);

        private const float HeaderHeight = 112;

        private const float OuterPadding = 36;

        private const float BottomPadding = 36;

        private const float CardWidth = 230;

        private const float CardHeight = 92;

        private const float CardGap = 14;

        private GlobalIcon[] Icons { get; } = (GlobalIcon[]) Enum.GetValues(typeof(GlobalIcon));

        private ScrollContainer ScreenScrollContainer { get; }

        private FlexContainer IconGrid { get; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        public GlobalIconsTestScreenView(Screen screen) : base(screen)
        {
            ScreenScrollContainer = new ScrollContainer(
                new ScalableVector2(Container.Width, Container.Height),
                new ScalableVector2(Container.Width, Container.Height + 1))
            {
                Parent = Container,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                ScrollSpeed = 160,
                Tint = Color.Transparent
            };
            ScreenScrollContainer.Scrollbar.Tint = ScrollbarColor;
            ScreenScrollContainer.Scrollbar.Width = 6;

            CreateHeader();

            IconGrid = new FlexContainer
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = HeaderHeight,
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.FlexStart,
                AlignContent = FlexAlignContent.FlexStart,
                RowGap = CardGap,
                ColumnGap = CardGap,
                UsePreviousSpriteBatchOptions = true
            };

            for (var i = 0; i < Icons.Length; i++)
            {
                var card = CreateIconCard(Icons[i], i);
                IconGrid.SetItemOptions(card, new FlexItemOptions
                {
                    Basis = CardWidth,
                    Grow = 0,
                    Shrink = 0
                });
            }

            ResizeToWindow();
        }

        public override void Update(GameTime gameTime)
        {
            ResizeToWindow();
            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(BackgroundColor);
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "GLOBAL ICONS", 28)
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 24,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight),
                $"{Icons.Length} icons — labels are read directly from the GlobalIcon enum", 17)
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 62,
                Tint = MutedTextColor,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private Container CreateIconCard(GlobalIcon icon, int index)
        {
            var card = new Container
            {
                Parent = IconGrid,
                Size = new ScalableVector2(CardWidth, CardHeight),
                UsePreviousSpriteBatchOptions = true
            };

            new Sprite
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                Size = card.Size,
                Image = WobbleAssets.WhiteBox,
                Tint = PanelColor,
                UsePreviousSpriteBatchOptions = true
            }.AddBorder(BorderColor, 1);

            new Sprite
            {
                Parent = card,
                Alignment = Alignment.MidLeft,
                X = 20,
                Size = new ScalableVector2(GlobalIconStore.IconSize, GlobalIconStore.IconSize),
                Region = GlobalIconStore.Get(icon),
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium), icon.ToString(), 16)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = 78,
                Y = 23,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight), $"Index {index}", 14)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = 78,
                Y = 50,
                Tint = MutedTextColor,
                UsePreviousSpriteBatchOptions = true
            };

            return card;
        }

        private void ResizeToWindow()
        {
            var width = WindowManager.Width;
            var height = WindowManager.Height;

            if (Math.Abs(LastWindowWidth - width) < 0.001f &&
                Math.Abs(LastWindowHeight - height) < 0.001f)
                return;

            Container.Size = new ScalableVector2(width, height);
            ScreenScrollContainer.Size = Container.Size;

            var gridWidth = Math.Max(CardWidth, width - OuterPadding * 2);
            var columns = Math.Max(1, (int) ((gridWidth + CardGap) / (CardWidth + CardGap)));
            var rows = (int) Math.Ceiling(Icons.Length / (double) columns);
            var gridHeight = rows * CardHeight + Math.Max(0, rows - 1) * CardGap;
            var contentHeight = Math.Max(height + 1, HeaderHeight + gridHeight + BottomPadding);

            ScreenScrollContainer.ContentContainer.Size = new ScalableVector2(width, contentHeight);
            IconGrid.Size = new ScalableVector2(gridWidth, gridHeight);
            IconGrid.RefreshLayout();

            LastWindowWidth = width;
            LastWindowHeight = height;
        }
    }
}
