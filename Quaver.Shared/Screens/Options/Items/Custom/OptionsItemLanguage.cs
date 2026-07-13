using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Localization;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemLanguage : OptionsItemDropdown
    {
        public OptionsItemLanguage(RectangleF containerRect, string name) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(250, 35), 20, Colors.MainAccent, GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                var language = QuaverLocalization.AvailableLanguages[args.Index];

                ConfigManager.Language.Value = language.CultureName;
            };
        }

        private static List<string> GetOptions() =>
            QuaverLocalization.AvailableLanguages
                .Select(x => LocalizationManager.Get(x.DisplayNameKey))
                .ToList();

        private static int GetSelectedIndex()
        {
            var cultureName = QuaverLocalization.GetLanguage(ConfigManager.Language.Value).CultureName;

            for (var i = 0; i < QuaverLocalization.AvailableLanguages.Count; i++)
            {
                if (QuaverLocalization.AvailableLanguages[i].CultureName == cultureName)
                    return i;
            }

            return 0;
        }
    }
}
