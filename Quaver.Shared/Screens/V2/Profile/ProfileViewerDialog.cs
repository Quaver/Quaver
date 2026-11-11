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

        private Task<ProfileViewerData> DataTask { get; }

        private ProfileViewerData Data { get; set; }

        private RectangleF? Anchor { get; }

        private bool IsClosing { get; set; }

        private bool IsContentCreated { get; set; }

        public ProfileViewerDialog(User user, RectangleF? anchor = null) : base(0)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            if (User.OnlineUser == null)
                throw new ArgumentException("The profile viewer requires user information.", nameof(user));

            Anchor = anchor;
            Skin = SkinManager.AcquireV2();
            Config = ProfileViewerStyle.Default;
            DataTask = ProfileViewerDataCache.GetAsync(User);
            Tint = Color.Transparent;

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

            Panel = new ProfileViewerPanel(User, Data, Skin, Close, Container)
            {
                Parent = Container,
                Alignment = Anchor.HasValue ? Alignment.TopLeft : Alignment.MidCenter
            };

            PositionPanel();
            FadeTo(1, Easing.Linear, Config.AnimationDurationMilliseconds);
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
            if (!Anchor.HasValue || Panel == null)
                return;

            var anchor = Anchor.Value;
            var gap = Config.AnchorGap;
            var availableBelow = WindowManager.Height - anchor.Bottom - gap;
            var availableAbove = anchor.Top - gap;
            var opensBelow = availableBelow >= Panel.Height || availableBelow >= availableAbove;
            var x = MathHelper.Clamp(anchor.Left, 0, Math.Max(0, WindowManager.Width - Panel.Width));
            var y = opensBelow ? anchor.Bottom + gap : anchor.Top - gap - Panel.Height;

            Panel.Position = new ScalableVector2(
                x - Container.ScreenRectangle.Left,
                MathHelper.Clamp(y, 0, Math.Max(0, WindowManager.Height - Panel.Height)) - Container.ScreenRectangle.Top);
        }
    }
}
