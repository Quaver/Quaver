using System.Collections.Generic;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModFilterDropdown : LabelledDropdown
    {

        public EditorAutoModFilterDropdown(Bindable<AutoModIssueCategory> category) : base("FILTER:", 22,
            new Dropdown(GetOptions(), new ScalableVector2(190, 32), 22, ColorHelper.HexToColor("#45D6F5")))
        {
            Dropdown.ItemSelected += (o, e) => category.Value = (AutoModIssueCategory) e.Index;
            Dropdown.Depth = -1;
            Dropdown.Items.ForEach(x => x.Depth = -1);
        }

        private static List<string> GetOptions() => new List<string>()
        {
            "All",
            "Files",
            "HitObjects",
            "Mapset",
            "Metadata",
            "Scroll Velocities",
            "Timing Points"
        };
    }
}