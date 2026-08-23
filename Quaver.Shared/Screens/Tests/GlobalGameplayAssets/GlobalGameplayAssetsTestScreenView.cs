using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.Shared.Assets;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;
using GlobalGameplayAssetStore = Quaver.Shared.Assets.GlobalGameplayAssets;

namespace Quaver.Shared.Screens.Tests.GlobalGameplayAssets
{
    public sealed class GlobalGameplayAssetsTestScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(17, 24, 32);

        private static readonly Color PanelColor = new Color(31, 41, 51);

        private static readonly Color BorderColor = new Color(56, 69, 82);

        private static readonly Color MutedTextColor = new Color(151, 164, 176);

        private static readonly Color ScrollbarColor = new Color(86, 97, 107);

        private const float HeaderHeight = 112;

        private const float OuterPadding = 36;

        private const float BottomPadding = 36;

        private const float CardWidth = 300;

        private const float CardHeight = 92;

        private const float CardGap = 14;

        private static readonly GlobalJudgementWindow[] Judgements =
            (GlobalJudgementWindow[]) Enum.GetValues(typeof(GlobalJudgementWindow));

        private static readonly ModIdentifier[] Mods =
        {
            ModIdentifier.Mirror,
            ModIdentifier.Autoplay,
            ModIdentifier.Coop,
            ModIdentifier.NoFail,
            ModIdentifier.NoSliderVelocity,
            ModIdentifier.NoMiss,
            ModIdentifier.NoMines,
            ModIdentifier.NoLongNotes,
            ModIdentifier.FullLN,
            ModIdentifier.Inverse,
            ModIdentifier.Randomize,
            ModIdentifier.HeatlthAdjust,
            ModIdentifier.NoPause,
            ModIdentifier.Paused,
            ModIdentifier.None
        };

        private static readonly float[] Rates = CreateRates();

        private List<AssetEntry> Entries { get; } = new List<AssetEntry>();

        private ScrollContainer ScreenScrollContainer { get; }

        private FlexContainer AssetGrid { get; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        public GlobalGameplayAssetsTestScreenView(Screen screen) : base(screen)
        {
            CreateEntries();

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

            AssetGrid = new FlexContainer
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

            for (var i = 0; i < Entries.Count; i++)
            {
                var card = CreateAssetCard(Entries[i], i);
                AssetGrid.SetItemOptions(card, new FlexItemOptions
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

        private void CreateEntries()
        {
            foreach (var judgement in Judgements)
            {
                Entries.Add(new AssetEntry(
                    $"Judgement: {judgement}",
                    "Single sprite",
                    GlobalGameplayAssetStore.JudgementDisplaySize,
                    GlobalGameplayAssetStore.GetJudgement(judgement)));
            }

            foreach (var mod in Mods)
            {
                Entries.Add(new AssetEntry(
                    $"Mod: {mod}",
                    "Active / inactive",
                    GlobalGameplayAssetStore.ModDisplaySize,
                    GlobalGameplayAssetStore.GetMod(mod),
                    GlobalGameplayAssetStore.GetMod(mod, true)));
            }

            Entries.Add(new AssetEntry(
                "Mod badge: More",
                "Active / inactive",
                GlobalGameplayAssetStore.ModDisplaySize,
                GlobalGameplayAssetStore.GetModBadge(GlobalModBadge.More),
                GlobalGameplayAssetStore.GetModBadge(GlobalModBadge.More, true)));

            foreach (var rate in Rates)
            {
                Entries.Add(new AssetEntry(
                    $"Rate: {rate:0.00}x",
                    "Single sprite",
                    GlobalGameplayAssetStore.RateDisplaySize,
                    GlobalGameplayAssetStore.GetRate(rate)));
            }
        }

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "GLOBAL GAMEPLAY ASSETS", 28)
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 24,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight),
                $"{Entries.Count} sprites — judgements, modifiers, badges, and rates", 17)
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 62,
                Tint = MutedTextColor,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private Container CreateAssetCard(AssetEntry entry, int index)
        {
            var card = new Container
            {
                Parent = AssetGrid,
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

            for (var i = 0; i < entry.Textures.Length; i++)
            {
                var texture = entry.Textures[i];
                new Sprite
                {
                    Parent = card,
                    Alignment = Alignment.MidLeft,
                    X = 16 + i * 68,
                    Size = new ScalableVector2(entry.DisplaySize.X, entry.DisplaySize.Y),
                    Region = texture,
                    Tint = Color.White,
                    UsePreviousSpriteBatchOptions = true
                };
            }

            var textX = entry.Textures.Length == 1 ? 102 : 156;

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium), entry.Name, 16)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = textX,
                Y = 18,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight), entry.Detail, 14)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = textX,
                Y = 44,
                Tint = MutedTextColor,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight), $"Index {index}", 13)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = textX,
                Y = 64,
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
            var rows = (int) Math.Ceiling(Entries.Count / (double) columns);
            var gridHeight = rows * CardHeight + Math.Max(0, rows - 1) * CardGap;
            var contentHeight = Math.Max(height + 1, HeaderHeight + gridHeight + BottomPadding);

            ScreenScrollContainer.ContentContainer.Size = new ScalableVector2(width, contentHeight);
            AssetGrid.Size = new ScalableVector2(gridWidth, gridHeight);
            AssetGrid.RefreshLayout();

            LastWindowWidth = width;
            LastWindowHeight = height;
        }

        private static float[] CreateRates()
        {
            var rates = new float[31];
            for (var i = 0; i < rates.Length; i++)
                rates[i] = (50 + i * 5) / 100f;

            return rates;
        }

        private sealed class AssetEntry
        {
            public string Name { get; }

            public string Detail { get; }

            public Point DisplaySize { get; }

            public TextureRegion[] Textures { get; }

            public AssetEntry(string name, string detail, Point displaySize, params TextureRegion[] textures)
            {
                Name = name;
                Detail = detail;
                DisplaySize = displaySize;
                Textures = textures;
            }
        }
    }
}
