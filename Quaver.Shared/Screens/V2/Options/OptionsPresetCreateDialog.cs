using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.V2.Options.Model;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options
{
    internal sealed class OptionsPresetCreateDialog : DialogScreen
    {
        private readonly Action<OptionsPreset> OnCreated;
        private FlexContainer Content;
        private Sprite Title;
        private SpriteTextPlus Header;
        private Sprite Banner;
        private Sprite BannerAlpha;
        private Sprite Name;
        private Sprite BackgroundInput;
        private Sprite Panel;
        private SpriteTextPlus Description;
        private SpriteTextPlus NameLabel;
        private Textbox NameTextbox;
        private RoundedButton CreateButton;
        private RoundedButton CancelButton;
        private bool Closing;

        public OptionsPresetCreateDialog(Action<OptionsPreset> onCreated) : base(0)
        {
            this.OnCreated = onCreated ?? throw new ArgumentNullException(nameof(onCreated));
            FadeTo(0.85f, Easing.Linear, 150);
            CreateContent();
        }

        public override void CreateContent()
        {
            Content = new FlexContainer
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(660, 369),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.FlexStart,
                RowGap = 10
            };

            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_CreatePreset").ToUpperInvariant(), 20)
            {
                Alignment = Alignment.MidCenter
            };

            Title = new Sprite
            {
                Parent = Content,
                Size = new ScalableVector2(Header.Width + 40, 36),
                Image = RoundedRectTextureCache.Get(Header.Width + 40, 36, 6),
                Tint = ColorHelper.HexToColor("#4B5973")
            };
            Header.Parent = Title;
            Content.SetItemOptions(Title, new FlexItemOptions { Basis = 36, Shrink = 0 });

            Banner = new Sprite
            {
                Parent = Content,
                Size = new ScalableVector2(660, 128),
                Image = UserInterface.DefaultBanner
            };
            Content.SetItemOptions(Banner, new FlexItemOptions { Basis = 128, Shrink = 0 });

            BannerAlpha = new Sprite
            {
                Parent = Banner,
                Size = Banner.Size,
                Tint = Color.Black,
                Alpha = 0.45f
            };

            Description = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_PresetDescription"), 16)
            {
                Parent = Banner,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };

            Panel = new Sprite
            {
                Parent = Content,
                Size = new ScalableVector2(660, 185),
                Image = RoundedRectTextureCache.Get(660, 185, 6),
                Tint = ColorHelper.HexToColor("#273038")
            };
            Content.SetItemOptions(Panel, new FlexItemOptions { Basis = 185, Shrink = 0 });

            var form = new FlexContainer
            {
                Parent = Panel,
                Position = new ScalableVector2(10, 10),
                Size = new ScalableVector2(620, 84),
                Direction = FlexDirection.Column,
                AlignItems = FlexAlignItems.FlexStart,
                RowGap = 10
            };

            NameLabel = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_PresetName"), 18)
            {
                Alignment = Alignment.MidCenter
            };

            Name = new Sprite
            {
                Parent = form,
                Size = new ScalableVector2(NameLabel.Width + 32, 34),
                Image = RoundedRectTextureCache.Get(NameLabel.Width + 32, 34, 6),
                Tint = ColorHelper.HexToColor("#4B5973")
            };
            NameLabel.Parent = Name;
            form.SetItemOptions(Name, new FlexItemOptions { Basis = 34, Shrink = 0 });

            BackgroundInput = new Sprite
            {
                Parent = form,
                Size = new ScalableVector2(620, 40),
                Image = RoundedRectTextureCache.Get(620, 40, 6),
                Tint = ColorHelper.HexToColor("#181E25")
            };
            form.SetItemOptions(BackgroundInput, new FlexItemOptions { Basis = 40, Shrink = 0 });

            NameTextbox = new Textbox(BackgroundInput.Size, FontManager.GetWobbleFont(Fonts.InterBold), 18, string.Empty, LocalizationManager.Get("Screen_Options_PresetName"))
            {
                Parent = BackgroundInput,
                Tint = Color.Transparent,
                AllowSubmission = false,
                MaxCharacters = 16,
                Focused = true
            };

            var buttons = new FlexContainer
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Y = -20,
                Size = new ScalableVector2(370, 40),
                Direction = FlexDirection.Row,
                AlignItems = FlexAlignItems.Center,
                JustifyContent = FlexJustifyContent.Center,
                ColumnGap = 10
            };

            CreateButton = new RoundedButton(OnCreateClicked)
            {
                Parent = buttons,
                Size = new ScalableVector2(180, 40),
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#27B06E")
            };
            CreateButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_CreatePreset"), 18, Color.White);
            buttons.SetItemOptions(CreateButton, new FlexItemOptions { Basis = 180, Shrink = 0 });

            CancelButton = new RoundedButton((sender, args) => Close())
            {
                Parent = buttons,
                Size = CreateButton.Size,
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#F9645D")
            };
            CancelButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("SkinEditor_Cancel"), 18, Color.White);
            buttons.SetItemOptions(CancelButton, new FlexItemOptions { Basis = 180, Shrink = 0 });

            form.RefreshLayout();
            buttons.RefreshLayout();
            Content.RefreshLayout();
        }

        private void OnCreateClicked(object sender, EventArgs args)
        {
            var presetName = NameTextbox.RawText?.Trim();
            if (string.IsNullOrWhiteSpace(presetName))
            {
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Editor_InvalidName"));
                return;
            }

            if (OptionsPressetStore.LoadAll().Any(preset => string.Equals(preset.Name, presetName, StringComparison.OrdinalIgnoreCase)))
            {
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetNameTaken"));
                return;
            }

            OptionsPreset preset;
            try
            {
                preset = new OptionsPreset
                {
                    Id = Guid.NewGuid(),
                    Name = presetName,
                    Values = OptionsList.CapturePresetValues()
                };
                OptionsList.CapturePresetInputs(preset);
                OptionsPressetStore.Save(preset);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not create options preset: {e}", LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetSaveFailed"));
                return;
            }

            OnCreated(preset);
            Close();
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (MouseManager.IsUniqueClick(MouseButton.Left) && BackgroundInput.IsHovered())
            {
                NameTextbox.Focused = true;
                return;
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) || MouseManager.IsUniqueClick(MouseButton.Left) && !Content.IsHovered())
                Close();
        }

        private void Close()
        {
            if (Closing)
                return;

            Closing = true;
            NameTextbox.Focused = false;
            NameTextbox.Visible = false;

            DialogManager.Dismiss(this);
        }
    }
}
