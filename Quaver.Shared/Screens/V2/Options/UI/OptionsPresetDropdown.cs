using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The preset picker in the header. Clicking it opens a menu of the built-in and user presets.
    /// </summary>
    internal sealed class OptionsPresetDropdown : Container
    {
        private SkinV2OptionsPresetConfig Config { get; }

        private WobbleFontStore Font { get; }

        private IReadOnlyList<QuaverPresetDescriptor> Presets { get; }

        private RoundedButton Trigger { get; }

        private Sprite Menu { get; set; }

        internal bool IsOpen => Menu != null;

        internal OptionsPresetDropdown(float width, float height, SkinV2OptionsPresetConfig config)
        {
            Config = config;
            Font = FontManager.GetWobbleFont(config.Font);
            Presets = QuaverYamlConfigManager.GetPresets();
            Size = new ScalableVector2(width, height);

            Trigger = new RoundedButton((sender, args) =>
            {
                if (IsOpen)
                    CloseMenu();
                else
                    OpenMenu();
            })
            {
                Parent = this,
                Size = Size,
                CornerRadius = config.CornerRadius,
                Tint = SkinV2Color.Parse(config.BackgroundColor),
                PerformHoverFade = true,
                Depth = -100
            };
            Trigger.SetIcon(FontAwesome.Get(FontAwesomeIcon.fa_chevron_arrow_down),
                new Vector2(config.IconSize, config.IconSize));
            Trigger.Icon.Tint = SkinV2Color.Parse(config.TextColor);
            Trigger.SetLabel(Font, GetActiveLabel(), config.FontSize, SkinV2Color.Parse(config.TextColor));
            LayoutTrigger();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            LayoutTrigger();

            var mouse = MouseManager.CurrentState.Position;

            if (Menu != null && MouseManager.IsUniqueClick(MouseButton.Left) &&
                !Trigger.ScreenRectangle.Contains(mouse) && !Menu.ScreenRectangle.Contains(mouse))
                CloseMenu();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            if (Trigger == null || !Trigger.SizeDiffers(Size))
                return;

            Trigger.Size = Size;
            LayoutTrigger();
        }

        internal void CloseMenu()
        {
            OptionsDrawableCleanup.DestroyTree(Menu);
            Menu = null;
        }

        /// <summary>
        ///     Turns clicking on or off while the dropdown is hidden during a search. Hiding it is not
        ///     enough, because a button still takes clicks when only its parent is hidden.
        /// </summary>
        internal void SetInteractionEnabled(bool enabled)
        {
            if (!enabled)
                CloseMenu();

            Trigger.IsInteractionEnabled = enabled;
        }

        private void OpenMenu()
        {
            var padding = Config.MenuPadding;

            Menu = new RoundedPanel(Config.CornerRadius)
            {
                Parent = this,
                Position = new ScalableVector2(0, Height + Config.MenuGap),
                Size = new ScalableVector2(Width, padding * 2 + Presets.Count * Config.ItemHeight +
                                                  Math.Max(0, Presets.Count - 1) * Config.ItemSpacing),
                Tint = SkinV2Color.Parse(Config.MenuColor),
                DrawOrder = 200
            };

            for (var index = 0; index < Presets.Count; index++)
            {
                var preset = Presets[index];
                var selected = IsActive(preset);

                var item = new RoundedButton((sender, args) => SelectPreset(preset))
                {
                    Parent = Menu,
                    Position = new ScalableVector2(padding, padding + index * (Config.ItemHeight + Config.ItemSpacing)),
                    Size = new ScalableVector2(Width - padding * 2, Config.ItemHeight),
                    CornerRadius = Config.CornerRadius,
                    Tint = SkinV2Color.Parse(selected ? Config.SelectedItemColor : Config.ItemColor),
                    PerformHoverFade = true,
                    Depth = -200
                };
                item.SetLabel(Font, GetLabel(preset), Config.FontSize,
                    SkinV2Color.Parse(selected ? Config.SelectedTextColor : Config.TextColor));
            }
        }

        private void SelectPreset(QuaverPresetDescriptor preset)
        {
            if (!QuaverYamlConfigManager.TrySelectPreset(preset.Id, out var errors))
            {
                var reason = errors == null || errors.Count == 0 ? "Unknown error" : string.Join("; ", errors);
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("Screen_Options_PresetSaveFailed", reason), forceShow: true);
                CloseMenu();
                return;
            }

            Trigger.SetLabel(Font, GetActiveLabel(), Config.FontSize, SkinV2Color.Parse(Config.TextColor));
            LayoutTrigger();
            CloseMenu();
        }

        private static bool IsActive(QuaverPresetDescriptor preset) =>
            string.Equals(preset.Id, QuaverYamlConfigManager.ActivePresetId, StringComparison.OrdinalIgnoreCase);

        private string GetActiveLabel()
        {
            var active = Presets.FirstOrDefault(IsActive);
            return active == null ? LocalizationManager.Get("Screen_Options_PresetGraphics") : GetLabel(active);
        }

        /// <summary>
        ///     Built-in presets store a localization key. User presets store the name itself.
        /// </summary>
        private static string GetLabel(QuaverPresetDescriptor preset) => preset.IsBuiltIn
            ? LocalizationManager.Get(preset.NameOrLocalizationKey)
            : preset.NameOrLocalizationKey;

        /// <summary>
        ///     Puts the label on the left and the arrow on the right.
        /// </summary>
        private void LayoutTrigger()
        {
            if (Trigger?.Label != null)
            {
                Trigger.Label.Alignment = Alignment.MidLeft;
                Trigger.Label.X = Config.HorizontalPadding;
            }

            if (Trigger?.Icon != null)
            {
                Trigger.Icon.Alignment = Alignment.MidRight;
                Trigger.Icon.X = -Config.HorizontalPadding;
            }
        }
    }
}
