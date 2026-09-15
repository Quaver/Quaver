using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Window;
using Wobble;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.V2.Options.Catalog.Categories
{
    internal static class VideoOptions
    {
        internal static OptionsRowGroup[] Create() => new[]
        {
            new OptionsRowGroup(OptionsCategoryId.Video, "Screen_Options_Window",
                CreateScreenResolutionRow(),
                OptionsRowDefinition.Toggle("Screen_Options_EnableFullscreen", ConfigManager.WindowFullScreen),
                OptionsRowDefinition.Toggle("Screen_Options_EnableBorderlessWindow", ConfigManager.WindowBorderless)),
            new OptionsRowGroup(OptionsCategoryId.Video, "Screen_Options_FrameRate",
                CreateFrameLimiterRow(),
                OptionsRowDefinition.Button("Screen_Options_SetCustomFPS", "Screen_Options_SetCustomFpsButton",
                    ShowCustomFpsDialog),
                OptionsRowDefinition.Toggle("Screen_Options_DisplayFPSCounter", ConfigManager.FpsCounter))
        };

        private static OptionsRowDefinition CreateScreenResolutionRow() =>
            OptionsRowDefinition.Dropdown("Screen_Options_ScreenResolution", GetScreenResolutionOptions(),
                () => $"{GameBase.Game.Graphics.PreferredBackBufferWidth}x" +
                      $"{GameBase.Game.Graphics.PreferredBackBufferHeight}",
                ChangeResolution);

        /// <summary>
        ///     Every resolution the display supports, plus a few small ones that are useful in a window.
        ///     Same list as the old options menu.
        /// </summary>
        private static List<string> GetScreenResolutionOptions()
        {
            var options = new List<string> { "640x360", "1024x576", "1152x648" };

            foreach (var mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                var option = $"{mode.Width}x{mode.Height}";
                if (!options.Contains(option))
                    options.Add(option);
            }

            return options.OrderBy(x => int.Parse(x.Split('x')[0])).ToList();
        }

        private static bool ChangeResolution(string value)
        {
            if (!QuaverWindowManager.CanChangeResolutionOnScene)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot change resolutions while on this screen!");
                return false;
            }

            var split = value.Split('x');
            ConfigManager.WindowWidth.Value = int.Parse(split[0]);
            ConfigManager.WindowHeight.Value = int.Parse(split[1]);

            (GameBase.Game as QuaverGame)?.ChangeResolution();
            return true;
        }

        private static OptionsRowDefinition CreateFrameLimiterRow() =>
            OptionsRowDefinition.Dropdown("Screen_Options_FrameLimiter", GetFrameLimiterOptions(),
                () => ConfigManager.FpsLimiterType.Value.ToString(),
                value =>
                {
                    if (Enum.TryParse<FpsLimitType>(value, out var type))
                        ConfigManager.FpsLimiterType.Value = type;

                    return true;
                });

        /// <summary>
        ///     Every frame limiter type. Wayland VSync is only listed on Linux.
        /// </summary>
        private static List<string> GetFrameLimiterOptions()
        {
            var options = new List<string>();

            foreach (FpsLimitType type in Enum.GetValues(typeof(FpsLimitType)))
            {
                if (type == FpsLimitType.WaylandVsync && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    continue;

                options.Add(type.ToString());
            }

            return options;
        }

        private static void ShowCustomFpsDialog()
        {
            DialogManager.Show(new YesNoTextDialog("Custom FPS", "Enter a custom FPS value.",
                ConfigManager.CustomFpsLimit.Value.ToString(), "", value =>
                {
                    if (!int.TryParse(value, out var fps))
                    {
                        NotificationManager.Show(NotificationLevel.Error, "Please enter a valid FPS value.");
                        return;
                    }

                    ConfigManager.CustomFpsLimit.Value = fps;
                    NotificationManager.Show(NotificationLevel.Success,
                        $"Custom FPS set to {ConfigManager.CustomFpsLimit.Value}.");
                }));
        }
    }
}
