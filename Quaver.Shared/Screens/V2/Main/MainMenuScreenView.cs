using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.V2.Main.UI;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Managers;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Main
{
    /// <summary>
    ///     View for the rewritten main menu.
    /// </summary>
    public sealed class MainMenuScreenView : ScreenView
    {
        private const float HorizontalPadding = 160;
        private const float NavigationHeight = 90;
        private const float ActionGap = 34;

        private static readonly Color ActionColor = new Color(43, 53, 63, 235);
        private static readonly Color ActionHoverColor = new Color(56, 72, 84, 245);
        private static readonly Color AccentColor = ColorHelper.HexToColor("#45D6F5");

        private BackgroundImage Background { get; }

        private FlexContainer Content { get; }

        private Sprite Logo { get; }

        private FlexItemOptions LogoOptions { get; }

        private FlexContainer ActionRow { get; }

        private List<FlexItemOptions> ActionOptions { get; } = new List<FlexItemOptions>();

        private MainMenuNewsCard News { get; }

        private float LastWindowWidth { get; set; } = -1;

        private float LastWindowHeight { get; set; } = -1;

        public MainMenuScreenView(MainMenuScreen screen) : base(screen)
        {
            Background = new BackgroundImage(UserInterface.MainMenuScreenBackground, 0, false)
            {
                Parent = Container
            };

            Content = new FlexContainer
            {
                Parent = Container,
                Position = new ScalableVector2(HorizontalPadding, NavigationHeight),
                Size = new ScalableVector2(WindowManager.Width - HorizontalPadding * 2,
                    WindowManager.Height - NavigationHeight * 2),
                Direction = FlexDirection.Column,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.Center,
                RowGap = 55
            };

            var logoTexture = UserInterface.MainMenuLogo;

            Logo = new Sprite
            {
                Parent = Content,
                Image = logoTexture,
                Size = new ScalableVector2(logoTexture.Width, logoTexture.Height)
            };
            LogoOptions = new FlexItemOptions { Basis = logoTexture.Height, Shrink = 1 };
            Content.SetItemOptions(Logo, LogoOptions);

            ActionRow = new FlexContainer
            {
                Parent = Content,
                Size = new ScalableVector2(Content.Width, 76),
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustifyContent.Center,
                AlignItems = FlexAlignItems.Center,
                RowGap = 20,
                ColumnGap = ActionGap
            };
            Content.SetItemOptions(ActionRow, new FlexItemOptions { Basis = 76, Shrink = 0 });

            CreateAction(FontAwesomeIcon.fa_gamepad_console, "Screen_Main_SinglePlayer",
                screen.ExitToSinglePlayer);
            CreateAction(FontAwesomeIcon.fa_group_profile_users, "Screen_Main_Multiplayer",
                screen.ExitToMultiplayer);
            CreateAction(FontAwesomeIcon.fa_pencil, "Screen_Main_Editor", screen.ExitToEditor);
            CreateAction(FontAwesomeIcon.fa_download_to_storage_drive, "Screen_Main_DownloadSongs",
                screen.ExitToDownload);

            News = new MainMenuNewsCard(570)
            {
                Parent = Container,
                Alignment = Alignment.BotCenter,
                Y = -20
            };

            UpdateResponsiveLayout(true);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateResponsiveLayout();
            Container.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#080D13"));
            Container.Draw(gameTime);
        }

        public override void Destroy() => Container.Destroy();

        private void CreateAction(FontAwesomeIcon icon, string localizationKey, Action action)
        {
            var button = new MainActionButton(action)
            {
                Parent = ActionRow,
                Size = new ScalableVector2(340, 64),
                Tint = ActionColor,
                CornerRadius = 6,
                PerformHoverFade = false,
                NormalColor = ActionColor,
                HoverColor = ActionHoverColor,
                AccentColor = AccentColor
            };
            button.SetIcon(FontAwesome.Get(icon), new Vector2(24, 24));
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterSemiBold), LocalizationManager.Get(localizationKey),
                22, Color.White);

            var options = new FlexItemOptions { Basis = 340, Grow = 1, Shrink = 1 };
            ActionOptions.Add(options);
            ActionRow.SetItemOptions(button, options);
        }

        private void UpdateResponsiveLayout(bool force = false)
        {
            var width = WindowManager.Width;
            var height = WindowManager.Height;

            if (!force && Math.Abs(width - LastWindowWidth) < 0.001f &&
                Math.Abs(height - LastWindowHeight) < 0.001f)
                return;

            LastWindowWidth = width;
            LastWindowHeight = height;

            // The root container captures the window size when it is constructed, so keep it
            // synchronized after resizes. Bottom-aligned children (such as the news card) are
            // recalculated when this changes.
            Container.Size = new ScalableVector2(width, height);

            var contentWidth = Math.Max(640, width - HorizontalPadding * 2);
            var contentHeight = Math.Max(620, height - NavigationHeight * 2);
            Content.Position = new ScalableVector2((width - contentWidth) / 2f, NavigationHeight);
            Content.Size = new ScalableVector2(contentWidth, contentHeight);

            var fourColumns = contentWidth >= 1400;
            var columns = fourColumns ? 4 : 2;
            var actionWidth = Math.Min(contentWidth, fourColumns ? contentWidth : Math.Max(640, contentWidth - 80));
            var basis = (actionWidth - ActionGap * (columns - 1)) / columns;

            ActionRow.Size = new ScalableVector2(actionWidth, fourColumns ? 76 : 160);
            foreach (var options in ActionOptions)
                options.Basis = basis;

            Content.SetItemOptions(ActionRow,
                new FlexItemOptions { Basis = fourColumns ? 76 : 160, Shrink = 0 });

            var logoTexture = Logo.Image;
            var logoWidth = Math.Min(logoTexture.Width,
                Math.Max(320, Math.Min(contentWidth - 40, width * (800f / 1920f))));
            var logoHeight = logoWidth * logoTexture.Height / logoTexture.Width;
            Logo.Size = new ScalableVector2(logoWidth, logoHeight);
            LogoOptions.Basis = logoHeight;

            News.ApplyWidth(Math.Min(570, contentWidth - 40));
            Content.RefreshLayout();
            ActionRow.RefreshLayout();
        }

        private sealed class MainActionButton : RoundedButton
        {
            private RoundedButton Indicator { get; }

            public Color NormalColor { get; set; }

            public Color HoverColor { get; set; }

            public Color AccentColor
            {
                set => Indicator.Tint = value;
            }

            public MainActionButton(Action action) : base((sender, args) => action())
            {
                Indicator = new RoundedButton
                {
                    Parent = this,
                    Alignment = Alignment.BotCenter,
                    Position = new ScalableVector2(0, 12),
                    Size = new ScalableVector2(60, 9),
                    Tint = Color.White,
                    Alpha = 0.45f,
                    PerformHoverFade = false,
                    IsClickable = false,
                    IsInteractionEnabled = false
                };
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                Tint = IsHovered ? HoverColor : NormalColor;
                Indicator.Alpha = IsHovered ? 1 : 0.45f;
            }
        }
    }
}
