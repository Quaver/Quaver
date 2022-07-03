using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using MonoGame.Extended;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemAudioOutputDevice : OptionsItemDropdown
    {
        public OptionsItemAudioOutputDevice(RectangleF containerRect, string name) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(300, 35), 22, Colors.MainAccent, GetSelectedIndex(), 240))
            => Dropdown.ItemSelected += (sender, args) =>
            {
                try
                {
                    var game = (QuaverGame) GameBase.Game;

                    if (game.CurrentScreen.Type == QuaverScreenType.Editor)
                    {
                        NotificationManager.Show(NotificationLevel.Error, "Please leave the editor before changing the output device.");
                        Dropdown.SelectItem(Dropdown.Items[GetSelectedIndex()], false);
                        return;
                    }

                    ConfigManager.AudioOutputDevice.Value = args.Options[args.Index];
                    QuaverGame.SetAudioDevice(true);
                }
                catch (Exception e)
                {
                    NotificationManager.Show(NotificationLevel.Error, "An error occurred while changing the audio output device.");
                    Logger.Error(e, LogType.Runtime);
                }
            };

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>();

            for (var i = 1; i < Bass.DeviceCount; i++)
                options.Add(Bass.GetDeviceInfo(i).Name);

            return options;
        }

        /// <summary>
        /// </summary>
        /// <param name="skin"></param>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            var index = GetOptions().FindIndex(x => x == ConfigManager.AudioOutputDevice.Value);
            return index == -1 ? 0 : index;
        }
    }
}