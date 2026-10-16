using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    public enum V2ModalButtonStyle
    {
        Confirm,
        Cancel,
        Neutral
    }

    public class ModalDialog : DialogScreen
    {
        private readonly SkinStoreV2Lease Skin;
        private readonly SkinV2ModalConfig Config;
        private readonly Action OnClosed;
        private readonly string Title;
        private readonly string Description;

        private readonly List<(Sprite Background, Textbox Textbox)> TextInputs = new List<(Sprite, Textbox)>();

        private readonly List<RoundedButton> Buttons = new List<RoundedButton>();

        private FlexContainer ModalLayout { get; set; }
        private Sprite Panel { get; set; }
        private FlexContainer FormLayout { get; set; }
        private FlexContainer ButtonsLayout { get; set; }

        private int FormItemCount { get; set; }
        private float FormItemsHeight { get; set; }
        private bool Closing { get; set; }
        private bool ClosedCallbackInvoked { get; set; }

        public ModalDialog(string title, string description, Action onClosed = null) : base(0)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            Skin = SkinManager.AcquireV2();
            Config = Skin.Config.Shared.Modal;
            OnClosed = onClosed;
            Title = title;
            Description = description;

            FadeTo(0.85f, Easing.Linear, Config.AnimationDurationMilliseconds);
            CreateContent();
        }

        public override void CreateContent()
        {
            if (ModalLayout != null)
                return;

            Container.Size = Size;

            ModalLayout = new FlexContainer
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(Config.Width, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.FlexStart,
                RowGap = Config.SectionGap
            };

            var header = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), Title.ToUpperInvariant(), Config.TitleFontSize)
            {
                Alignment = Alignment.MidCenter,
                Tint = SkinV2Color.Parse(Config.TextColor)
            };

            var titleBackground = new Sprite
            {
                Parent = ModalLayout,
                Size = new ScalableVector2(header.Width + Config.TitleHorizontalPadding, Config.TitleHeight),
                Image = RoundedRectTextureCache.Get(header.Width + Config.TitleHorizontalPadding, Config.TitleHeight, Config.CornerRadius),
                Tint = SkinV2Color.Parse(Config.TitleColor)
            };
            header.Parent = titleBackground;
            ModalLayout.SetItemOptions(titleBackground, new FlexItemOptions { Basis = Config.TitleHeight, Shrink = 0 });

            var banner = new Sprite
            {
                Parent = ModalLayout,
                Size = new ScalableVector2(Config.Width, Config.BannerHeight),
                Image = Skin.LoadTexture(Config.BannerImage, UserInterface.DefaultBanner)
            };
            ModalLayout.SetItemOptions(banner, new FlexItemOptions { Basis = Config.BannerHeight, Shrink = 0 });

            new Sprite
            {
                Parent = banner,
                Size = banner.Size,
                Tint = SkinV2Color.Parse(Config.BannerShadeColor)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), Description, Config.DescriptionFontSize)
            {
                Parent = banner,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center,
                Tint = SkinV2Color.Parse(Config.TextColor)
            };

            Panel = new Sprite
            {
                Parent = ModalLayout,
                Size = new ScalableVector2(Config.Width, 1),
                Tint = SkinV2Color.Parse(Config.PanelColor)
            };

            FormLayout = new FlexContainer
            {
                Parent = Panel,
                Position = new ScalableVector2(Config.FormInset, Config.FormInset),
                Size = new ScalableVector2(Config.FormWidth, 1),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.FlexStart,
                RowGap = Config.FormItemGap
            };

            ButtonsLayout = new FlexContainer
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -Config.ButtonBottomInset,
                Size = new ScalableVector2(1, Config.ButtonHeight),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustifyContent.Center,
                ColumnGap = Config.ButtonGap
            };

            RefreshLayout();
        }

        public Textbox AddTextInput(string label = "", string initialText = "", string placeholder = "", int maxCharacters = 64, bool initiallyFocused = false, Action<string> onSubmit = null)
        {
            if (maxCharacters <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxCharacters));

            if (!string.IsNullOrWhiteSpace(label))
            {
                var labelText = new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), label, Config.ControlFontSize)
                {
                    Alignment = Alignment.MidCenter,
                    Tint = SkinV2Color.Parse(Config.TextColor)
                };

                var labelBackground = new Sprite
                {
                    Size = new ScalableVector2(labelText.Width + Config.FieldLabelHorizontalPadding, Config.FieldLabelHeight),
                    Image = RoundedRectTextureCache.Get(labelText.Width + Config.FieldLabelHorizontalPadding, Config.FieldLabelHeight, Config.CornerRadius),
                    Tint = SkinV2Color.Parse(Config.TitleColor)
                };
                labelText.Parent = labelBackground;
                AddContent(labelBackground, Config.FieldLabelHeight);
            }

            var inputBackground = new Sprite
            {
                Size = new ScalableVector2(Config.FormWidth, Config.InputHeight),
                Image = RoundedRectTextureCache.Get(Config.FormWidth, Config.InputHeight, Config.CornerRadius),
                Tint = SkinV2Color.Parse(Config.InputColor)
            };
            AddContent(inputBackground, Config.InputHeight);

            var textbox = new Textbox(inputBackground.Size, FontManager.GetWobbleFont(Config.Font), Config.ControlFontSize, initialText ?? string.Empty, placeholder ?? string.Empty, onSubmit)
            {
                Parent = inputBackground,
                Tint = Color.Transparent,
                AllowSubmission = onSubmit != null,
                MaxCharacters = maxCharacters,
                Focused = initiallyFocused
            };

            TextInputs.Add((inputBackground, textbox));
            return textbox;
        }

        public T AddContent<T>(T content, float height) where T : Drawable
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            content.Parent = FormLayout;
            FormLayout.SetItemOptions(content, new FlexItemOptions { Basis = height, Shrink = 0 });
            FormItemsHeight += height;
            FormItemCount++;
            RefreshLayout();
            return content;
        }

        public RoundedButton AddButton(string label, V2ModalButtonStyle style, Action<ModalDialog> action)
        {
            if (label == null)
                throw new ArgumentNullException(nameof(label));
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var button = new RoundedButton((sender, args) =>
            {
                if (!Closing)
                    action(this);
            })
            {
                Parent = ButtonsLayout,
                Size = new ScalableVector2(Config.ButtonWidth, Config.ButtonHeight),
                CornerRadius = Config.CornerRadius,
                Tint = GetButtonColor(style)
            };
            button.SetLabel(FontManager.GetWobbleFont(Config.Font), label, Config.ControlFontSize, SkinV2Color.Parse(Config.TextColor));
            ButtonsLayout.SetItemOptions(button, new FlexItemOptions { Basis = Config.ButtonWidth, Shrink = 0 });

            Buttons.Add(button);
            RefreshLayout();
            return button;
        }

        private Color GetButtonColor(V2ModalButtonStyle style) => style switch
        {
            V2ModalButtonStyle.Confirm => SkinV2Color.Parse(Config.ConfirmButtonColor),
            V2ModalButtonStyle.Cancel => SkinV2Color.Parse(Config.CancelButtonColor),
            V2ModalButtonStyle.Neutral => SkinV2Color.Parse(Config.NeutralButtonColor),
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null)
        };

        private void RefreshLayout()
        {
            var formHeight = FormItemsHeight + Math.Max(0, FormItemCount - 1) * Config.FormItemGap;
            FormLayout.Height = Math.Max(1, formHeight);

            var panelHeight = Config.FormInset + formHeight + Config.FormToButtonsGap + Config.ButtonHeight + Config.ButtonBottomInset;
            Panel.Height = Math.Max(1, panelHeight);
            Panel.Image = RoundedRectTextureCache.Get(Config.Width, Panel.Height, Config.CornerRadius);
            ModalLayout.SetItemOptions(Panel, new FlexItemOptions { Basis = Panel.Height, Shrink = 0 });

            ButtonsLayout.Width = Math.Max(1, Buttons.Count * Config.ButtonWidth + Math.Max(0, Buttons.Count - 1) * Config.ButtonGap);

            ModalLayout.Height = Config.TitleHeight + Config.BannerHeight + Panel.Height + Config.SectionGap * 2;

            FormLayout.RefreshLayout();
            ButtonsLayout.RefreshLayout();
            ModalLayout.RefreshLayout();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Container.Width != Width || Container.Height != Height)
                Container.Size = Size;
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (MouseManager.IsUniqueClick(MouseButton.Left))
            {
                foreach (var input in TextInputs)
                {
                    if (!input.Background.IsHovered())
                        continue;

                    foreach (var otherInput in TextInputs)
                        otherInput.Textbox.Focused = otherInput.Textbox == input.Textbox;
                    return;
                }
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) || MouseManager.IsUniqueClick(MouseButton.Left) && !ModalLayout.IsHovered())
                Close();
        }

        public void Close()
        {
            if (Closing)
                return;

            Closing = true;
            ReleaseControls();
            InvokeClosedCallback();
            DialogManager.Dismiss(this);
        }

        private void ReleaseControls()
        {
            foreach (var input in TextInputs)
            {
                input.Textbox.Focused = false;
                input.Textbox.Visible = false;
            }

            foreach (var button in Buttons)
                button.IsClickable = false;
        }

        private void InvokeClosedCallback()
        {
            if (ClosedCallbackInvoked)
                return;

            ClosedCallbackInvoked = true;
            OnClosed?.Invoke();
        }

        public override void Destroy()
        {
            Closing = true;
            ReleaseControls();
            InvokeClosedCallback();
            base.Destroy();
            Skin.Dispose();
        }
    }
}
