using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Wobble;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.V2.Options.Catalog.Categories
{
    internal static class AudioOptions
    {
        internal static OptionsRowGroup[] Create() => new[]
        {
            new OptionsRowGroup(OptionsCategoryId.Audio, "Screen_Options_Output",
                OptionsRowDefinition.Dropdown("Screen_Options_AudioOutputDevice", GetOutputDeviceOptions(),
                    () => ConfigManager.AudioOutputDevice.Value, ChangeOutputDevice)),
            new OptionsRowGroup(OptionsCategoryId.Audio, "Screen_Options_Volume",
                OptionsRowDefinition.Slider("Screen_Options_MasterVolume", ConfigManager.VolumeGlobal,
                    value => $"{value}%"),
                OptionsRowDefinition.Slider("Screen_Options_MusicVolume", ConfigManager.VolumeMusic,
                    value => $"{value}%"),
                OptionsRowDefinition.Slider("Screen_Options_MenuMusicVolume", ConfigManager.VolumeMenuMusic,
                    value => $"{value}%"),
                OptionsRowDefinition.Slider("Screen_Options_EffectVolume", ConfigManager.VolumeEffect,
                    value => $"{value}%"),
                OptionsRowDefinition.Toggle("Screen_Options_MuteAudioWhenUnfocused",
                    ConfigManager.MuteAudioOnWindowInactive)),
            new OptionsRowGroup(OptionsCategoryId.Audio, "Screen_Options_Offset",
                OptionsRowDefinition.Slider("Screen_Options_GlobalAudioOffset", ConfigManager.GlobalAudioOffset,
                    value => $"{value} ms"),
                OptionsRowDefinition.Slider("Screen_Options_VisualOffset", ConfigManager.VisualOffset,
                    value => $"{value} ms"),
                OptionsRowDefinition.Button("Screen_Options_CalibrateOffset", "Screen_Options_CalibrateOffsetButton",
                    StartOffsetCalibration)),
            new OptionsRowGroup(OptionsCategoryId.Audio, "Screen_Options_Effects",
                OptionsRowDefinition.Toggle("Screen_Options_PitchAudioWithPlaybackRate", ConfigManager.Pitched))
        };

        /// <summary>
        ///     The audio devices reported by Bass. A dropdown needs at least one option so the current
        ///     device is listed when Bass reports none.
        /// </summary>
        private static List<string> GetOutputDeviceOptions()
        {
            var options = new List<string>();

            for (var i = 1; i < Bass.DeviceCount; i++)
                options.Add(Bass.GetDeviceInfo(i).Name);

            if (options.Count == 0)
                options.Add(ConfigManager.AudioOutputDevice.Value ?? "Default");

            return options;
        }

        private static bool ChangeOutputDevice(string value)
        {
            var game = (QuaverGame) GameBase.Game;

            if (game.CurrentScreen?.Type == QuaverScreenType.Editor)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    "Please leave the editor before changing the output device.");
                return false;
            }

            try
            {
                ConfigManager.AudioOutputDevice.Value = value;
                QuaverGame.SetAudioDevice(true);
                return true;
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error,
                    "An error occurred while changing the audio output device.");
                Logger.Error(e, LogType.Runtime);
                return false;
            }
        }

        /// <summary>
        ///     Starts a calibration map (with No Fail on) that the player taps along to, to find their offset.
        ///     Only allowed from the main menu and song select.
        /// </summary>
        private static void StartOffsetCalibration()
        {
            if (GameBase.Game is not QuaverGame game)
                return;

            if (game.CurrentScreen?.Type != QuaverScreenType.Menu && game.CurrentScreen?.Type != QuaverScreenType.Select)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "Finish what you're doing before calibrating a new offset");
                return;
            }

            const string path = "Quaver.Resources/Maps/Offset/offset.qua";
            var qua = Qua.Parse(GameBase.Game.Resources.Get(path));

            if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed && AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();

            game.CurrentScreen?.Exit(() =>
            {
                MapManager.Selected.Value = Map.FromQua(qua, path, true);
                MapManager.Selected.Value.Qua = qua;

                ModManager.RemoveAllMods();
                ModManager.AddMod(ModIdentifier.NoFail);

                BackgroundHelper.Load(MapManager.Selected.Value);
                DialogManager.Dismiss(DialogManager.Dialogs.Last());

                return new GameplayScreen(qua, "", new List<Score>(), null, false, 0, true);
            });
        }
    }
}
