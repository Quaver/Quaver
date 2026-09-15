using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Input.Global;
using Quaver.Shared.Screens.V2.Options.UI;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     The V2 options menu. A header with the title, search bar and preset picker sits on top.
    ///     Below it are the category rail, the subcategory list and the option rows.
    /// </summary>
    internal sealed partial class OptionsDialogV2 : DialogScreen, ISkinV2EditorHost
    {
        private SkinStoreV2Lease Skin { get; }

        private SkinV2Config RootConfig { get; set; }

        private SkinV2OptionsConfig Config => RootConfig.Screens.Options;

        private GlobalInputScopeToken GlobalInputToken { get; }

        /// <summary>
        ///     The rounded surface that holds the whole menu.
        /// </summary>
        private RoundedPanel RootSurface { get; set; }

        /// <summary>
        ///     Work queued by a click and run at the start of the next update. A click can rebuild the
        ///     list that holds the clicked button, which is not safe while that button is still updating.
        /// </summary>
        private Action PendingNavigationAction { get; set; }

        private bool Closing { get; set; }

        private bool Destroyed { get; set; }

        internal OptionsDialogV2() : base(0)
        {
            Skin = SkinManager.AcquireV2();
            RootConfig = Skin.Config;
            GlobalInputToken = new DialogInputToken(this);
            Tint = SkinV2Color.Parse(Config.Backdrop.Color);
            Alpha = Config.Backdrop.Opacity;

            PreviewRoot = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Pivot = Vector2.Zero
            };
            CreateContent();
            EditorRoot = new Container
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Visible = false
            };

            Clicked += OnBackdropClicked;
            WindowManager.VirtualScreenSizeChanged += OnVirtualScreenSizeChanged;
        }

        public override void CreateContent()
        {
            LoadIcons();

            RootSurface = new RoundedPanel(Config.Backdrop.CornerRadius)
            {
                Parent = PreviewRoot,
                Tint = SkinV2Color.Parse(Config.Backdrop.GapColor)
            };

            CreateBody();
            CreateRailOverlay();
            CreateCategoryNavigation();
            CreateSubcategoryNavigation();
            CreateHeader();
            CreateContentArea();

            EditorTargets = CreateEditorTargets();
            UpdateResponsiveLayout(true);
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsCtrlDown() && KeyboardManager.IsShiftDown() &&
                KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.E))
                ToggleSkinEditor();
        }

        public override void Update(GameTime gameTime)
        {
            if (PendingNavigationAction != null)
            {
                var action = PendingNavigationAction;
                PendingNavigationAction = null;
                action();
            }

            UpdateResponsiveLayout();
            UpdateHeader();
            UpdateRail();
            UpdateNavigationLayout();
            ContentList.UpdateLayout();

            CategoryNavigationScroll.InputEnabled = RailOverlay.Visible && RailOverlay.IsHovered();
            SubcategoryNavigationScroll.InputEnabled = CategoryPanel.Visible && CategoryPanel.IsHovered();

            V2Slider.SuspendKeyboardAdjustment = SearchBar.IsFocused;

            ContentList.UpdateCulling();

            base.Update(gameTime);
            UpdateCategoryLabelProgress();
        }

        public override void Destroy()
        {
            if (Destroyed)
                return;

            Destroyed = true;
            PendingNavigationAction = null;
            V2Slider.SuspendKeyboardAdjustment = false;
            WindowManager.VirtualScreenSizeChanged -= OnVirtualScreenSizeChanged;
            SkinEditor?.Destroy();
            SkinEditor = null;
            GlobalInputToken.Dispose();
            OptionsDrawableCleanup.DestroyTree(Container);
            Skin.Dispose();
            base.Destroy();
        }

        /// <summary>
        ///     Back closes the innermost open thing first: the skin editor, the preset menu, a slider value
        ///     being typed, the search box, the search, the open rail, and last the menu itself.
        /// </summary>
        private GlobalInputHandleResult HandleGlobalInputAction(GlobalKeybindActions action, bool isKeyPress,
            bool isRelease)
        {
            if (!IsOnTop || !isKeyPress || isRelease || action.BaseWithLayer() != GlobalKeybindActions.Back)
                return GlobalInputHandleResult.Pass;

            if (SkinEditor?.IsOpen == true)
                SkinEditor.RequestClose();
            else if (PresetDropdown.IsOpen)
                PresetDropdown.CloseMenu();
            else if (V2Slider.IsEditingValue)
                V2Slider.CancelValueEdit();
            else if (SearchBar.IsFocused)
                SearchBar.IsFocused = false;
            else if (SearchActive)
                PendingNavigationAction = () => ClearSearch();
            else if (RailExpanded)
                SetRailExpanded(false, true);
            else
                Close();

            return GlobalInputHandleResult.Consumed;
        }

        /// <summary>
        ///     Clicking outside the menu closes it.
        /// </summary>
        private void OnBackdropClicked(object? sender, EventArgs args)
        {
            if (SkinEditor?.IsOpen == true)
                return;

            if (!RootSurface.IsHovered())
                Close();
        }

        private void OnVirtualScreenSizeChanged(object? sender, WindowVirtualScreenSizeChangedEventArgs args) =>
            UpdateResponsiveLayout(true);

        private void Close()
        {
            if (Closing)
                return;

            Closing = true;
            DialogManager.Dismiss(this);
        }

        /// <summary>
        ///     Sends global keybinds to this menu while it is open.
        /// </summary>
        private sealed class DialogInputToken : GlobalInputScopeToken
        {
            private OptionsDialogV2 Dialog { get; }

            internal DialogInputToken(OptionsDialogV2 dialog) => Dialog = dialog;

            public override GlobalInputScope Scope => GlobalInputScope.Options;

            public override GlobalInputHandleResult Handle(GlobalKeybindActions action, bool isKeyPress = true,
                bool isRelease = false) => Dialog.HandleGlobalInputAction(action, isKeyPress, isRelease);
        }
    }
}
