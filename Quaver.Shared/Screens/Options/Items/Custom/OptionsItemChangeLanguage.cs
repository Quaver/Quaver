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
    public class OptionsItemChangeLanguage : OptionsItemDropdown
    {
        public OptionsItemChangeLanguage(RectangleF containerRect, string name) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (ConfigManager.ChangeLanguage == null)
                    return;

                ConfigManager.ChangeLanguage.Value = args.Text switch
                {
                    "English" => Language.English,
                    "한국어" => Language.Korean,
                    _ => throw new InvalidEnumArgumentException()
                };
            };
        }

        private static List<string> GetOptions() => new List<string>{ "English", "한국어" };

        private static int GetSelectedIndex()
        {
            if (ConfigManager.ChangeLanguage == null)
                return 0;

            return (int)ConfigManager.ChangeLanguage.Value;
        }
    }
}
