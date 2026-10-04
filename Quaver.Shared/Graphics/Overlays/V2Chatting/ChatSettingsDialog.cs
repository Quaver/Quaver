using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Tooltips;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Graphics.Overlays.V2Chatting
{
    internal sealed class ChatSettingsDialog : DialogScreen
    {

        private FlexContainer Content { get; set; }

        private Sprite Title { get; set; }

        private SpriteTextPlus Header { get; set; }

        private Sprite Banner { get; set; }

        private Sprite BannerShade { get; set; }

        private Sprite Panel { get; set; }

        private RoundedButton CensorRow { get; set; }

        private SpriteTextPlus CensorLabel { get; set; }

        private Sprite CensorHelpIcon { get; set; }

        private IDisposable CensorTooltipRegistration { get; set; }

        private ToggleV2 CensorToggle { get; set; }

        private bool Closing { get; set; }

        public ChatSettingsDialog() : base(0)
        {
            FadeTo(0.85f, Easing.Linear, 150);
            CreateContent();
            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            UpdateLayout();
        }

        public override void CreateContent()
        {
            Content = new FlexContainer
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.FlexStart,
                RowGap = 10
            };

            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Chat_Settings").ToUpperInvariant(), 20)
            {
                Alignment = Alignment.MidCenter,
                Tint = Color.White
            };

            Title = new Sprite
            {
                Parent = Content,
                Tint = ColorHelper.HexToColor("#4B5973")
            };
            Header.Parent = Title;
            Content.SetItemOptions(Title, new FlexItemOptions { Basis = 36, Shrink = 0 });

            Banner = new Sprite
            {
                Parent = Content,
                Image = UserInterface.DefaultBanner
            };
            Content.SetItemOptions(Banner, new FlexItemOptions { Basis = 128, Shrink = 0 });

            BannerShade = new Sprite
            {
                Parent = Banner,
                Tint = Color.Black,
                Alpha = 0.45f
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Chat_SettingsDescription"), 16)
            {
                Parent = Banner,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center,
                Tint = Color.White
            };

            Panel = new Sprite
            {
                Parent = Content,
                Tint = ColorHelper.HexToColor("#273038")
            };

            CensorRow = new RoundedButton
            {
                Parent = Panel,
                Position = new ScalableVector2(10, 10),
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#181E25"),
                PerformHoverFade = false,
                IsClickable = false
            };

            CensorLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Chat_CensorMessages"), 18)
            {
                Parent = CensorRow,
                Alignment = Alignment.MidLeft,
                X = 15,
                Tint = Color.White
            };

            CensorHelpIcon = new Sprite
            {
                Parent = CensorRow,
                Alignment = Alignment.MidLeft,
                X = 15 + CensorLabel.Width + 10,
                Region = GlobalIcons.Get(GlobalIcon.QuestionMark),
                Size = new ScalableVector2(30, 30),
                Tint = ColorHelper.HexToColor("#8CAFEA")
            };
            CensorTooltipRegistration = CensorHelpIcon.AddTooltip("Will only work in version 1.8");

            CensorToggle = new ToggleV2(ConfigManager.ChatCensorEnabled)
            {
                Parent = CensorRow,
                Alignment = Alignment.MidRight,
                X = -15
            };
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) ||
                MouseManager.IsUniqueClick(MouseButton.Left) && !Content.IsHovered())
                Close();
        }

        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs args) => UpdateLayout();

        private void UpdateLayout()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;

            var width = MathHelper.Max(1, MathHelper.Min(660, WindowManager.Width - 20 * 2));
            var panelHeight = 54 + 10 * 2;
            var contentHeight = 36 + 128 + panelHeight + 10 * 2;

            Content.Size = new ScalableVector2(width, contentHeight);

            Title.Size = new ScalableVector2(MathHelper.Min(width, Header.Width + 40), 36);
            Title.Image = RoundedRectTextureCache.Get(Title.Width, Title.Height, 6);

            Banner.Size = new ScalableVector2(width, 128);
            BannerShade.Size = Banner.Size;

            Panel.Size = new ScalableVector2(width, panelHeight);
            Panel.Image = RoundedRectTextureCache.Get(Panel.Width, Panel.Height, 6);
            Content.SetItemOptions(Panel, new FlexItemOptions { Basis = panelHeight, Shrink = 0 });

            CensorRow.Size = new ScalableVector2(MathHelper.Max(1, Panel.Width - 10 * 2), 54);

            Content.RefreshLayout();
        }

        private void Close()
        {
            if (Closing)
                return;

            Closing = true;
            DialogManager.Dismiss(this);
        }

        public override void Destroy()
        {
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;
            CensorTooltipRegistration?.Dispose();
            base.Destroy();
        }
    }
}
