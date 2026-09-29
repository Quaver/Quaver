using System;
using System.Collections.Generic;
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

        public static Drawable GameMode(Container host) => 
            Create(host, ConfigManager.PrioritizedGameMode, ModeHelper.AllModes.Select(mode => new DropdownOption<GameMode>(mode, ModeHelper.ToLongHand(mode))).ToArray());

        public static Drawable Language(Container host) =>
            Create(host, ConfigManager.Language, QuaverLocalization.AvailableLanguages.Select(language => new DropdownOption<string>(language.CultureName, LocalizationManager.Get(language.DisplayNameKey))).ToArray(), 250);

        public static Drawable DefaultSkin(Container host)
        {
            var dropdown = Create(host, ConfigManager.DefaultSkin, Enum.GetValues(typeof(DefaultSkins)).Cast<DefaultSkins>().Select(value => new DropdownOption<DefaultSkins>(value, value.ToString())).ToArray());
            dropdown.OptionSelected += (_, _) => RequestSkinReload();
            return dropdown;
        }

        public static Drawable CustomSkin(Container host, Bindable<string> skin)
        {
            var options = SkinStore.GetSkins();
            var selected = options.FirstOrDefault(option => ConfigManager.UseSteamWorkshopSkin.Value ? !string.IsNullOrEmpty(skin.Value) && option.Contains(skin.Value) : option == skin.Value);
            var selection = new Bindable<string>(selected ?? options.FirstOrDefault() ?? skin.Value ?? string.Empty);

            var entries = options.Select(option => new DropdownOption<string>(option, option.Split('<')[0].TrimEnd())).ToArray();
            if (entries.Length == 0)
                entries = new[] { new DropdownOption<string>(selection.Value, selection.Value) };

            var dropdown = CreateOwned(host, selection, entries);
            dropdown.OptionSelected += (_, args) =>
            {
                var option = args.Option.Value;
                if (option.Contains('<') && option.Contains('>'))
                {
                    ConfigManager.UseSteamWorkshopSkin.Value = true;
                    skin.Value = option.Split('<')[1].Replace(">", "");
                }
                else
                {
                    ConfigManager.UseSteamWorkshopSkin.Value = false;
                    skin.Value = option;
                }

                RequestSkinReload();
            };
            return dropdown;
        }

        public static Drawable ScreenResolution(Container host)
        {
            var current = $"{GameBase.Game.Graphics.PreferredBackBufferWidth}x{GameBase.Game.Graphics.PreferredBackBufferHeight}";
            var values = new HashSet<string> { "640x360", "1024x576", "1152x648", current };
            foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                values.Add($"{mode.Width}x{mode.Height}");

            var selection = new Bindable<string>(current);
            var entries = values.OrderBy(value => int.Parse(value.Split('x')[0]))
                .Select(value => new DropdownOption<string>(value, value)).ToArray();
            var dropdown = CreateOwned(host, selection, entries);
            dropdown.OptionSelected += (_, args) =>
            {
                if (!QuaverWindowManager.CanChangeResolutionOnScene)
                {
                    selection.Value = current;
                    NotificationManager.Show(NotificationLevel.Warning, "You cannot change resolutions while on this screen!");
                    return;
                }

                var size = args.Option.Value.Split('x');
                ConfigManager.WindowWidth.Value = int.Parse(size[0]);
                ConfigManager.WindowHeight.Value = int.Parse(size[1]);
                (GameBase.Game as QuaverGame)?.ChangeResolution();
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
            var dropdown = CreateOwned(host, selection,
                devices.Select(value => new DropdownOption<string>(value, value)).ToArray(), 300);
            dropdown.OptionSelected += (_, args) =>
            {
                try
                {
                    var game = (QuaverGame)GameBase.Game;
                    if (game.CurrentScreen.Type == QuaverScreenType.Editor)
                    {
                        selection.Value = ConfigManager.AudioOutputDevice.Value;
                        NotificationManager.Show(NotificationLevel.Error, "Please leave the editor before changing the output device.");
                        return;
                    }

                    ConfigManager.AudioOutputDevice.Value = args.Option.Value;
                    QuaverGame.SetAudioDevice(true);
                }
                catch (Exception e)
                {
                    selection.Value = ConfigManager.AudioOutputDevice.Value;
                    NotificationManager.Show(NotificationLevel.Error, "An error occurred while changing the audio output device.");
                    Logger.Error(e, LogType.Runtime);
                }
            };
            return dropdown;
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

            public OwnedDropdown(float width, Bindable<T> value, IReadOnlyList<DropdownEntry<T>> entries, WobbleFontStore font, SkinV2DropdownConfig config, Container host) : base(width, value, entries, font, config, host) => OwnedValue = value;

            public override void Destroy()
            {
                base.Destroy();
                OwnedValue.Dispose();
            }
        }
    }
}
