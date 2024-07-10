using System;
using System.ComponentModel;
using System.Collections.Generic;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;
using Quaver.API.Helpers;

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

                if (args.Index == 0)
                {
                    ConfigManager.PrioritizedGameMode.Value = (GameMode)0;
                    return;
                }

                ConfigManager.PrioritizedGameMode.Value = ModeHelper.FromKeyCount(args.Index);
            };
        }

        private static List<string> GetOptions()
        {
            var list = new List<string>
            {
                "None",
            };

            for(var i = 1; i <= 10; i++)
            {
                var mode = ModeHelper.FromKeyCount(i);
                list.Add($"{i} Keys");
            }

            return list;
        }

        private static int GetSelectedIndex()
        {
            if (ConfigManager.PrioritizedGameMode == null)
                return 0;

            return (int)ConfigManager.PrioritizedGameMode.Value;
        }
    }
}
