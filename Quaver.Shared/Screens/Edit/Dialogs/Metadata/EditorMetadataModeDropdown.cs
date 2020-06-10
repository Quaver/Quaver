using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs.Metadata
{
    public class EditorMetadataModeDropdown : LabelledDropdown
    {
        public GameMode SelectedMode
        {
            get
            {
                var mode = (GameMode) (Dropdown.SelectedIndex + 1);

                if ((int) mode == 3)
                    mode = GameMode.Keys7;
                
                return mode;
            }
        }

        /// <summary>
        ///     The index of 7K+1 in the dropdown
        /// </summary>
        public static int Keys7Plus1Index { get; } = 2;

        public EditorMetadataModeDropdown(Qua map) : base("GAME MODE: ", 20, new Dropdown(GetDropdownItems(),
            new ScalableVector2(140, 38), 22, ColorHelper.HexToColor($"#ffe76b"), GetSelectedIndex(map)))
        {
        }
        
        private static List<string> GetDropdownItems()
        {
            var values = new List<string>();

            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
                values.Add(ModeHelper.ToLongHand(mode));

            values.Add("7+1 Keys");
            
            return values;
        }
        
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        private static int GetSelectedIndex(Qua map)
        {
            var index = (int) map.Mode - 1;

            if (map.HasScratchKey)
                index = Keys7Plus1Index;

            return index;
        }
    }
}