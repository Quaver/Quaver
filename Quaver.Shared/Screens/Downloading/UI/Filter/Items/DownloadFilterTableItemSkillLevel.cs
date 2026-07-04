using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Filter.Items
{
    public class DownloadFilterTableItemSkillLevel : DownloadFilterTableItem
    {
        /// <summary>
        /// </summary>
        private Dropdown Dropdown { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>

        public DownloadFilterTableItemSkillLevel(int width) : base(width, "Skill Level")
        {
            Dropdown = new Dropdown(new List<string>
            {
                DownloadLocalization.Get("None"),
                DownloadLocalization.Get("Beginner"),
                DownloadLocalization.Get("Easy"),
                DownloadLocalization.Get("Normal"),
                DownloadLocalization.Get("Hard"),
                DownloadLocalization.Get("Insane"),
                DownloadLocalization.Get("Expert")
            }, new ScalableVector2(160, 34), 22, Colors.MainAccent)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X
            };
        }
    }

    public enum DownloadFilterSkillLevel
    {
        None,
        Beginner,
        Easy,
        Normal,
        Hard,
        Insane,
        Expert
    }
}
