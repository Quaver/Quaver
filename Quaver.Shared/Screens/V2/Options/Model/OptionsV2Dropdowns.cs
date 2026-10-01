using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using ManagedBass;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Localization;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Quaver.Shared.Skinning.V2;
using Quaver.Shared.Window;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Wobble.Logging;

namespace Quaver.Shared.Screens.V2.Options.Model
{
    internal static class OptionsV2Dropdowns
    {
        public static Drawable FrameLimiter(Container host)
        {
            var values = Enum.GetValues(typeof(FpsLimitType)).Cast<FpsLimitType>().Where(value => value != FpsLimitType.WaylandVsync || RuntimeInformation.IsOSPlatform(OSPlatform.Linux));
            return Create(host, ConfigManager.FpsLimiterType, values.Select(value => new DropdownOption<FpsLimitType>(value, value.ToString())).ToArray());
        }

        public static Drawable ScrollDirection(Container host, GameMode mode) => 
            Create(host, ConfigManager.ScrollDirections[mode], Enum.GetValues(typeof(ScrollDirection)).Cast<ScrollDirection>().Select(value => new DropdownOption<ScrollDirection>(value, value.ToString())).ToArray());

        public static Drawable GameMode(Container host)
        {
            var entries = new[] { new DropdownOption<GameMode>((GameMode)0, LocalizationManager.Get("Screen_Selection_None")) }
                .Concat(ModeHelper.AllModes.Select(mode => new DropdownOption<GameMode>(mode, ModeHelper.ToLongHand(mode))))
                .ToArray();
            return Create(host, ConfigManager.PrioritizedGameMode, entries);
        }

        public static Drawable Language(Container host) =>
            Create(host, ConfigManager.Language, QuaverLocalization.AvailableLanguages.Select(language => new DropdownOption<string>(language.CultureName, LocalizationManager.Get(language.DisplayNameKey))).ToArray(), 250);

        public static Drawable DefaultSkin(Container host)
        {
            var dropdown = Create(host, ConfigManager.DefaultSkin, Enum.GetValues(typeof(DefaultSkins)).Cast<DefaultSkins>().Select(value => new DropdownOption<DefaultSkins>(value, value.ToString())).ToArray());
            dropdown.OptionSelected += (_, args) => ApplyDefaultSkin(args.Option.Value, true);
            return dropdown;
        }

        public static Drawable CustomSkin(Container host, Bindable<string> skin)
        {
            var options = SkinStore.GetSkins();
            var selected = FindCurrentCustomSkinOption(skin, options);
            var selection = new Bindable<string>(selected ?? options.FirstOrDefault() ?? skin.Value ?? string.Empty);

            var entries = options.Select(option => new DropdownOption<string>(option, option.Split('<')[0].TrimEnd())).ToArray();
            if (entries.Length == 0)
                entries = new[] { new DropdownOption<string>(selection.Value, selection.Value) };

            var dropdown = CreateOwned(host, selection, entries);
            dropdown.OptionSelected += (_, args) =>
            {
                if (!TryApplyCustomSkinOption(skin, args.Option.Value))
                    selection.Value = FindCurrentCustomSkinOption(skin, options) ?? string.Empty;
            };

            void UpdateSelection() => selection.Value = FindCurrentCustomSkinOption(skin, options) ?? string.Empty;
            EventHandler<BindableValueChangedEventArgs<string>> onSkinChanged = (_, _) => UpdateSelection();
            EventHandler<BindableValueChangedEventArgs<bool>> onSourceChanged = (_, _) => UpdateSelection();
            skin.ValueChanged += onSkinChanged;
            ConfigManager.UseSteamWorkshopSkin.ValueChanged += onSourceChanged;
            dropdown.Cleanup = () =>
            {
                skin.ValueChanged -= onSkinChanged;
                ConfigManager.UseSteamWorkshopSkin.ValueChanged -= onSourceChanged;
            };
            return dropdown;
        }

        public static Drawable ScreenResolution(Container host)
        {
            var current = FormatResolution(GameBase.Game.Graphics.PreferredBackBufferWidth, GameBase.Game.Graphics.PreferredBackBufferHeight);
            var values = GetAvailableResolutions();

            var selection = new Bindable<string>(current);
            var entries = values.OrderBy(value => int.Parse(value.Split('x')[0]))
                .Select(value => new DropdownOption<string>(value, value)).ToArray();
            var dropdown = CreateOwned(host, selection, entries);
            dropdown.OptionSelected += (_, args) =>
            {
                if (!TryApplyScreenResolution(args.Option.Value))
                {
                    selection.Value = FormatResolution(GameBase.Game.Graphics.PreferredBackBufferWidth, GameBase.Game.Graphics.PreferredBackBufferHeight);
                    if (!QuaverWindowManager.CanChangeResolutionOnScene)
                        NotificationManager.Show(NotificationLevel.Warning, "You cannot change resolutions while on this screen!");
                }
            };

            void UpdateSelection() => selection.Value = CaptureScreenResolution();
            EventHandler<BindableValueChangedEventArgs<int>> onWidthChanged = (_, _) => UpdateSelection();
            EventHandler<BindableValueChangedEventArgs<int>> onHeightChanged = (_, _) => UpdateSelection();
            ConfigManager.WindowWidth.ValueChanged += onWidthChanged;
            ConfigManager.WindowHeight.ValueChanged += onHeightChanged;
            dropdown.Cleanup = () =>
            {
                ConfigManager.WindowWidth.ValueChanged -= onWidthChanged;
                ConfigManager.WindowHeight.ValueChanged -= onHeightChanged;
            };
            return dropdown;
        }

        public static Drawable AudioOutputDevice(Container host)
        {
            var current = ConfigManager.AudioOutputDevice.Value;
            var devices = new List<string>();
            for (var i = 1; i < Bass.DeviceCount; i++)
                devices.Add(Bass.GetDeviceInfo(i).Name);
            if (devices.Count == 0 || !devices.Contains(current))
                devices.Insert(0, current);

            var selection = new Bindable<string>(current);
            var dropdown = CreateOwned(host, selection, devices.Select(value => new DropdownOption<string>(value, value)).ToArray(), 300);
            dropdown.OptionSelected += (_, args) =>
            {
                if (!TryApplyAudioOutputDevice(args.Option.Value))
                    selection.Value = ConfigManager.AudioOutputDevice.Value;
            };

            EventHandler<BindableValueChangedEventArgs<string>> onDeviceChanged = (_, _) => selection.Value = ConfigManager.AudioOutputDevice.Value;
            ConfigManager.AudioOutputDevice.ValueChanged += onDeviceChanged;
            dropdown.Cleanup = () => ConfigManager.AudioOutputDevice.ValueChanged -= onDeviceChanged;
            return dropdown;
        }

        public static bool IsAvailableDefaultSkin(string text) => Enum.TryParse(text, out DefaultSkins skin) && Enum.IsDefined(skin);

        public static bool TryApplyDefaultSkin(string text) => IsAvailableDefaultSkin(text) && ApplyDefaultSkin(Enum.Parse<DefaultSkins>(text));

        private static bool ApplyDefaultSkin(DefaultSkins skin, bool forceReload = false)
        {
            var changed = ConfigManager.DefaultSkin.Value != skin;
            if (changed)
                ConfigManager.DefaultSkin.Value = skin;

            if (changed || forceReload)
                RequestSkinReload();
            return true;
        }

        public static string CaptureCustomSkin(Bindable<string> skin) =>
            string.IsNullOrEmpty(skin.Value) ? "none" : $"{(ConfigManager.UseSteamWorkshopSkin.Value ? "workshop:" : "local:")}{skin.Value}";

        public static bool IsAvailableCustomSkin(string text)
        {
            if (!TryParseCustomSkin(text, out var workshop, out var name))
                return false;
            if (text == "none")
                return true;

            var options = SkinStore.GetSkins();
            return workshop
                ? options.Any(option => TryGetWorkshopSkinId(option, out var id) && id == name)
                : options.Contains(name, StringComparer.Ordinal);
        }

        public static bool TryApplyCustomSkin(Bindable<string> skin, string text)
        {
            if (!IsAvailableCustomSkin(text))
                return false;

            TryParseCustomSkin(text, out var workshop, out var name);
            if (text == "none")
            {
                if (skin.Value != string.Empty)
                {
                    skin.Value = string.Empty;
                    RequestSkinReload();
                }

                return true;
            }

            if (ConfigManager.UseSteamWorkshopSkin.Value == workshop && skin.Value == name)
                return true;

            ConfigManager.UseSteamWorkshopSkin.Value = workshop;
            skin.Value = name;
            RequestSkinReload();
            return true;
        }

        private static bool TryApplyCustomSkinOption(Bindable<string> skin, string option) =>
            TryGetWorkshopSkinId(option, out var id)
                ? TryApplyCustomSkin(skin, $"workshop:{id}")
                : TryApplyCustomSkin(skin, $"local:{option}");

        private static bool TryParseCustomSkin(string text, out bool workshop, out string name)
        {
            workshop = false;
            name = null;
            if (text == null)
                return false;
            if (text == "none")
            {
                name = string.Empty;
                return true;
            }

            if (text.StartsWith("workshop:", StringComparison.Ordinal))
            {
                workshop = true;
                name = text.Substring("workshop:".Length);
            }
            else if (text.StartsWith("local:", StringComparison.Ordinal))
                name = text.Substring("local:".Length);

            return !string.IsNullOrEmpty(name);
        }

        private static string FindCurrentCustomSkinOption(Bindable<string> skin, IEnumerable<string> options) =>
            string.IsNullOrEmpty(skin.Value)
                ? options.FirstOrDefault(option => option == "Default Skin")
                : ConfigManager.UseSteamWorkshopSkin.Value
                ? options.FirstOrDefault(option => TryGetWorkshopSkinId(option, out var id) && id == skin.Value)
                : options.FirstOrDefault(option => option == skin.Value);

        private static bool TryGetWorkshopSkinId(string option, out string id)
        {
            id = null;
            if (option == null || !option.EndsWith(">", StringComparison.Ordinal))
                return false;

            var start = option.LastIndexOf(" <", StringComparison.Ordinal);
            if (start < 0)
                return false;

            id = option.Substring(start + 2, option.Length - start - 3);
            return id.Length > 0;
        }

        public static string CaptureScreenResolution() =>
            FormatResolution(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);

        private static string FormatResolution(int width, int height) =>
            FormattableString.Invariant($"{width}x{height}");

        public static bool IsAvailableScreenResolution(string text)
        {
            if (!TryParseResolution(text, out var width, out var height))
                return false;
            if (width < ConfigManager.WindowWidth.MinValue || width > ConfigManager.WindowWidth.MaxValue ||
                height < ConfigManager.WindowHeight.MinValue || height > ConfigManager.WindowHeight.MaxValue)
                return false;

            if (width == ConfigManager.WindowWidth.Value && height == ConfigManager.WindowHeight.Value)
                return true;

            return QuaverWindowManager.CanChangeResolutionOnScene && GetAvailableResolutions().Contains(text);
        }

        public static bool TryApplyScreenResolution(string text)
        {
            if (!IsAvailableScreenResolution(text))
                return false;

            TryParseResolution(text, out var width, out var height);
            if (width == ConfigManager.WindowWidth.Value && height == ConfigManager.WindowHeight.Value)
                return true;

            ConfigManager.WindowWidth.Value = width;
            ConfigManager.WindowHeight.Value = height;
            (GameBase.Game as QuaverGame)?.ChangeResolution();
            return true;
        }

        private static bool TryParseResolution(string text, out int width, out int height)
        {
            width = 0;
            height = 0;
            var parts = text?.Split('x');
            return parts?.Length == 2 &&
                   int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out width) &&
                   int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out height);
        }

        private static HashSet<string> GetAvailableResolutions()
        {
            var current = FormatResolution(GameBase.Game.Graphics.PreferredBackBufferWidth, GameBase.Game.Graphics.PreferredBackBufferHeight);
            var values = new HashSet<string> { "640x360", "1024x576", "1152x648", current };
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                values.Add(FormatResolution(mode.Width, mode.Height));

            return values;
        }

        public static bool IsAvailableAudioOutputDevice(string device)
        {
            if (string.IsNullOrEmpty(device))
                return false;
            if (device == ConfigManager.AudioOutputDevice.Value)
                return true;
            if (GameBase.Game is not QuaverGame game || game.CurrentScreen?.Type == QuaverScreenType.Editor)
                return false;

            for (var i = 1; i < Bass.DeviceCount; i++)
            {
                if (device == Bass.GetDeviceInfo(i).Name)
                    return true;
            }

            return false;
        }

        public static bool TryApplyAudioOutputDevice(string device)
        {
            if (!IsAvailableAudioOutputDevice(device))
            {
                if ((GameBase.Game as QuaverGame)?.CurrentScreen?.Type == QuaverScreenType.Editor)
                    NotificationManager.Show(NotificationLevel.Error, "Please leave the editor before changing the output device.");
                return false;
            }
            if (device == ConfigManager.AudioOutputDevice.Value)
                return true;

            var previous = ConfigManager.AudioOutputDevice.Value;
            try
            {
                ConfigManager.AudioOutputDevice.Value = device;
                QuaverGame.SetAudioDevice(true);
                return true;
            }
            catch (Exception e)
            {
                ConfigManager.AudioOutputDevice.Value = previous;
                NotificationManager.Show(NotificationLevel.Error, "An error occurred while changing the audio output device.");
                Logger.Error(e, LogType.Runtime);
                return false;
            }
        }

        private static void RequestSkinReload()
        {
            Transitioner.FadeIn();
            SkinManager.TimeSkinReloadRequested = GameBase.Game.TimeRunning;
        }

        private static V2Dropdown<T> Create<T>(Container host, Bindable<T> value, IReadOnlyList<DropdownEntry<T>> entries, float width = 240) => 
            new V2Dropdown<T>(width, value, entries, FontManager.GetWobbleFont(Fonts.InterBold), CreateStyle(), host);

        private static OwnedDropdown<T> CreateOwned<T>(Container host, Bindable<T> value, IReadOnlyList<DropdownEntry<T>> entries, float width = 240) =>
            new OwnedDropdown<T>(width, value, entries, FontManager.GetWobbleFont(Fonts.InterBold), CreateStyle(), host);

        private static SkinV2DropdownConfig CreateStyle() => new SkinV2DropdownConfig
        {
            Height = 40,
            ItemHeight = 40,
            FontSize = 18,
            TriggerColor = "#181E25FF",
            ItemColor = "#181E25FF",
            HoverColor = "#354451FF",
            SelectedItemColor = "#6B83B2FF",
            TextColor = "#8CAFEAFF",
            IconColor = "#EBF3FFFF",
            CornerRadius = SkinV2BorderRadiusConfig.Normal
        };

        private sealed class OwnedDropdown<T> : V2Dropdown<T>
        {
            private Bindable<T> OwnedValue { get; }

            public Action Cleanup { get; set; }

            public OwnedDropdown(float width, Bindable<T> value, IReadOnlyList<DropdownEntry<T>> entries, WobbleFontStore font, SkinV2DropdownConfig config, Container host) : base(width, value, entries, font, config, host) => OwnedValue = value;

            public override void Destroy()
            {
                Cleanup?.Invoke();
                base.Destroy();
                OwnedValue.Dispose();
            }
        }
    }
}
