using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Screens.V2.Music.UI;
using Quaver.Shared.Screens.V2.SkinEditor;
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
    internal sealed class MusicQueueDialog : DialogScreen
    {
        private Drawable Anchor { get; }

        private SkinV2MusicQueueDialogConfig Config { get; }

        private SkinV2VolumeDialogConfig PlacementConfig { get; }

        private MusicPlaybackController Playback { get; }

        private bool PreviewMode { get; }

        private Bindable<string> Query { get; } = new Bindable<string>(string.Empty);

        private MusicQueuePanel Panel { get; set; }

        private bool Closing { get; set; }

        private bool DismissRequested { get; set; }

        private double CloseTimeRemaining { get; set; }

        private float OpenX { get; set; }

        public bool IsClosing => Closing;

        public MusicQueueDialog(Drawable anchor, SkinV2FooterMusicPlayerConfig musicPlayerConfig, MusicPlaybackController playback) : base(0)
        {
            Anchor = anchor ?? throw new ArgumentNullException(nameof(anchor));
            if (musicPlayerConfig == null)
                throw new ArgumentNullException(nameof(musicPlayerConfig));

            Playback = playback ?? throw new ArgumentNullException(nameof(playback));
            Config = musicPlayerConfig.MusicQueueDialog;
            PlacementConfig = musicPlayerConfig.VolumeDialog;

            CreateContent();
            BeginOpenAnimation();
            FadeTo(PlacementConfig.DimOpacity, Easing.Linear, PlacementConfig.AnimationDuration);

            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
        }

        internal MusicQueueDialog(SkinV2FooterMusicPlayerConfig musicPlayerConfig, MusicPlaybackController playback, Drawable anchor) : base(0)
        {
            if (musicPlayerConfig == null)
                throw new ArgumentNullException(nameof(musicPlayerConfig));

            Anchor = anchor;
            Playback = playback ?? throw new ArgumentNullException(nameof(playback));
            Config = musicPlayerConfig.MusicQueueDialog;
            PlacementConfig = musicPlayerConfig.VolumeDialog;
            PreviewMode = true;
            Alpha = 0;

            CreateContent();
            RefreshPreviewLayout();
        }

        public override void CreateContent()
        {
            if (Panel != null)
                return;

            Container.Size = Size;
            Panel = new MusicQueuePanel(GetPanelSize(), Query, Config)
            {
                Parent = Container
            };
            Panel.SetPlaybackController(Playback);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Container.Size = Size;

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

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) ||
                MouseManager.IsUniqueClick(MouseButton.Left) && !Panel.IsHovered())
                Close();
        }

        public void Close()
        {
            if (PreviewMode || Closing)
                return;

            Closing = true;
            CloseTimeRemaining = PlacementConfig.AnimationDuration;
            ClearAnimations();
            FadeTo(0, Easing.Linear, PlacementConfig.AnimationDuration);
            Panel.ClearAnimations();
            FadePanel(Panel, PlacementConfig.AnimationDuration);
        }

        public override void Destroy()
        {
            if (!PreviewMode)
                WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;

            base.Destroy();
        }

        internal static SkinEditorTarget CreateSkinEditorTarget(Drawable drawable) =>
            new SkinEditorTarget("music-queue-dialog", LocalizationManager.Get("SkinEditor_Component_MusicQueue"), "Shared.MusicPlayer.MusicQueueDialog", drawable);

        internal void RefreshPreviewLayout()
        {
            if (!PreviewMode)
                return;

            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;
            Panel.Size = GetPanelSize();
            Panel.ClearAnimations();
            PositionPanel();
        }

        private void BeginOpenAnimation()
        {
            PositionPanel();
            Panel.X = OpenX + GetSlideOffset();
            Panel.MoveToX(OpenX, Easing.OutQuint, PlacementConfig.AnimationDuration);
        }

        private float GetSlideOffset() => PlacementConfig.SlideFromLeft ? -PlacementConfig.SlideDistance : PlacementConfig.SlideDistance;

        private static void FadePanel(Drawable drawable, int duration)
        {
            if (drawable is Sprite sprite)
                sprite.FadeTo(0, Easing.Linear, duration);

            foreach (var child in drawable.Children)
                FadePanel(child, duration);
        }

        private ScalableVector2 GetPanelSize() => new ScalableVector2(
            Math.Min(Config.Width, Math.Max(1, Width - PlacementConfig.WindowInset * 2)),
            Math.Min(Config.Height, Math.Max(1, Height - PlacementConfig.WindowInset * 2)));

        private void PositionPanel()
        {
            if (Anchor == null || Anchor.IsDisposed)
            {
                OpenX = MathHelper.Clamp((Width - Panel.Width) / 2f, PlacementConfig.WindowInset, Math.Max(PlacementConfig.WindowInset, Width - PlacementConfig.WindowInset - Panel.Width));
                Panel.Position = new ScalableVector2(OpenX, MathHelper.Clamp((Height - Panel.Height) / 2f, PlacementConfig.WindowInset, Math.Max(PlacementConfig.WindowInset, Height - PlacementConfig.WindowInset - Panel.Height)));
                return;
            }

            var anchor = Anchor.ScreenRectangle;
            var container = Container.ScreenRectangle;
            var scaleX = Math.Abs(Container.AbsoluteScale.X) < float.Epsilon ? 1 : Container.AbsoluteScale.X;
            var scaleY = Math.Abs(Container.AbsoluteScale.Y) < float.Epsilon ? 1 : Container.AbsoluteScale.Y;
            var anchorLeft = (anchor.Left - container.Left) / scaleX;
            var anchorRight = (anchor.Right - container.Left) / scaleX;
            var anchorCenterY = (anchor.Center.Y - container.Top) / scaleY;
            var rightX = anchorRight + PlacementConfig.AnchorGap;
            var anchorsRight = rightX + Panel.Width <= Width - PlacementConfig.WindowInset;
            OpenX = (anchorsRight ? rightX : anchorLeft - PlacementConfig.AnchorGap - Panel.Width) + PlacementConfig.HorizontalOffset;
            OpenX = MathHelper.Clamp(OpenX, PlacementConfig.WindowInset, Math.Max(PlacementConfig.WindowInset, Width - PlacementConfig.WindowInset - Panel.Width));

            Panel.Position = new ScalableVector2(OpenX, MathHelper.Clamp(anchorCenterY - Panel.Height / 2f, PlacementConfig.WindowInset, Math.Max(PlacementConfig.WindowInset, Height - PlacementConfig.WindowInset - Panel.Height)));
        }

        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs args)
        {
            Size = new ScalableVector2(args.Size.X, args.Size.Y);
            Container.Size = Size;
            Panel.Size = GetPanelSize();

            if (!Closing)
            {
                Panel.ClearAnimations();
                PositionPanel();
            }
        }
    }
}
