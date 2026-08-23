/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using FlagAssets = Quaver.Shared.Assets.Flags;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tests.Flags
{
    public sealed class FlagsTestScreenView : ScreenView
    {
        private static readonly Color BackgroundColor = new Color(17, 24, 32);

        private static readonly Color PanelColor = new Color(31, 41, 51);

        private static readonly Color BorderColor = new Color(56, 69, 82);

        private static readonly Color MutedTextColor = new Color(151, 164, 176);

        private static readonly Color ScrollbarColor = new Color(86, 97, 107);

        private const float HeaderHeight = 112;

        private const float OuterPadding = 36;

        private const float BottomPadding = 36;

        private const float CardWidth = 190;

        private const float CardHeight = 104;

        private const float CardGap = 14;

        private List<string> CountryCodes { get; } = new List<string>(FlagAssets.CountryCodes);

        private ScrollContainer ScreenScrollContainer { get; }

        private FlexContainer FlagGrid { get; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        public FlagsTestScreenView(Screen screen) : base(screen)
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

            FlagGrid = new FlexContainer
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

            for (var i = 0; i < CountryCodes.Count; i++)
            {
                var card = CreateFlagCard(CountryCodes[i], i);
                FlagGrid.SetItemOptions(card, new FlexItemOptions
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
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), "FLAGS", 28)
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 24,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight),
                $"{CountryCodes.Count} flags — labels use string country codes", 17)
            {
                Parent = ScreenScrollContainer.ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 62,
                Tint = MutedTextColor,
                UsePreviousSpriteBatchOptions = true
            };
        }

        private Container CreateFlagCard(string countryCode, int index)
        {
            var card = new Container
            {
                Parent = FlagGrid,
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
                X = 12,
                Size = new ScalableVector2(72, 72),
                Region = FlagAssets.GetRegion(countryCode),
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium), countryCode, 18)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = 98,
                Y = 31,
                Tint = Color.White,
                UsePreviousSpriteBatchOptions = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterLight), $"Index {index}", 14)
            {
                Parent = card,
                Alignment = Alignment.TopLeft,
                X = 98,
                Y = 58,
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
            var rows = (int) Math.Ceiling(CountryCodes.Count / (double) columns);
            var gridHeight = rows * CardHeight + Math.Max(0, rows - 1) * CardGap;
            var contentHeight = Math.Max(height + 1, HeaderHeight + gridHeight + BottomPadding);

            ScreenScrollContainer.ContentContainer.Size = new ScalableVector2(width, contentHeight);
            FlagGrid.Size = new ScalableVector2(gridWidth, gridHeight);
            FlagGrid.RefreshLayout();

            LastWindowWidth = width;
            LastWindowHeight = height;
        }
    }
}
