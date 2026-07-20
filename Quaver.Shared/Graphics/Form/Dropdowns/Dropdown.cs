using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Options.Search;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Form.Dropdowns
{
    public class Dropdown : ImageButton
    {
        /// <summary>
        ///     The available options for the dropdown
        /// </summary>
        public List<string> Options { get; }

        /// <summary>
        ///     The index of the selected option in <see cref="Options"/>
        /// </summary>
        public int SelectedIndex { get; set; }

        /// <summary>
        ///     The color of the text elements
        /// </summary>
        public Color HoverColor { get; }

        /// <summary>
        ///     The chevron pointing down on the dropdown
        /// </summary>
        public Sprite Chevron { get; private set; }

        /// <summary>
        ///     The text of the selected item
        ///     <see cref="SelectedIndex"/> of <see cref="Options"/>
        /// </summary>
        public SpriteTextPlus SelectedText { get; private set; }

        /// <summary>
        ///     The amount of padding for elements on the x axis
        /// </summary>
        public const int PaddingX = 12;

        /// <summary>
        ///     The size of the dropdown font
        /// </summary>
        public int FontSize { get; }

        /// <summary>
        ///     Holds all of the dropdown items in the container
        ///
        ///     When the dropdown is opened/closed, this will give it a clipping rectangle effect, so that
        ///     the dropdown gradually is opening/closing
        /// </summary>
        public ScrollContainer ItemContainer { get; private set; }

        /// <summary>
        ///     The clickable items in the dropdown
        /// </summary>
        public List<DropdownItem> Items { get; private set; }

        /// <summary>
        ///     If the dropdown is currently open
        /// </summary>
        public bool Opened { get; private set; }

        /// <summary>
        ///     If the dropdown item list is waiting for its close animation to finish before hiding.
        /// </summary>
        private bool HidingItemsAfterClose { get; set; }

        /// <summary>
        ///     The line that divides the top element from the items
        /// </summary>
        public Sprite DividerLine { get; private set; }

        /// <summary>
        ///     The sprite that lights up when hovered
        /// </summary>
        public Sprite HoverSprite { get; private set; }

        /// <summary>
        ///     Event invoked when an item was selected in the dropdown
        /// </summary>
        public event EventHandler<DropdownClickedEventArgs> ItemSelected;

        /// <summary>
        ///     The alpha of the buttons when it's highlighted
        /// </summary>
        public float HighlightAlpha { get; set; } = 0.45f;

        /// <summary>
        ///     The max width of the dropdown's text
        /// </summary>
        public int MaxWidth { get; }

        /// <summary>
        /// </summary>
        private int MaxHeight { get; }

        /// <summary>
        ///     The height of the dropdown when opened
        /// </summary>
        public int OpenHeight
        {
            get
            {
                var height = (int)Height * Options.Count;

                if (MaxHeight != 0 && height >= MaxHeight)
                    height = MaxHeight;
                return height;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="size"></param>
        /// <param name="fontSize"></param>
        /// <param name="color"></param>
        /// <param name="selectedIndex"></param>
        /// <param name="maxWidth"></param>
        public Dropdown(List<string> options, ScalableVector2 size, int fontSize, Color? color = null, int selectedIndex = 0,
            int maxWidth = 0, int maxHeight = 0)
            : base(UserInterface.DropdownClosed)
        {
            Options = options;
            SelectedIndex = selectedIndex;
            HoverColor = color ?? Colors.MainAccent;
            FontSize = fontSize;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;

            if (Options == null || Options.Count == 0)
                throw new InvalidOperationException("You cannot create a dropdown with zero elements");

            Size = size;
            Tint = Colors.DarkGray;

            CreateHoverSprite();
            CreateChevron();
            CreateSelectedText();
            CreateDividerLine();
            CreateItemContainer();
            CreateItems();

            Hovered += OnHovered;
            LeftHover += OnHoverLeft;
            Clicked += OnClicked;
            ClickedOutside += OnClickedOutside;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (HidingItemsAfterClose && ItemContainer.Height <= 0)
            {
                HidingItemsAfterClose = false;
                SetItemVisibility(false);
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ItemSelected = null;
            base.Destroy();
        }

        /// <summary>
        ///     Creates <see cref="HoverSprite"/>
        /// </summary>
        private void CreateHoverSprite()
        {
            HoverSprite = new Sprite
            {
                Parent = this,
                Size = Size,
                Alpha = 0,
                Image = Image
            };
        }

        /// <summary>
        ///     Creates <see cref="Chevron"/>
        /// </summary>
        private void CreateChevron()
        {
            Chevron = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Height * 0.45f, Height * 0.45f),
                Tint = HoverColor,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_chevron_arrow_down),
                X = -PaddingX
            };
        }

        /// <summary>
        ///     Creates <see cref="SelectedText"/>
        /// </summary>
        private void CreateSelectedText()
        {
            SelectedText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), Options[SelectedIndex],
                FontSize)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = PaddingX,
                Tint = HoverColor
            };

            if (MaxWidth != 0)
                SelectedText.TruncateWithEllipsis(MaxWidth);
        }

        /// <summary>
        ///     Creates <see cref="DividerLine"/>
        /// </summary>
        private void CreateDividerLine()
        {
            DividerLine = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 2),
                Y = Height,
                Tint = new Color(HoverColor.R / 2, HoverColor.G / 2, HoverColor.B / 2),
                Alpha = 0
            };
        }

        /// <summary>
        ///     Creates <see cref="ItemContainer"/>
        /// </summary>
        private void CreateItemContainer()
        {
            var height = Height * Options.Count;

            ItemContainer = new ScrollContainer(new ScalableVector2(Width, 0),
                new ScalableVector2(Width, height))
            {
                Parent = this,
                Visible = false,
                Y = DividerLine.Y + DividerLine.Height,
                Scrollbar =
                {
                    Visible = false
                },
                Alpha = 0
            };
        }

        /// <summary>
        ///     Creates the items to be used in the dropdown
        /// </summary>
        public void CreateItems()
        {
            Items = new List<DropdownItem>();

            for (var i = 0; i < Options.Count; i++)
            {
                var item = new DropdownItem(this, i, i == Options.Count - 1 ? UserInterface.DropdownBottom : null)
                {
                    Y = i * Height,
                    IsClickable = false
                };

                Items.Add(item);
                ItemContainer.AddContainedDrawable(item);
            }

            SetItemVisibility(false);
        }

        /// <summary>
        ///     Replaces the dropdown options and rebuilds the item list.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="selectedIndex"></param>
        public void SetOptions(List<string> options, int selectedIndex = 0)
        {
            if (options == null || options.Count == 0)
                throw new InvalidOperationException("You cannot create a dropdown with zero elements");

            Close(0);

            if (Items != null)
            {
                foreach (var item in Items)
                {
                    ItemContainer.RemoveContainedDrawable(item);
                    item.Destroy();
                }
            }

            Options.Clear();
            Options.AddRange(options);
            SelectedIndex = Math.Clamp(selectedIndex, 0, Options.Count - 1);
            SelectedText.Text = Options[SelectedIndex];

            if (MaxWidth != 0)
                SelectedText.TruncateWithEllipsis(MaxWidth);

            ItemContainer.ContentContainer.Height = Height * Options.Count;
            CreateItems();
        }

        /// <summary>
        ///     Opens the dropdown menu
        /// </summary>
        public void Open(int time = 500)
        {
            Opened = true;
            HidingItemsAfterClose = false;
            SetItemVisibility(true);

            Image = UserInterface.DropdownOpen;
            HoverSprite.Image = UserInterface.DropdownOpen;

            DividerLine.ClearAnimations();

            Chevron.ClearAnimations();

            ItemContainer.ClearAnimations();

            var height = OpenHeight;

            if (time <= 0)
            {
                DividerLine.Alpha = 1;
                Chevron.Rotation = MathF.PI;
                ItemContainer.Height = height;
            }
            else
            {
                DividerLine.FadeTo(1, Easing.OutQuint, time / 2);
                Chevron.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.OutQuint, Chevron.Rotation, MathF.PI, time));
                ItemContainer.ChangeHeightTo(height, Easing.OutQuint, time);
            }

            Items.ForEach(x => x.IsClickable = true);
        }

        /// <summary>
        ///     Closes the dropdown menu
        /// </summary>
        public virtual void Close(int time = 500)
        {
            Opened = false;
            HidingItemsAfterClose = time > 0;

            Image = UserInterface.DropdownClosed;
            HoverSprite.Image = UserInterface.DropdownClosed;

            DividerLine.ClearAnimations();

            Chevron.ClearAnimations();

            ItemContainer.ClearAnimations();

            if (time <= 0)
            {
                DividerLine.Alpha = 0;
                Chevron.Rotation = 0;
                ItemContainer.Height = 0;
            }
            else
            {
                DividerLine.FadeTo(0, Easing.OutQuint, (int)(time * 2.5f));
                Chevron.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.OutQuint, Chevron.Rotation, 0, time));
                ItemContainer.ChangeHeightTo(0, Easing.OutQuint, time);
            }

            Items.ForEach(x => x.IsClickable = false);

            if (!HidingItemsAfterClose)
                SetItemVisibility(false);
        }

        /// <summary>
        ///     Toggles visibility of the closed dropdown item list and its child text.
        /// </summary>
        private void SetItemVisibility(bool visible)
        {
            ItemContainer.Visible = visible;
            ItemContainer.ContentContainer.Visible = visible;
            ItemContainer.Scrollbar.Visible = visible && MaxHeight != 0 && Height * Options.Count > OpenHeight;

            if (Items == null)
                return;

            Items.ForEach(x => x.Visible = visible);
        }

        /// <summary>
        ///     Restores the dropdown item list visibility after an external owner toggles the drawable tree.
        /// </summary>
        public virtual void ApplyItemVisibilityState() => SetItemVisibility(Opened);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHovered(object sender, EventArgs e)
        {
            HoverSprite.ClearAnimations();
            HoverSprite.FadeTo(HighlightAlpha, Easing.OutQuint, 75);

            SkinManager.Skin?.SoundHover?.CreateChannel()?.Play();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHoverLeft(object sender, EventArgs e)
        {
            HoverSprite.ClearAnimations();
            HoverSprite.FadeTo(0f, Easing.OutQuint, 75);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClicked(object sender, EventArgs e)
        {
            if (Opened)
                Close();
            else
                Open();
        }

        /// <summary>
        ///     Selects a new dropdown item to be the new value
        /// </summary>
        public void SelectItem(DropdownItem item, bool invokeEvent = true)
        {
            // Already selected.
            if (SelectedIndex == item.Index)
                return;

            SelectedText.Text = item.Text.Text;
            SelectedIndex = item.Index;

            if (invokeEvent)
                ItemSelected?.Invoke(this, new DropdownClickedEventArgs(item));
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnClickedOutside(object sender, EventArgs e)
        {
            var mousePoint = MouseManager.CurrentState.Position.ToPoint();

            if (ItemContainer.ScreenRectangle.Contains(mousePoint) || ScreenRectangle.Contains(mousePoint))
                return;

            if (Opened)
                Close();
        }
    }
}
