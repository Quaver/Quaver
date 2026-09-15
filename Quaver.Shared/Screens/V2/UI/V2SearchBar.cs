using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Graphics.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared V2 search bar: a rounded box with a search icon and a text box. While a search is
    ///     running it also shows a clear button, a line and a result text on the right.
    ///     The screen that uses it decides what a search does.
    /// </summary>
    public sealed class V2SearchBar : Container
    {
        private SkinV2SearchConfig Config { get; }

        private RoundedPanel Background { get; }

        private SearchTextbox Input { get; }

        private SpriteTextPlus ResultText { get; }

        private Sprite Separator { get; }

        private RoundedButton ClearButton { get; }

        /// <summary>
        ///     Whether the text box has keyboard focus. Set it to false to leave the box, for example on Escape.
        /// </summary>
        public bool IsFocused
        {
            get => Input.Focused;
            set => Input.Focused = value;
        }

        /// <summary>
        ///     Raised a short time after the user stops typing, not on every key press.
        ///     See <see cref="SkinV2SearchConfig.DebounceMilliseconds"/>.
        /// </summary>
        public event EventHandler<string> StoppedTyping;

        public event EventHandler ClearClicked;

        public V2SearchBar(WobbleFontStore font, SkinV2SearchConfig config, TextureRegion searchIcon,
            string placeholder)
        {
            Config = config;

            Background = new RoundedPanel(config.CornerRadius)
            {
                Parent = this,
                Tint = SkinV2Color.Parse(config.BackgroundColor)
            };

            Input = new SearchTextbox(font, config, searchIcon, placeholder)
            {
                Parent = Background,
                Alignment = Alignment.MidLeft
            };
            Input.OnStoppedTyping = query => StoppedTyping?.Invoke(this, query);

            ResultText = new SpriteTextPlus(font, string.Empty, config.FontSize)
            {
                Parent = Background,
                Alignment = Alignment.MidRight,
                X = -config.ResultRightInset,
                Tint = SkinV2Color.Parse(config.ResultTextColor),
                UsePreviousSpriteBatchOptions = true,
                Visible = false
            };

            Separator = new Sprite
            {
                Parent = Background,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(config.SeparatorWidth, config.SeparatorHeight),
                Tint = SkinV2Color.Parse(config.SeparatorColor),
                UsePreviousSpriteBatchOptions = true,
                Visible = false
            };

            ClearButton = new RoundedButton((sender, args) => ClearClicked?.Invoke(this, EventArgs.Empty))
            {
                Parent = Background,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(config.ClearButtonSize, config.ClearButtonSize),
                CornerRadius = config.ClearButtonSize / 2f,
                Tint = SkinV2Color.Parse(config.ClearButtonColor),
                PerformHoverFade = true,
                Depth = -100,
                Visible = false,
                IsInteractionEnabled = false
            };
            ClearButton.SetIcon(FontAwesome.Get(FontAwesomeIcon.fa_times),
                new Vector2(config.ClearIconSize, config.ClearIconSize));
            ClearButton.Icon.Tint = SkinV2Color.Parse(config.ClearIconColor);
        }

        /// <summary>
        ///     The text next to the clear button while a search is running, for example the result count.
        /// </summary>
        public void SetResultText(string text) => ResultText.Text = text ?? string.Empty;

        /// <summary>
        ///     Empties the text box without raising <see cref="StoppedTyping"/>.
        /// </summary>
        public void ClearText()
        {
            Input.RawText = string.Empty;
            Input.ReadjustTextbox();
        }

        /// <summary>
        ///     Lays out the bar. With no search running the text box uses the whole bar. With a search
        ///     running the clear button, line and result text take the right side.
        ///     Call this after a resize, and when a search starts or ends.
        /// </summary>
        public void Refresh(bool active)
        {
            ResultText.Visible = active;
            Separator.Visible = active;
            ClearButton.Visible = active;
            ClearButton.IsInteractionEnabled = active;

            if (!active)
            {
                Input.Size = new ScalableVector2(Math.Max(1, Width), Height);
                return;
            }

            var gap = Config.SeparatorGap;
            var resultWidth = Math.Min(Config.ResultWidth, ResultText.Width);

            ResultText.X = -Config.ResultRightInset;
            Separator.Size = new ScalableVector2(Config.SeparatorWidth, Math.Min(Config.SeparatorHeight, Height));
            Separator.X = -(Config.ResultRightInset + resultWidth + gap);
            ClearButton.X = Separator.X - Config.SeparatorWidth - gap;

            var reserved = -ClearButton.X + Config.ClearButtonSize + gap;
            Input.Size = new ScalableVector2(Math.Max(1, Width - reserved), Height);
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            Background?.SetSizeIfChanged(Size);
        }

        /// <summary>
        ///     The text box: an invisible input area on top of the bar, with the search icon on its left.
        /// </summary>
        private sealed class SearchTextbox : Textbox
        {
            private Color TextColor { get; }

            private Color PlaceholderColor { get; }

            private Sprite SearchIcon { get; }

            internal SearchTextbox(WobbleFontStore font, SkinV2SearchConfig config, TextureRegion searchIcon,
                string placeholder) : base(new ScalableVector2(1, 1), font, config.FontSize, "", placeholder)
            {
                TextColor = SkinV2Color.Parse(config.TextColor);
                PlaceholderColor = SkinV2Color.Parse(config.PlaceholderColor);
                Image = null;
                Alpha = 0;
                Cursor.Tint = SkinV2Color.Parse(config.CursorColor);
                InputText.X = config.TextLeftInset;
                InputEnabled = false;
                Scrollbar.Visible = false;

                AllowSubmission = false;

                StoppedTypingActionCalltime = config.DebounceMilliseconds;

                SearchIcon = new Sprite
                {
                    Parent = this,
                    Alignment = Alignment.MidLeft,
                    X = config.HorizontalPadding,
                    Region = searchIcon,
                    Size = new ScalableVector2(config.IconSize, config.IconSize),
                    Tint = SkinV2Color.Parse(config.IconColor),
                    UsePreviousSpriteBatchOptions = true
                };
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                InputText.Tint = string.IsNullOrEmpty(RawText) ? PlaceholderColor : TextColor;
                InputText.Alpha = 1;
            }

            protected override void OnRectangleRecalculated()
            {
                base.OnRectangleRecalculated();

                if (SearchIcon == null)
                    return;

                Button.SetSizeIfChanged(Size);
                ContentContainer.SetSizeIfChanged(Size);
            }
        }
    }
}
