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
        public GameMode SelectedMode => (GameMode) (Dropdown.SelectedIndex + 1);

        public EditorMetadataModeDropdown(Qua map) : base("GAME MODE: ", 20, new Dropdown(GetDropdownItems(),
            new ScalableVector2(140, 38), 22, ColorHelper.HexToColor($"#ffe76b"), (int) map.Mode - 1))
        {
        }

        private static List<string> GetDropdownItems()
        {
            var values = new List<string>();

            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
                values.Add(ModeHelper.ToLongHand(mode));

            return values;
        }
    }
}