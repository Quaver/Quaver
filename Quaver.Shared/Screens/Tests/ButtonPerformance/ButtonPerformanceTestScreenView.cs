using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Buttons;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Debugging;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Screens;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Tests.ButtonPerformance
{
    public class ButtonPerformanceTestScreenView : ScreenView
    {
        private const int VisibleButtonCount = 10;
        private const int ButtonCount = 20;
        private const int ButtonWidth = 220;
        private const int ButtonHeight = 42;
        private const int ButtonSpacing = 10;
        private const int StatsRefreshTime = 100;

        private Container NewButtonGroup { get; }

        private Container OldButtonGroup { get; }

        private SpriteTextPlus NewStateText { get; }

        private SpriteTextPlus OldStateText { get; }

        private List<SpriteTextPlus> StatLines { get; } = new List<SpriteTextPlus>();

        private List<ScrollContainer> ButtonScrollContainers { get; } = new List<ScrollContainer>();

        private double StatsRefreshTimer { get; set; }

        public ButtonPerformanceTestScreenView(Screen screen) : base(screen)
        {
            CreateHeader();
            CreateToggleControls();

            NewButtonGroup = CreateButtonGroup("NEW ROUNDED BUTTONS", -170, CreateRoundedButton);
            OldButtonGroup = CreateButtonGroup("OLD IMAGE BUTTONS", 170, CreateImageButton);

            NewStateText = CreateGroupStateText("New buttons: ON", -170);
            OldStateText = CreateGroupStateText("Old image buttons: ON", 170);

            CreateStatsPanel();
            UpdateStateText();
            UpdateStatsText();
        }

        private void CreateHeader()
        {
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterHeavy), "BUTTON PERFORMANCE", 26)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 30,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterRegular), "1: new buttons  |  2: old image buttons  |  R: reset both", 18)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 66,
                Tint = ColorHelper.HexToColor("#B8C3CC")
            };
        }

        private void CreateToggleControls()
        {
            CreateToggleControl("TOGGLE NEW", -240, (sender, args) => ToggleNewButtons());
            CreateToggleControl("TOGGLE OLD", 0, (sender, args) => ToggleOldButtons());
            CreateToggleControl("RESET", 240, (sender, args) => ResetButtons());
        }

        private void CreateToggleControl(string text, float x, EventHandler clicked)
        {
            var button = new ImageButton(UserInterface.BlankButton, clicked)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = 105,
                Size = new ScalableVector2(190, 36),
                Tint = ColorHelper.HexToColor("#1F2933")
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), text, 15)
            {
                Parent = button,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };
        }

        private Container CreateButtonGroup(string title, float x, Func<int, Drawable> createButton)
        {
            var group = new Container
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = x,
                Y = 170,
                Size = new ScalableVector2(ButtonWidth, 40 + VisibleButtonCount * (ButtonHeight + ButtonSpacing) - ButtonSpacing),
                SetChildrenVisibility = true
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), title, 17)
            {
                Parent = group,
                Alignment = Alignment.TopCenter,
                Tint = Color.White
            };

            var scrollContainer = new ScrollContainer(
                new ScalableVector2(ButtonWidth + 18, VisibleButtonCount * (ButtonHeight + ButtonSpacing) - ButtonSpacing),
                new ScalableVector2(ButtonWidth + 18, ButtonCount * (ButtonHeight + ButtonSpacing) - ButtonSpacing))
            {
                Parent = group,
                Alignment = Alignment.TopCenter,
                Y = 40,
                InputEnabled = true,
                AllowScrollbarDragging = true,
                ScrollSpeed = ButtonHeight + ButtonSpacing,
                Tint = Color.Transparent
            };

            scrollContainer.Scrollbar.Tint = ColorHelper.HexToColor("#56616B");
            scrollContainer.Scrollbar.Width = 6;
            ButtonScrollContainers.Add(scrollContainer);

            for (var i = 0; i < ButtonCount; i++)
            {
                var button = createButton(i);
                button.Alignment = Alignment.TopCenter;
                button.Y = i * (ButtonHeight + ButtonSpacing);
                scrollContainer.AddContainedDrawable(button);
            }

            UpdateScrollButtonVisibility(scrollContainer);
            return group;
        }

        private Drawable CreateRoundedButton(int index)
        {
            var button = new RoundedButton
            {
                Size = new ScalableVector2(ButtonWidth, ButtonHeight),
                Tint = ColorHelper.HexToColor(index % 2 == 0 ? "#0FBAE5" : "#755CDE"),
                PerformHoverFade = false
            };

            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), $"NEW BUTTON {index + 1}", 16, Color.White);
            button.Clicked += (sender, args) => { };

            return button;
        }

        private Drawable CreateImageButton(int index)
        {
            var button = new ImageButton(UserInterface.BlankButton)
            {
                Size = new ScalableVector2(ButtonWidth, ButtonHeight),
                Tint = ColorHelper.HexToColor(index % 2 == 0 ? "#0FBAE5" : "#755CDE")
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), $"OLD BUTTON {index + 1}", 16)
            {
                Parent = button,
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };

            button.Clicked += (sender, args) => { };

            return button;
        }

        private SpriteTextPlus CreateGroupStateText(string text, float x) => new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), text, 18)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            X = x,
            Y = 715,
            Tint = Color.White
        };

        private void CreateStatsPanel()
        {
            var panel = new Container
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = -30,
                Y = 30,
                Size = new ScalableVector2(320, 210)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterHeavy), "LIVE STATS", 18)
            {
                Parent = panel,
                Alignment = Alignment.TopLeft,
                Tint = Color.White
            };

            for (var i = 0; i < 8; i++)
            {
                StatLines.Add(new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterRegular), string.Empty, 16)
                {
                    Parent = panel,
                    Alignment = Alignment.TopLeft,
                    Y = 30 + i * 21,
                    Tint = ColorHelper.HexToColor("#C9D2DA")
                });
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.D1) || KeyboardManager.IsUniqueKeyPress(Keys.NumPad1))
                ToggleNewButtons();

            if (KeyboardManager.IsUniqueKeyPress(Keys.D2) || KeyboardManager.IsUniqueKeyPress(Keys.NumPad2))
                ToggleOldButtons();

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
                ResetButtons();

            StatsRefreshTimer += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (StatsRefreshTimer >= StatsRefreshTime)
            {
                StatsRefreshTimer = 0;
                UpdateStatsText();
            }

            Container?.Update(gameTime);
            UpdateScrollButtonVisibility();
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#111820"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();

        private void ToggleNewButtons()
        {
            NewButtonGroup.Visible = !NewButtonGroup.Visible;
            UpdateStateText();
        }

        private void ToggleOldButtons()
        {
            OldButtonGroup.Visible = !OldButtonGroup.Visible;
            UpdateStateText();
        }

        private void ResetButtons()
        {
            NewButtonGroup.Visible = true;
            OldButtonGroup.Visible = true;
            UpdateStateText();
        }

        private void UpdateStateText()
        {
            NewStateText.Text = $"New buttons: {(NewButtonGroup.Visible ? "ON" : "OFF")}";
            NewStateText.Tint = NewButtonGroup.Visible ? ColorHelper.HexToColor("#69E6A6") : ColorHelper.HexToColor("#FF7777");

            OldStateText.Text = $"Old image buttons: {(OldButtonGroup.Visible ? "ON" : "OFF")}";
            OldStateText.Tint = OldButtonGroup.Visible ? ColorHelper.HexToColor("#69E6A6") : ColorHelper.HexToColor("#FF7777");
        }

        private void UpdateScrollButtonVisibility()
        {
            foreach (var scrollContainer in ButtonScrollContainers)
                UpdateScrollButtonVisibility(scrollContainer);
        }

        private static void UpdateScrollButtonVisibility(ScrollContainer scrollContainer)
        {
            foreach (var drawable in scrollContainer.ContentContainer.Children)
                drawable.Visible = !RectangleF.Intersection(drawable.ScreenRectangle, scrollContainer.ScreenRectangle).IsEmpty;
        }

        private void UpdateStatsText()
        {
            StatLines[0].Text = $"FPS / UPS: {PerformanceStats.FrameRate} / {PerformanceStats.UpdateRate}";
            StatLines[1].Text = $"Frame: {PerformanceStats.FrameTimeMs:0.00} ms avg {PerformanceStats.AverageFrameTimeMs:0.00} ms";
            StatLines[2].Text = $"Draw: {PerformanceStats.DrawTimeMs:0.00} ms avg {PerformanceStats.AverageDrawTimeMs:0.00} ms";
            StatLines[3].Text = $"Screen draw: {PerformanceStats.ScreenDrawTimeMs:0.00} ms";
            StatLines[4].Text = $"Drawn drawables: {PerformanceStats.DrawnDrawableCount}";
            StatLines[5].Text = $"New group: {(NewButtonGroup.Visible ? "visible" : "hidden")}";
            StatLines[6].Text = $"Old group: {(OldButtonGroup.Visible ? "visible" : "hidden")}";
            StatLines[7].Text = $"Buttons per group: {ButtonCount} ({VisibleButtonCount} visible)";
        }
    }
}
