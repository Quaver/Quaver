using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
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
    internal sealed class OptionsPresetManageDialog : DialogScreen
    {

        private readonly OptionsPreset Preset;
        private readonly Action<OptionsPreset> OnSaved;
        private readonly Action<Guid> OnDeleted;

        private FlexContainer Content;
        private Sprite Title;
        private SpriteTextPlus Header;
        private Sprite Banner;
        private Sprite Panel;
        private Sprite Name;
        private SpriteTextPlus NameLabel;
        private Sprite NameBackground;
        private Textbox NameTextbox;
        private RoundedButton SaveButton;
        private RoundedButton DeleteButton;
        private bool Closing;

        public OptionsPresetManageDialog(OptionsPreset preset, Action<OptionsPreset> onSaved, Action<Guid> onDeleted) : base(0)
        {
            this.Preset = preset ?? throw new ArgumentNullException(nameof(preset));
            this.OnSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
            this.OnDeleted = onDeleted ?? throw new ArgumentNullException(nameof(onDeleted));
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

            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_ManagePreset").ToUpperInvariant(), 20)
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

            var bannerShade = new Sprite
            {
                Parent = Banner,
                Size = Banner.Size,
                Tint = Color.Black,
                Alpha = 0.5f
            };
            var description = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_PresetManageDescription"), 16)
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

            NameBackground = new Sprite
            {
                Parent = form,
                Size = new ScalableVector2(620, 40),
                Image = RoundedRectTextureCache.Get(620, 40, 6),
                Tint = ColorHelper.HexToColor("#181E25")
            };
            form.SetItemOptions(NameBackground, new FlexItemOptions { Basis = 40, Shrink = 0 });
            NameTextbox = new Textbox(NameBackground.Size, FontManager.GetWobbleFont(Fonts.InterBold), 18, Preset.Name, LocalizationManager.Get("Screen_Options_PresetName"))
            {
                Parent = NameBackground,
                Tint = Color.Transparent,
                AllowSubmission = false,
                MaxCharacters = 64
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

            SaveButton = new RoundedButton(OnSaveClicked)
            {
                Parent = buttons,
                Size = new ScalableVector2(180, 40),
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#27B06E")
            };
            SaveButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_SavePreset"), 18, Color.White);
            buttons.SetItemOptions(SaveButton, new FlexItemOptions { Basis = 180, Shrink = 0 });

            DeleteButton = new RoundedButton(OnDeleteClicked)
            {
                Parent = buttons,
                Size = SaveButton.Size,
                CornerRadius = 6,
                Tint = ColorHelper.HexToColor("#F9645D")
            };
            DeleteButton.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), LocalizationManager.Get("Screen_Options_DeletePreset"), 18, Color.White);
            buttons.SetItemOptions(DeleteButton, new FlexItemOptions { Basis = 180, Shrink = 0 });

            form.RefreshLayout();
            buttons.RefreshLayout();
            Content.RefreshLayout();
        }

        private void OnSaveClicked(object sender, EventArgs args)
        {
            if (Closing)
                return;

            var name = NameTextbox.RawText?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Editor_InvalidName"));
                return;
            }

            if (OptionsPressetStore.LoadAll().Any(other => other.Id != Preset.Id && string.Equals(other.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetNameTaken"));
                return;
            }

            OptionsPreset saved;
            try
            {
                saved = new OptionsPreset
                {
                    Id = Preset.Id,
                    Name = name,
                    Values = OptionsList.CapturePresetValues()
                };
                OptionsList.CapturePresetInputs(saved);
                OptionsPressetStore.Save(saved);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not save options preset {name} ({Preset.Id}): {e}", LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetSaveFailed"));
                return;
            }

            OnSaved(saved);
            Close();
        }

        private void OnDeleteClicked(object sender, EventArgs args)
        {
            if (Closing)
                return;

            try
            {
                OptionsPressetStore.Delete(Preset.Id);
            }
            catch (Exception e)
            {
                Logger.Error($"Could not delete options preset {Preset.Name} ({Preset.Id}): {e}", LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, LocalizationManager.Get("Screen_Options_PresetDeleteFailed"));
                return;
            }

            OnDeleted(Preset.Id);
            Close();
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (MouseManager.IsUniqueClick(MouseButton.Left) && NameBackground.IsHovered())
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
            SaveButton.IsClickable = false;
            DeleteButton.IsClickable = false;

            DialogManager.Dismiss(this);
        }
    }
}
