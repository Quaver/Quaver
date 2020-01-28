using System.Collections.Generic;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadDifficultyDropdown : LabelledDropdown
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DownloadDifficultyDropdown() : base("DIFFICULTY: ", 22,
            new Dropdown(GetDropdownItems(), new ScalableVector2(150, 38), 22, ColorHelper.HexToColor("#ffe76b")))
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems() => new List<string>
        {
            "None",
            "Beginner",
            "Easy",
            "Normal",
            "Hard",
            "Insane",
            "Expert"
        };
    }
}