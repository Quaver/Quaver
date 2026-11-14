using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Server.Client;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Profile
{
    public sealed class ProfileViewerDialog : DialogScreen
    {
        private User User { get; }

        private SkinStoreV2Lease Skin { get; }

        private ProfileViewerStyle Config { get; }

        private ProfileViewerPanel Panel { get; set; }

        private HorizontalClippingContainer RevealClip { get; set; }

        private Task<ProfileViewerData> DataTask { get; }

        private ProfileViewerData Data { get; set; }

        private RectangleF? Anchor { get; }

        private bool IsClosing { get; set; }

        private bool IsContentCreated { get; set; }

        private bool RevealFromBottom { get; set; }

        public ProfileViewerDialog(User user, RectangleF? anchor = null) : base(0.5f)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            if (User.OnlineUser == null)
                throw new ArgumentException("The profile viewer requires user information.", nameof(user));

            Anchor = anchor;
            Skin = SkinManager.AcquireV2();
            Config = ProfileViewerStyle.Default;
            DataTask = ProfileViewerDataCache.GetViewerDataAsync(User);

            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
        }

        public static void Show(User user, RectangleF? anchor = null)
        {
            if (user?.OnlineUser == null)
                return;

            foreach (var existing in DialogManager.Dialogs.OfType<ProfileViewerDialog>().ToArray())
                DialogManager.Dismiss(existing);

            DialogManager.Show(new ProfileViewerDialog(user, anchor));
        }

        public override void CreateContent()
        {
            if (IsContentCreated)
                return;

            IsContentCreated = true;
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;

            RevealClip = new HorizontalClippingContainer
            {
                Parent = Container,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Config.Width, Config.Height)
            };

            Panel = new ProfileViewerPanel(User, Data, Skin, Close, Container)
            {
                Parent = RevealClip,
                Alignment = Alignment.TopLeft
            };

            PositionPanel();
            BeginOpenAnimation();
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsClosing && !IsContentCreated && DataTask.IsCompleted)
            {
                Data = DataTask.Status == TaskStatus.RanToCompletion ? DataTask.Result : new ProfileViewerData(null, null);
                CreateContent();
            }

            base.Update(gameTime);
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (IsClosing)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                Close();
                return;
            }

            if (!IsContentCreated)
                return;

            if (MouseManager.IsUniquePress(MouseButton.Left) && !Panel.IsInteractionHovered())
                Close();
        }

        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;
            ClearAnimations();
            FadeTo(0, Easing.Linear, Config.AnimationDurationMilliseconds);
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), Config.AnimationDurationMilliseconds + 20);
        }

        public override void Destroy()
        {
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;
            OnlineManager.Status.ValueChanged -= OnConnectionStatusChanged;
            base.Destroy();
            Skin.Dispose();
        }

        private void OnConnectionStatusChanged(object sender, BindableValueChangedEventArgs<ConnectionStatus> args) => AddScheduledUpdate(() =>
        {
            if (!IsDisposed)
                Close();
        });

        private void OnVirtualScreenSizeChanged(object sender, WindowVirtualScreenSizeChangedEventArgs args) => AddScheduledUpdate(() =>
            {
                if (IsDisposed)
                    return;

                Size = new ScalableVector2(args.Size.X, args.Size.Y);
                Container.Size = Size;
                PositionPanel();
            });

        private void PositionPanel()
        {
            if (Panel == null || RevealClip == null)
                return;

            var safetyGap = ScreenNavigation.TopSafetyGap;
            var maximumX = Math.Max(safetyGap, WindowManager.Width - safetyGap - Panel.Width);
            var maximumY = Math.Max(safetyGap, WindowManager.Height - safetyGap - Panel.Height);
            float x;
            float y;

            if (Anchor.HasValue)
            {
                var anchor = Anchor.Value;
                var gap = Config.AnchorGap;
                var availableBelow = WindowManager.Height - safetyGap - anchor.Bottom - gap;
                var availableAbove = anchor.Top - gap - safetyGap;
                var opensBelow = availableBelow >= Panel.Height || availableBelow >= availableAbove;
                RevealFromBottom = !opensBelow;
                x = anchor.Left;
                y = opensBelow ? anchor.Bottom + gap : anchor.Top - gap - Panel.Height;
            }
            else
            {
                RevealFromBottom = false;
                x = (WindowManager.Width - Panel.Width) / 2f;
                y = (WindowManager.Height - Panel.Height) / 2f;
            }

            RevealClip.ClearAnimations();
            Panel.ClearAnimations();
            RevealClip.Size = Panel.Size;
            Panel.Position = new ScalableVector2(0, 0);
            RevealClip.Position = new ScalableVector2(
                MathHelper.Clamp(x, safetyGap, maximumX) - Container.ScreenRectangle.Left,
                MathHelper.Clamp(y, safetyGap, maximumY) - Container.ScreenRectangle.Top);
        }

        private void BeginOpenAnimation()
        {
            var duration = Math.Max(1, Config.AnimationDurationMilliseconds);
            var finalPosition = new Vector2(RevealClip.X, RevealClip.Y);
            RevealClip.Height = 1;

            if (RevealFromBottom)
            {
                var offset = Panel.Height - 1;
                RevealClip.Y = finalPosition.Y + offset;
                Panel.Y = -offset;
                RevealClip.MoveToPosition(finalPosition, Easing.OutQuint, duration);
                Panel.MoveToPosition(Vector2.Zero, Easing.OutQuint, duration);
            }

            RevealClip.ChangeHeightTo((int) Math.Round(Panel.Height), Easing.OutQuint, duration);
            Panel.BeginOpenAnimation(duration);
        }
    }
}
