using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemDefaultSkin : OptionsItemDropdown
    {
        public OptionsItemDefaultSkin(RectangleF containerRect, string name, Bindable<DefaultSkins> skin) : base(containerRect, name,
            new Dropdown(GetOptions(), new ScalableVector2(300, 35), 22,
                Colors.MainAccent, GetSelectedIndex(skin), 240))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (skin == null)
                    return;

                skin.Value = (DefaultSkins) args.Index;
                SkinManager.Load();
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>();

            foreach (DefaultSkins skin in Enum.GetValues(typeof(DefaultSkins)))
                options.Add(skin.ToString());

            return options;
        }

        /// <summary>
        /// </summary>
        /// <param name="skin"></param>
        /// <returns></returns>
        private static int GetSelectedIndex(Bindable<DefaultSkins> skin)
        {
            if (skin == null)
                return 0;

            return (int) skin.Value;
        }
    }
}