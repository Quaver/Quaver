using System;
using System.ComponentModel;
using System.Collections.Generic;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemPrioritizedGameMode : OptionsItemDropdown
    {
        public OptionsItemPrioritizedGameMode(RectangleF containerRect, string name) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (ConfigManager.PrioritizedGameMode == null)
                    return;

                ConfigManager.PrioritizedGameMode.Value = args.Text switch
                {
                    "None" => (GameMode)0,
                    "4 Keys" => GameMode.Keys4,
                    "7 Keys" => GameMode.Keys7,
                    _ => throw new InvalidEnumArgumentException()
                };
            };
        }

        private static List<string> GetOptions() => new List<string>{ "None", "4 Keys", "7 Keys" };

        private static int GetSelectedIndex()
        {
            if (ConfigManager.PrioritizedGameMode == null)
                return 0;

            return (int)ConfigManager.PrioritizedGameMode.Value;
        }
    }
}
