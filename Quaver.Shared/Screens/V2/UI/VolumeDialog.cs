using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.UI
{
    internal sealed class VolumeDialog : DialogScreen
    {
        private const int KeyboardVolumeStep = 5;

        private SkinStoreV2Lease Skin { get; }

        private bool OwnsSkin { get; }

        private SkinV2VolumeDialogConfig Config { get; }

        private Drawable Anchor { get; }

        private bool PreviewMode { get; }

        private bool UsesConfiguredPosition { get; }

        private Sprite Panel { get; set; }

        private FlexContainer SliderLayout { get; set; }

        private List<VolumeSlider> Sliders { get; } = new List<VolumeSlider>();

        private VolumeSlider FocusedSlider { get; set; }

        private bool AnchorsRight { get; set; }

        private bool Closing { get; set; }

        private bool DismissRequested { get; set; }

        private double CloseTimeRemaining { get; set; }

        private float OpenX { get; set; }

        private bool KeyboardControlled { get; set; }

        public bool IsClosing => Closing;

        public VolumeDialog(Drawable anchor, SkinV2FooterMusicPlayerConfig musicPlayerConfig, bool keyboardControlled = false) : base(0)
        {
            Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            if (musicPlayerConfig == null)
                throw new ArgumentNullException(nameof(musicPlayerConfig));

            Skin = SkinManager.AcquireV2();
            OwnsSkin = true;
            Config = musicPlayerConfig.VolumeDialog;
            KeyboardControlled = keyboardControlled;
            UsesConfiguredPosition = keyboardControlled;
            CreateContent();
            BeginOpenAnimation();
            FadeTo(Config.DimOpacity, Easing.Linear, Config.AnimationDuration);
            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
        }

        internal VolumeDialog(SkinStoreV2Lease skin, SkinV2FooterMusicPlayerConfig musicPlayerConfig,
            Drawable anchor) : base(0)
        {
            Skin = skin ?? throw new ArgumentNullException(nameof(skin));
            if (musicPlayerConfig == null)
                throw new ArgumentNullException(nameof(musicPlayerConfig));

            Anchor = anchor;
            Config = musicPlayerConfig.VolumeDialog;
            PreviewMode = true;
            UsesConfiguredPosition = true;
            Alpha = 0;
            CreateContent();
            RefreshPreviewLayout();
        }

        public override void CreateContent()
        {
            if (Panel != null)
                return;

            Container.Size = Size;
            var panelWidth = Config.CardWidth * 3 + Config.CardGap * 2;

            Panel = new Sprite
            {
                Parent = Container,
                Size = new ScalableVector2(panelWidth, Config.CardHeight),
                Tint = Color.Transparent
            };

            SliderLayout = new FlexContainer
            {
                Parent = Panel,
                Size = Panel.Size,
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.FlexStart,
                ColumnGap = Config.CardGap
            };

            AddVolumeSlider(UserInterface.MasterVolumeIcon, ConfigManager.VolumeGlobal);
            AddVolumeSlider(UserInterface.MusicVolumeIcon, ConfigManager.VolumeMusic);
            AddVolumeSlider(UserInterface.EffectVolumeIcon, ConfigManager.VolumeEffect);
            SliderLayout.RefreshLayout();
            SetFocusedSlider(Sliders[0]);
        }

        private void AddVolumeSlider(Texture2D icon, BindableInt value)
        {
            var slider = new VolumeSlider(icon, value, Config)
            {
                Parent = SliderLayout
            };
            SliderLayout.SetItemOptions(slider, new FlexItemOptions { Basis = Config.CardWidth, Shrink = 0 });
            Sliders.Add(slider);
        }

        private void SetFocusedSlider(VolumeSlider slider)
        {
            if (FocusedSlider == slider)
                return;

            FocusedSlider?.SetFocused(false);
            FocusedSlider = slider;
            FocusedSlider.SetFocused(true);
        }

        private void BeginOpenAnimation()
        {
            PositionPanel();
            Panel.X = OpenX + (Config.SlideFromLeft ? -Config.SlideDistance : Config.SlideDistance);
            Panel.MoveToX(OpenX, Easing.OutQuint, Config.AnimationDuration);
        }

        private void PositionPanel()
        {
            if (Anchor == null || Anchor.IsDisposed)
                return;

            var anchor = Anchor.ScreenRectangle;
            var container = Container.ScreenRectangle;
            var scaleX = Math.Abs(Container.AbsoluteScale.X) < float.Epsilon
                ? 1
                : Container.AbsoluteScale.X;
            var scaleY = Math.Abs(Container.AbsoluteScale.Y) < float.Epsilon
                ? 1
                : Container.AbsoluteScale.Y;
            var anchorLeft = (anchor.Left - container.Left) / scaleX;
            var anchorRight = (anchor.Right - container.Left) / scaleX;
            var anchorCenterY = (anchor.Center.Y - container.Top) / scaleY;
            var rightX = anchorRight + Config.AnchorGap;
            AnchorsRight = rightX + Panel.Width <= Width - Config.WindowInset;
            var offsetX = UsesConfiguredPosition ? Config.PositionOffsetX : 0;
            var offsetY = UsesConfiguredPosition ? Config.PositionOffsetY : 0;
            OpenX = (AnchorsRight ? rightX : anchorLeft - Config.AnchorGap - Panel.Width) +
                    Config.HorizontalOffset + offsetX;
            OpenX = MathHelper.Clamp(OpenX, Config.WindowInset,
                Math.Max(Config.WindowInset, Width - Config.WindowInset - Panel.Width));

            Panel.Position = new ScalableVector2(OpenX,
                MathHelper.Clamp(anchorCenterY - Panel.Height / 2f + offsetY, Config.WindowInset,
                    Math.Max(Config.WindowInset, Height - Config.WindowInset - Panel.Height)));
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Container.Size = Size;

            if (!PreviewMode && !Closing)
            {
                foreach (var slider in Sliders)
                {
                    if (!slider.IsHovered())
                        continue;

                    SetFocusedSlider(slider);
                    break;
                }
            }

            if (!Closing || DismissRequested)
                return;

            CloseTimeRemaining -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (CloseTimeRemaining > 0)
                return;

            DismissRequested = true;
            DialogManager.Dismiss(this);
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (PreviewMode || Closing)
                return;

            if (KeyboardControlled)
            {
                if (!KeyboardManager.IsAltDown())
                {
                    Close();
                    return;
                }

                if (KeyboardManager.IsUniqueKeyPress(Keys.Left))
                    SelectRelativeSlider(-1);
                else if (KeyboardManager.IsUniqueKeyPress(Keys.Right))
                    SelectRelativeSlider(1);

                if (KeyboardManager.IsUniqueKeyPress(Keys.Up))
                    AdjustFocusedSlider(KeyboardVolumeStep);
                else if (KeyboardManager.IsUniqueKeyPress(Keys.Down))
                    AdjustFocusedSlider(-KeyboardVolumeStep);
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) ||
                MouseManager.IsUniqueClick(MouseButton.Left) && !Panel.IsHovered())
                Close();
        }

        public void EnableKeyboardControl() => KeyboardControlled = true;

        public void AdjustFocusedSlider(int amount) => FocusedSlider?.AdjustValue(amount);

        public void SelectRelativeSlider(int direction)
        {
            var index = Sliders.IndexOf(FocusedSlider);
            if (index < 0)
                index = 0;

            index = (index + direction + Sliders.Count) % Sliders.Count;
            SetFocusedSlider(Sliders[index]);
            SkinManager.Skin?.SoundHover?.CreateChannel()?.Play();
        }

        public void Close()
        {
            if (PreviewMode || Closing)
                return;

            Closing = true;
            CloseTimeRemaining = Config.AnimationDuration;
            ClearAnimations();
            FadeTo(0, Easing.Linear, Config.AnimationDuration);
            Panel.ClearAnimations();
            if (KeyboardControlled)
            {
                Panel.MoveToX(OpenX +
                    (Config.SlideFromLeft ? -Config.SlideDistance : Config.SlideDistance),
                    Easing.OutQuint, Config.AnimationDuration);
            }

            foreach (var slider in Sliders)
                slider.FadeOut(Config.AnimationDuration);
        }

        internal static SkinEditorTarget CreateSkinEditorTarget(Drawable drawable) =>
            new SkinEditorTarget("volume-dialog", LocalizationManager.Get("SkinEditor_Component_VolumeDialog"), "Shared.MusicPlayer.VolumeDialog", drawable);

        internal void RefreshPreviewLayout()
        {
            if (!PreviewMode)
                return;

            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;
            if (Anchor != null && !Anchor.IsDisposed)
            {
                PositionPanel();
                return;
            }

            Panel.Position = new ScalableVector2(
                MathHelper.Clamp((Width - Panel.Width) / 2f + Config.PositionOffsetX, 0,
                    Math.Max(0, Width - Panel.Width)),
                MathHelper.Clamp((Height - Panel.Height) / 2f + Config.PositionOffsetY, 0,
                    Math.Max(0, Height - Panel.Height)));
        }

        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs args)
        {
            Size = new ScalableVector2(args.Size.X, args.Size.Y);
            Container.Size = Size;

            if (!Closing)
            {
                Panel.ClearAnimations();
                PositionPanel();
            }
        }

        public override void Destroy()
        {
            if (!PreviewMode)
                WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;

            base.Destroy();
            if (OwnsSkin)
                Skin.Dispose();
        }
    }
}
