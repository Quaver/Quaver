using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemFrameLimiter : OptionsItemDropdown
    {
        public OptionsItemFrameLimiter(RectangleF containerRect, string name) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex()))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (ConfigManager.FpsLimiterType == null)
                    return;

                ConfigManager.FpsLimiterType.Value = (FpsLimitType) Enum.Parse(typeof(FpsLimitType), args.Text);
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>();

            foreach (FpsLimitType val in Enum.GetValues(typeof(FpsLimitType)))
            {
                if (val == FpsLimitType.WaylandVsync && !RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    continue;

                options.Add(val.ToString());
            }

            return options;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            if (ConfigManager.FpsLimiterType == null)
                return 0;

            var options = GetOptions();
            var selected = ConfigManager.FpsLimiterType.Value.ToString();
            var index = options.IndexOf(selected);

            return index == -1 ? 0 : index;
        }
    }
}
