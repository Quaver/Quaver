using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        private SkinV2ProfileViewerConfig Config { get; }

        private ProfileViewerPanel Panel { get; set; }

        private bool IsClosing { get; set; }

        public ProfileViewerDialog(User user) : base(0)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            if (User.OnlineUser == null)
                throw new ArgumentException("The profile viewer requires user information.", nameof(user));

            Skin = SkinManager.AcquireV2();
            Config = Skin.Config.Shared.ProfileViewer;
            CreateContent();

            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
            OnlineManager.Status.ValueChanged += OnConnectionStatusChanged;
            FadeTo(Config.DimOpacity, Easing.Linear, Config.AnimationDurationMilliseconds);
        }

        public static void Show(User user)
        {
            if (user?.OnlineUser == null)
                return;

            foreach (var existing in DialogManager.Dialogs.OfType<ProfileViewerDialog>().ToArray())
                DialogManager.Dismiss(existing);

            DialogManager.Show(new ProfileViewerDialog(user));
        }

        public override void CreateContent()
        {
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            Container.Size = Size;

            Panel = new ProfileViewerPanel(User, Skin, Close, Container)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter
            };
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
            });
    }
}
