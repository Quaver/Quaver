using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.UI
{
    public enum DropdownSelectionMode
    {
        Single,
        Multiple
    }

    public abstract class DropdownEntry<T>
    {
    }

    public sealed class DropdownOption<T> : DropdownEntry<T>
    {
        public T Value { get; }

        public string Label { get; }

        public string ShortLabel { get; }

        public TextureRegion? Icon { get; }

        public DropdownOption(T value, string label, string shortLabel = null,
            TextureRegion? icon = null)
        {
            Value = value;
            Label = label ?? string.Empty;
            ShortLabel = shortLabel;
            Icon = icon;
        }

        public DropdownOption(T value, string label, Texture2D icon, string shortLabel = null)
            : this(value, label, shortLabel,
                icon == null ? (TextureRegion?) null : new TextureRegion(icon, icon.Bounds))
        {
        }
    }

    public sealed class DropdownDivider<T> : DropdownEntry<T>
    {
    }

    public sealed class DropdownOptionEventArgs<T> : EventArgs
    {
        public DropdownOption<T> Option { get; }

        public bool Selected { get; }

        public DropdownOptionEventArgs(DropdownOption<T> option, bool selected = true)
        {
            Option = option;
            Selected = selected;
        }
    }

    public abstract class V2DropdownBase : Container
    {
        protected V2DropdownBase()
        {
        }

        internal abstract void CloseImmediatelyFromRegistry();

        internal abstract void SetExternalAlphaInternal(float alpha);
    }

    internal static class V2DropdownRegistry
    {
        private static V2DropdownBase ActiveDropdown { get; set; }

        public static void Activate(V2DropdownBase dropdown)
        {
            if (ActiveDropdown != null && ActiveDropdown != dropdown)
                ActiveDropdown.CloseImmediatelyFromRegistry();

            ActiveDropdown = dropdown;
        }

        public static void Deactivate(V2DropdownBase dropdown)
        {
            if (ActiveDropdown == dropdown)
                ActiveDropdown = null;
        }
    }

    /// <summary>
    ///     Shared V2 dropdown selector. The trigger remains in its owner's layout while the opened
    ///     menu is created as a transient child of the caller-provided full-screen overlay host.
    /// </summary>
    public sealed class V2Dropdown<T> : V2DropdownBase
    {
        private const int MinimumMenuHeight = 1;
        // ButtonManager resolves overlapping buttons from the lowest depth first. Keep transient
        // menu rows ahead of controls underneath the menu, even when their rectangles overlap.
        private const int OpenMenuButtonDepth = int.MinValue;

        private IReadOnlyList<DropdownEntry<T>> Entries { get; }

        private Bindable<T> Value { get; }

        private HashSet<T> SelectedValues { get; }

        private WobbleFontStore Font { get; }

        private SkinV2DropdownConfig Config { get; }

        private Container OverlayHost { get; }

        private DropdownTrigger Trigger { get; }

        private Container Menu { get; set; }

        private Sprite MenuDivider { get; set; }

        private HorizontalClippingContainer MenuClip { get; set; }

        private ScrollContainer MenuScroll { get; set; }

        private List<V2DropdownOptionButton> OptionButtons { get; } =
            new List<V2DropdownOptionButton>();

        private List<Sprite> EntryDividers { get; } = new List<Sprite>();

        private bool OpensDown { get; set; }

        private bool IsClosing { get; set; }

        private bool IsOpening { get; set; }

        private bool CloseImmediatelyRequested { get; set; }

        private float RequestedMenuHeight { get; set; }

        private float TargetMenuHeight { get; set; }

        private int OptionCount { get; set; }


        private float ExternalAlpha { get; set; } = 1;

        private TextureRegion? SummaryIcon { get; set; }

        public DropdownSelectionMode SelectionMode { get; }

        public bool Opened { get; private set; }

        public int MaxVisibleItems { get; set; }

        public string EmptySelectionText { get; set; } = string.Empty;

        public Func<IReadOnlyList<DropdownOption<T>>, string> SummaryFormatter { get; set; }

        public event EventHandler<DropdownOptionEventArgs<T>> OptionSelected;

        public event EventHandler SelectionChanged;

        public IReadOnlyCollection<T> SelectedItems => SelectedValues;

        public V2Dropdown(float width, Bindable<T> value, IReadOnlyList<DropdownEntry<T>> entries,
            WobbleFontStore font, SkinV2DropdownConfig config, Container overlayHost,
            TextureRegion? selectedIcon = null)
            : this(width, entries, font, config, overlayHost, DropdownSelectionMode.Single,
                selectedIcon, null)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Value.ValueChanged += OnValueChanged;
            RefreshTrigger();
        }

        public V2Dropdown(float width, IReadOnlyList<DropdownEntry<T>> entries,
            IEnumerable<T> selectedValues, WobbleFontStore font, SkinV2DropdownConfig config,
            Container overlayHost, TextureRegion? selectedIcon = null)
            : this(width, entries, font, config, overlayHost, DropdownSelectionMode.Multiple,
                selectedIcon, selectedValues)
        {
            RefreshTrigger();
        }

        private V2Dropdown(float width, IReadOnlyList<DropdownEntry<T>> entries,
            WobbleFontStore font, SkinV2DropdownConfig config, Container overlayHost,
            DropdownSelectionMode selectionMode, TextureRegion? selectedIcon,
            IEnumerable<T> selectedValues)
        {
            if (entries == null)
                throw new ArgumentNullException(nameof(entries));
            if (entries.Count == 0)
                throw new ArgumentException("A dropdown must contain at least one entry.", nameof(entries));
            if (font == null)
                throw new ArgumentNullException(nameof(font));
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (overlayHost == null)
                throw new ArgumentNullException(nameof(overlayHost));

            Entries = entries;
            Font = font;
            Config = config;
            OverlayHost = overlayHost;
            SelectionMode = selectionMode;
            SummaryIcon = selectedIcon;
            SelectedValues = new HashSet<T>(selectedValues ?? Array.Empty<T>());
            MaxVisibleItems = config.DefaultMaxVisibleItems;

            Size = new ScalableVector2(width, config.Height);
            Trigger = new DropdownTrigger(null)
            {
                Parent = this,
                Size = Size,
                Depth = 20,
                PerformHoverFade = true
            };
            Trigger.Configure(Font, config);
            Trigger.Clicked += OnTriggerClicked;
        }

        public override void Update(GameTime gameTime)
        {
            if (Opened && DialogManager.Dialogs.Count != 0)
                CloseImmediatelyInternal();

            base.Update(gameTime);

            if (IsOpening && Menu != null && Menu.Height >= TargetMenuHeight - 0.5f)
                IsOpening = false;

            Trigger.ExternalAlpha = ExternalAlpha;
            Trigger.IsInteractionEnabled = ExternalAlpha > 0.001f && !IsClosing;
            Trigger.IsClickable = ExternalAlpha > 0.001f && !IsClosing;

            if (Menu == null)
                return;

            if (CloseImmediatelyRequested || IsClosing && Menu.Height <= MinimumMenuHeight)
            {
                DestroyMenu();
                return;
            }

            RefreshMenuLayout();
            MenuScroll.InputEnabled = IsMenuScrollInputAllowed();
            RefreshMenuVisibility();

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Contains(Trigger.ScreenRectangle,
                    MouseManager.CurrentState.Position) && !Contains(Menu.ScreenRectangle,
                    MouseManager.CurrentState.Position))
                Close();
        }

        public override void Destroy()
        {
            if (Value != null)
                Value.ValueChanged -= OnValueChanged;

            V2DropdownRegistry.Deactivate(this);
            DestroyMenu();
            base.Destroy();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Trigger == null)
                return;

            Trigger.Size = Size;
            RefreshTrigger();
            if (Menu != null)
                RefreshMenuLayout();
        }

        public void Open()
        {
            if (Opened || DialogManager.Dialogs.Count != 0 || ExternalAlpha <= 0.001f)
                return;

            V2DropdownRegistry.Activate(this);
            BringOverlayHostToFront();
            DestroyMenu();
            Opened = true;
            IsClosing = false;
            CloseImmediatelyRequested = false;
            CreateMenu();
            Trigger.IsClickable = true;
            Trigger.IsInteractionEnabled = true;
        }

        private void BringOverlayHostToFront()
        {
            // Drawable.Draw() follows the parent's child insertion order and updates DrawOrder
            // after the draw has already happened. Re-assigning the same parent moves the
            // explicitly supplied overlay host to the end of its sibling list, which makes the
            // transient menu render above the screen content regardless of when the host was
            // created.
            var parent = OverlayHost.Parent;
            if (parent != null)
                OverlayHost.Parent = parent;
        }

        public void Close(int time = -1)
        {
            if (Menu == null)
            {
                Opened = false;
                V2DropdownRegistry.Deactivate(this);
                RefreshTrigger();
                return;
            }

            if (time < 0)
                time = Config.AnimationDurationMilliseconds;

            Opened = false;
            V2DropdownRegistry.Deactivate(this);
            Trigger.IsClickable = false;
            Trigger.IsInteractionEnabled = false;
            IsOpening = false;

            if (time <= 0)
            {
                CloseImmediatelyInternal();
                return;
            }

            IsClosing = true;
            CloseImmediatelyRequested = false;
            Menu.ClearAnimations();
            Menu.ChangeHeightTo(MinimumMenuHeight, Easing.InCubic, time);
            FadeMenuTo(0, time);
            RefreshTrigger();
        }

        public void CloseImmediately()
        {
            CloseImmediatelyInternal();
        }

        internal override void CloseImmediatelyFromRegistry()
        {
            CloseImmediatelyInternal();
        }

        public void SetSummaryIcon(TextureRegion? icon)
        {
            SummaryIcon = icon;
            RefreshTrigger();
        }

        public void SetSelected(T value, bool selected, bool invokeEvent = true)
        {
            if (SelectionMode != DropdownSelectionMode.Multiple)
                throw new InvalidOperationException("SetSelected is only valid for multi-select dropdowns.");

            var changed = selected ? SelectedValues.Add(value) : SelectedValues.Remove(value);
            if (!changed)
                return;

            RefreshTrigger();
            RefreshOptionButtons();
            if (invokeEvent)
                SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetExternalAlpha(float alpha)
        {
            ExternalAlpha = MathHelper.Clamp(alpha, 0, 1);
            if (ExternalAlpha <= 0.001f && Menu != null)
                CloseImmediatelyInternal();

            Trigger.ExternalAlpha = ExternalAlpha;
        }

        internal override void SetExternalAlphaInternal(float alpha) => SetExternalAlpha(alpha);

        private void ToggleMenu()
        {
            if (Opened)
                Close();
            else
                Open();
        }

        private void OnTriggerClicked(object sender, EventArgs args)
        {
            if (ExternalAlpha <= 0.001f)
                return;

            ToggleMenu();
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<T> args)
        {
            RefreshTrigger();
            RefreshOptionButtons();
        }

        private void CreateMenu()
        {
            Menu = new Container
            {
                Parent = OverlayHost,
                Size = new ScalableVector2(Width, MinimumMenuHeight),
                DrawOrder = 1000
            };

            MenuDivider = new Sprite
            {
                Parent = Menu,
                Alignment = Alignment.TopLeft,
                Image = WobbleAssets.WhiteBox,
                Tint = SkinV2Color.Parse(Config.DividerColor),
                Alpha = 0
            };

            var contentWidth = Math.Max(1, Width);
            MenuClip = new HorizontalClippingContainer
            {
                Parent = Menu,
                Size = new ScalableVector2(contentWidth, 1),
                UsePreviousSpriteBatchOptions = true
            };

            MenuScroll = new DropdownScrollContainer(new ScalableVector2(contentWidth, 1),
                new ScalableVector2(contentWidth, 1),
                IsMenuScrollInputAllowed, IsMenuMouseWheelCaptureActive)
            {
                Parent = MenuClip,
                Tint = Color.Transparent,
                InputEnabled = false,
                AllowScrollbarDragging = true,
                AllowMiddleMouseDragging = false,
                Alpha = 0
            };
            MenuScroll.Scrollbar.Width = Config.ScrollbarWidth;
            MenuScroll.Scrollbar.Tint = SkinV2Color.Parse(Config.ScrollbarColor);
            MenuScroll.Scrollbar.Visible = false;

            var y = 0f;
            var optionCount = 0;
            for (var i = 0; i < Entries.Count; i++)
            {
                var option = Entries[i] as DropdownOption<T>;
                if (option != null)
                {
                    var button = CreateOptionButton(option);
                    button.Position = new ScalableVector2(0, y);
                    button.Size = new ScalableVector2(contentWidth, Config.ItemHeight);
                    MenuScroll.AddContainedDrawable(button);
                    OptionButtons.Add(button);
                    y += Config.ItemHeight;
                    optionCount++;
                }
                else
                {
                    var divider = new Sprite
                    {
                        Parent = MenuScroll.ContentContainer,
                        Position = new ScalableVector2(Config.DividerInset, y),
                        Size = new ScalableVector2(Math.Max(1, contentWidth - Config.DividerInset * 2),
                            Config.DividerThickness),
                        Image = WobbleAssets.WhiteBox,
                        Tint = SkinV2Color.Parse(Config.DividerColor),
                        Alpha = 0
                    };
                    EntryDividers.Add(divider);
                    y += Config.DividerThickness;
                }

                if (i < Entries.Count - 1)
                    y += Config.ItemSpacing;
            }

            MenuScroll.ContentContainer.Height = Math.Max(1, y);
            OptionCount = optionCount;
            RequestedMenuHeight = CalculateTargetMenuHeight(y, optionCount);
            TargetMenuHeight = RequestedMenuHeight;
            Menu.Height = MinimumMenuHeight;
            IsOpening = true;
            RefreshMenuLayout(false);

            var duration = Math.Max(1, Config.AnimationDurationMilliseconds);
            Menu.ChangeHeightTo((int)TargetMenuHeight, Easing.OutQuint, duration);
            FadeMenuTo(1, duration);

            for (var i = 0; i < OptionButtons.Count; i++)
            {
                var button = OptionButtons[i];
                button.Alpha = 0;
                button.FadeTo(1, Easing.OutQuint, duration + i * 12);
            }

            foreach (var divider in EntryDividers)
                divider.FadeTo(1, Easing.OutQuint, duration);
        }

        private V2DropdownOptionButton CreateOptionButton(DropdownOption<T> option)
        {
            var selected = IsSelected(option);
            var button = new V2DropdownOptionButton(option, Config)
            {
                Parent = MenuScroll.ContentContainer,
                Tint = SkinV2Color.Parse(selected ? Config.SelectedItemColor : Config.ItemColor),
                Depth = OpenMenuButtonDepth,
                IsClickable = true,
                IsInteractionEnabled = true,
                CornerRadius = 0,
                PerformHoverFade = false
            };
            button.SetViewportHitTest(() => MenuScroll != null &&
                Contains(MenuScroll.ScreenRectangle, MouseManager.CurrentState.Position));
            button.Clicked += (sender, args) => OnOptionClicked(option);
            button.Configure(Font, Config, selected);
            return button;
        }

        private void OnOptionClicked(DropdownOption<T> option)
        {
            if (!Opened || IsClosing)
                return;

            if (SelectionMode == DropdownSelectionMode.Multiple)
            {
                var selected = !SelectedValues.Contains(option.Value);
                if (selected)
                    SelectedValues.Add(option.Value);
                else
                    SelectedValues.Remove(option.Value);

                RefreshTrigger();
                RefreshOptionButtons();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                OptionSelected?.Invoke(this, new DropdownOptionEventArgs<T>(option, selected));
                return;
            }

            Close(0);
            Value.Value = option.Value;
            OptionSelected?.Invoke(this, new DropdownOptionEventArgs<T>(option));
        }

        private void RefreshTrigger()
        {
            if (Trigger == null)
                return;

            var selected = GetSelectedOptions();
            var label = GetSummaryLabel(selected);
            var icon = SummaryIcon;
            if (icon == null && selected.Count == 1)
                icon = selected[0].Icon;

            Trigger.SetContent(label, icon, Config);
            Trigger.CornerRadii = GetTriggerRadii();
        }

        private void RefreshOptionButtons()
        {
            foreach (var button in OptionButtons)
                button.SetSelected(IsSelected(button.Option));
        }

        private void RefreshMenuVisibility()
        {
            if (MenuScroll == null)
                return;

            // ScrollContainer owns clipping. Do not fade rows based on their current screen
            // rectangle: that makes rows pop in and out while the viewport is animating or
            // while the content is being scrolled. Rows should keep their menu animation alpha
            // and simply be clipped by the scroll container at its edges.
            foreach (var button in OptionButtons)
                button.Visible = true;

            foreach (var divider in EntryDividers)
                divider.Visible = true;
        }

        private bool IsMenuScrollable()
        {
            if (MenuScroll == null)
                return false;

            var targetViewportHeight = Math.Max(MinimumMenuHeight,
                TargetMenuHeight - Config.DividerThickness - Config.MenuPadding * 2);
            return MenuScroll.ContentContainer.Height > targetViewportHeight + 0.5f;
        }

        private bool IsMenuScrollInputAllowed() =>
            Opened && ExternalAlpha > 0.001f && MenuScroll != null && MenuScroll.IsHovered() &&
            IsMenuScrollable();

        private bool IsMenuMouseWheelCaptureActive() =>
            ExternalAlpha > 0.001f && Menu != null &&
            (Contains(Menu.ScreenRectangle, MouseManager.CurrentState.Position) ||
             Contains(Trigger.ScreenRectangle, MouseManager.CurrentState.Position));

        private void RefreshMenuLayout(bool applyMenuHeight = true)
        {
            if (Menu == null || MenuClip == null || MenuScroll == null)
                return;

            var availableBelow = WindowManager.Height - Trigger.ScreenRectangle.Bottom - Config.MenuGap;
            var availableAbove = Trigger.ScreenRectangle.Top - Config.MenuGap;
            var desiredHeight = Math.Max(MinimumMenuHeight, RequestedMenuHeight);
            var enoughBelow = availableBelow >= desiredHeight;
            OpensDown = enoughBelow || availableBelow >= availableAbove;

            var available = Math.Max(MinimumMenuHeight, OpensDown ? availableBelow : availableAbove);
            var currentHeight = Math.Max(MinimumMenuHeight, Menu.Height);
            var menuHeight = Math.Min(desiredHeight, available);
            TargetMenuHeight = menuHeight;
            if (IsClosing)
                menuHeight = currentHeight;

            var x = MathHelper.Clamp(Trigger.ScreenRectangle.Left, 0,
                Math.Max(0, WindowManager.Width - Width));
            var y = OpensDown
                ? Trigger.ScreenRectangle.Bottom + Config.MenuGap
                : Trigger.ScreenRectangle.Top - Config.MenuGap - currentHeight;
            var hostX = x - OverlayHost.ScreenRectangle.Left;
            var hostY = y - OverlayHost.ScreenRectangle.Top;
            Menu.Position = new ScalableVector2(hostX, hostY);
            Menu.Width = Width;

            MenuDivider.Position = new ScalableVector2(Config.DividerInset,
                OpensDown ? 0 : Math.Max(0, currentHeight - Config.DividerThickness));
            MenuDivider.Size = new ScalableVector2(Math.Max(1, Width - Config.DividerInset * 2),
                Config.DividerThickness);

            var dividerSpace = Config.DividerThickness;
            var viewportHeight = Math.Max(MinimumMenuHeight,
                currentHeight - dividerSpace - Config.MenuPadding * 2);
            var viewportPosition = new ScalableVector2(0,
                OpensDown ? dividerSpace + Config.MenuPadding : Config.MenuPadding);
            var viewportSize = new ScalableVector2(Math.Max(1, Width),
                viewportHeight);
            MenuClip.Position = viewportPosition;
            MenuClip.Size = viewportSize;
            MenuScroll.Position = new ScalableVector2(0, 0);
            MenuScroll.Size = viewportSize;
            // Use the requested item limit and the available viewport as the source of truth.
            // Comparing the animated viewport to content height directly can expose a scrollbar
            // for a rounding remainder even when every option is already visible.
            var itemLimitRequiresScroll = MaxVisibleItems > 0 && MaxVisibleItems < OptionCount;
            var viewportRequiresScroll = available < desiredHeight - 0.5f;
            MenuScroll.Scrollbar.Visible = itemLimitRequiresScroll || viewportRequiresScroll;
            MenuScroll.InputEnabled = IsMenuScrollInputAllowed();

            if (applyMenuHeight && Math.Abs(Menu.Height - menuHeight) > 0.5f &&
                !IsClosing && !IsOpening)
                Menu.Height = menuHeight;

            Trigger.CornerRadii = GetTriggerRadii();
        }

        private float CalculateTargetMenuHeight(float contentHeight, int optionCount)
        {
            var visibleItems = MaxVisibleItems <= 0 ? optionCount : MaxVisibleItems;
            if (visibleItems >= optionCount)
                return (float) Math.Ceiling(contentHeight + Config.MenuPadding * 2 +
                                            Config.DividerThickness);

            var visibleHeight = 0f;
            var shown = 0;
            for (var i = 0; i < Entries.Count && shown < visibleItems; i++)
            {
                var option = Entries[i] as DropdownOption<T>;
                visibleHeight += option == null ? Config.DividerThickness : Config.ItemHeight;
                if (option != null)
                    shown++;

                if (i < Entries.Count - 1 && shown < visibleItems)
                    visibleHeight += Config.ItemSpacing;
            }

            return (float) Math.Ceiling(visibleHeight + Config.MenuPadding * 2 +
                                        Config.DividerThickness);
        }

        private RoundedRectCornerRadii GetTriggerRadii()
        {
            if (!Opened && !IsClosing)
                return RoundedRectCornerRadii.All(Config.CornerRadius);

            return OpensDown
                ? new RoundedRectCornerRadii(Config.CornerRadius, Config.CornerRadius, 0, 0)
                : new RoundedRectCornerRadii(0, 0, Config.CornerRadius, Config.CornerRadius);
        }

        private void FadeMenuTo(float alpha, int time)
        {
            MenuDivider?.FadeTo(alpha, Easing.OutQuint, time);
            MenuScroll?.FadeTo(alpha, Easing.OutQuint, time);
        }

        private IReadOnlyList<DropdownOption<T>> GetSelectedOptions()
        {
            var selected = new List<DropdownOption<T>>();
            foreach (var entry in Entries)
            {
                var option = entry as DropdownOption<T>;
                if (option == null)
                    continue;

                if (SelectionMode == DropdownSelectionMode.Single)
                {
                    if (Value != null && EqualityComparer<T>.Default.Equals(Value.Value, option.Value))
                    {
                        selected.Add(option);
                        break;
                    }
                }
                else if (SelectedValues.Contains(option.Value))
                    selected.Add(option);
            }

            if (SelectionMode == DropdownSelectionMode.Single && selected.Count == 0)
            {
                foreach (var entry in Entries)
                {
                    var option = entry as DropdownOption<T>;
                    if (option != null)
                    {
                        selected.Add(option);
                        break;
                    }
                }
            }

            return selected;
        }

        private string GetSummaryLabel(IReadOnlyList<DropdownOption<T>> selected)
        {
            if (SelectionMode == DropdownSelectionMode.Multiple)
            {
                if (SummaryFormatter != null)
                    return SummaryFormatter(selected);

                if (selected.Count == 0)
                    return EmptySelectionText;

                var labels = new List<string>();
                foreach (var option in selected)
                    labels.Add(string.IsNullOrEmpty(option.ShortLabel) ? option.Label : option.ShortLabel);
                return string.Join(", ", labels);
            }

            return selected.Count == 0 ? EmptySelectionText : selected[0].Label;
        }

        private bool IsSelected(DropdownOption<T> option)
        {
            if (SelectionMode == DropdownSelectionMode.Multiple)
                return SelectedValues.Contains(option.Value);

            return Value != null && EqualityComparer<T>.Default.Equals(Value.Value, option.Value);
        }

        private void CloseImmediatelyInternal()
        {
            Opened = false;
            IsClosing = false;
            CloseImmediatelyRequested = true;
            V2DropdownRegistry.Deactivate(this);
            DestroyMenu();
            RefreshTrigger();
        }

        private void DestroyMenu()
        {
            if (Menu != null)
                Menu.Destroy();

            Menu = null;
            MenuDivider = null;
            MenuClip = null;
            MenuScroll = null;
            OptionButtons.Clear();
            EntryDividers.Clear();
            IsOpening = false;
            RequestedMenuHeight = 0;
            TargetMenuHeight = 0;
            OptionCount = 0;
            IsClosing = false;
            CloseImmediatelyRequested = false;
            RefreshTrigger();
        }

        private static bool Contains(RectangleF rectangle, Vector2 point) =>
            point.X >= rectangle.Left && point.X <= rectangle.Right &&
            point.Y >= rectangle.Top && point.Y <= rectangle.Bottom;

        private sealed class DropdownScrollContainer : ScrollContainer
        {
            private Func<bool> InputGate { get; }

            private Func<bool> MouseWheelCaptureGate { get; }

            public DropdownScrollContainer(ScalableVector2 size, ScalableVector2 contentSize,
                Func<bool> inputGate, Func<bool> mouseWheelCaptureGate) : base(size, contentSize)
            {
                InputGate = inputGate;
                MouseWheelCaptureGate = mouseWheelCaptureGate;
                CapturesMouseWheelInput = true;
                Scrollbar.UsePreviousSpriteBatchOptions = true;
            }

            public override void Update(GameTime gameTime)
            {
                InputEnabled = InputGate?.Invoke() == true;
                base.Update(gameTime);
            }

            protected override bool IsMouseWheelInputCaptureActive() =>
                CapturesMouseWheelInput && Visible && !IsDisposed &&
                MouseWheelCaptureGate?.Invoke() == true;
        }

        private sealed class DropdownTrigger : RoundedButton
        {
            private const int IconSpacing = 8;

            private WobbleFontStore Font { get; set; }

            private SkinV2DropdownConfig Config { get; set; }

            private Sprite SelectedIcon { get; }

            private Sprite Chevron { get; }

            private MarqueeSpriteText MarqueeLabel { get; set; }

            private float LastMarqueeWidth { get; set; } = -1;

            private float HoverAlpha { get; set; } = 1;

            public float ExternalAlpha { get; set; } = 1;

            public DropdownTrigger(EventHandler clickAction) : base(clickAction)
            {
                SelectedIcon = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    UsePreviousSpriteBatchOptions = true
                };
                Chevron = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidRight,
                    Region = GlobalIcons.Get(GlobalIcon.MoreOptions),
                    UsePreviousSpriteBatchOptions = true
                };
            }

            public void Configure(WobbleFontStore font, SkinV2DropdownConfig config)
            {
                Font = font;
                Config = config;
                Tint = SkinV2Color.Parse(config.TriggerColor);
                CornerRadius = config.CornerRadius;
                MarqueeLabel = new MarqueeSpriteText(font, string.Empty, config.FontSize, 1)
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    UsePreviousSpriteBatchOptions = true
                };
                MarqueeLabel.StartDelayMilliseconds = 450;
                MarqueeLabel.TextSprite.Tint = SkinV2Color.Parse(config.TextColor);
                Chevron.Size = new ScalableVector2(config.ChevronSize, config.ChevronSize);
                Chevron.X = -config.HorizontalPadding;
                Chevron.Tint = SkinV2Color.Parse(config.IconColor);
                SelectedIcon.Tint = SkinV2Color.Parse(config.IconColor);
            }

            public void SetContent(string label, TextureRegion? icon,
                SkinV2DropdownConfig config)
            {
                MarqueeLabel.TextSprite.Text = label ?? string.Empty;
                MarqueeLabel.TextSprite.Tint = SkinV2Color.Parse(Config.TextColor);
                MarqueeLabel.ResetPosition();
                MarqueeLabel.IsActive = false;
                Chevron.X = -config.HorizontalPadding;
                SelectedIcon.Visible = icon.HasValue;
                if (icon.HasValue)
                {
                    SelectedIcon.Region = icon;
                    SelectedIcon.Size = new ScalableVector2(config.IconSize, config.IconSize);
                }

                LayoutContent(config);
            }

            public override void Update(GameTime gameTime)
            {
                MarqueeLabel.IsActive = IsHovered && ExternalAlpha > 0.001f;
                Alpha = HoverAlpha;
                base.Update(gameTime);
                HoverAlpha = Alpha;
                Alpha = HoverAlpha * ExternalAlpha;
                MarqueeLabel.IsActive = IsHovered && ExternalAlpha > 0.001f;
                if (Config != null)
                    LayoutContent(Config);
            }

            private void LayoutContent(SkinV2DropdownConfig config)
            {
                if (MarqueeLabel == null || Chevron == null)
                    return;

                var left = config.HorizontalPadding +
                           (SelectedIcon.Visible ? config.IconSize + IconSpacing : 0);
                var right = config.HorizontalPadding + config.ChevronSize + IconSpacing;
                MarqueeLabel.Alignment = Alignment.MidLeft;
                MarqueeLabel.X = left;
                var marqueeWidth = Math.Max(1, Width - left - right);
                if (Math.Abs(LastMarqueeWidth - marqueeWidth) > 0.5f)
                {
                    LastMarqueeWidth = marqueeWidth;
                    MarqueeLabel.ResetPosition();
                }

                MarqueeLabel.Width = marqueeWidth;
                Chevron.Alignment = Alignment.MidRight;
                Chevron.X = -config.HorizontalPadding;
            }
        }

        private sealed class V2DropdownOptionButton : RoundedButton
        {
            private SkinV2DropdownConfig Config { get; set; }

            private Func<bool> ViewportHitTest { get; set; }

            private Sprite OptionIcon { get; }

            private MarqueeSpriteText MarqueeLabel { get; set; }

            private float LastMarqueeWidth { get; set; } = -1;

            public DropdownOption<T> Option { get; }

            private bool Selected { get; set; }

            public V2DropdownOptionButton(DropdownOption<T> option, SkinV2DropdownConfig config)
            {
                Option = option;
                Config = config;
                OptionIcon = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    Size = new ScalableVector2(config.IconSize, config.IconSize),
                    Tint = SkinV2Color.Parse(config.IconColor),
                    UsePreviousSpriteBatchOptions = true
                };
                Hovered += OnHovered;
                LeftHover += OnLeftHover;
            }

            public void Configure(WobbleFontStore font, SkinV2DropdownConfig config, bool selected)
            {
                Config = config;
                MarqueeLabel = new MarqueeSpriteText(font, Option.Label, config.FontSize, 1)
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    UsePreviousSpriteBatchOptions = true
                };
                MarqueeLabel.StartDelayMilliseconds = 450;
                MarqueeLabel.ResetPosition();
                MarqueeLabel.TextSprite.Tint = SkinV2Color.Parse(
                    selected ? config.SelectedTextColor : config.TextColor);
                if (Option.Icon.HasValue)
                {
                    OptionIcon.Region = Option.Icon;
                    OptionIcon.Visible = true;
                }
                else
                    OptionIcon.Visible = false;

                SetSelected(selected);
            }

            public void SetSelected(bool selected)
            {
                Selected = selected;
                Tint = SkinV2Color.Parse(selected ? Config.SelectedItemColor : Config.ItemColor);
                if (MarqueeLabel != null)
                    MarqueeLabel.TextSprite.Tint = SkinV2Color.Parse(
                        selected ? Config.SelectedTextColor : Config.TextColor);
                LayoutContent();
            }

            public void SetViewportHitTest(Func<bool> viewportHitTest)
            {
                ViewportHitTest = viewportHitTest;
            }

            protected override bool IsMouseInClickArea()
            {
                var inButton = base.IsMouseInClickArea();
                var inViewport = ViewportHitTest == null || ViewportHitTest();
                return inButton && inViewport;
            }

            protected override void OnRectangleRecalculated()
            {
                base.OnRectangleRecalculated();
                LayoutContent();
            }

            public override void Update(GameTime gameTime)
            {
                if (MarqueeLabel != null)
                    MarqueeLabel.IsActive = IsHovered;
                base.Update(gameTime);
                if (MarqueeLabel != null)
                    MarqueeLabel.IsActive = IsHovered;
            }

            private void OnHovered(object sender, EventArgs args)
            {
                Tint = SkinV2Color.Parse(Config.HoverColor);
            }

            private void OnLeftHover(object sender, EventArgs args)
            {
                Tint = SkinV2Color.Parse(Selected ? Config.SelectedItemColor : Config.ItemColor);
            }

            private void LayoutContent()
            {
                if (MarqueeLabel == null)
                    return;

                var left = Config.HorizontalPadding +
                           (OptionIcon.Visible ? Config.IconSize + 8 : 0);
                var right = Config.HorizontalPadding;
                MarqueeLabel.Alignment = Alignment.MidLeft;
                MarqueeLabel.X = left;
                var marqueeWidth = Math.Max(1, Width - left - right);
                if (Math.Abs(LastMarqueeWidth - marqueeWidth) > 0.5f)
                {
                    LastMarqueeWidth = marqueeWidth;
                    MarqueeLabel.ResetPosition();
                }

                MarqueeLabel.Width = marqueeWidth;
                OptionIcon.X = Config.HorizontalPadding;
            }
        }
    }
}
