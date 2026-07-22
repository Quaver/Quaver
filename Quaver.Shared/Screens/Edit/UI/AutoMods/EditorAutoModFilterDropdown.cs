using System.Collections.Generic;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModFilterDropdown : LabelledDropdown
    {

        public EditorAutoModFilterDropdown(Bindable<AutoModIssueCategory> category) : base(
            LocalizationManager.Get("Screen_Editor_FilterLabel"), 22,
            new Dropdown(GetOptions(), new ScalableVector2(190, 32), 18, ColorHelper.HexToColor("#45D6F5")))
        {
            Dropdown.ItemSelected += (o, e) => category.Value = (AutoModIssueCategory) e.Index;
            Dropdown.Depth = -1;
            Dropdown.Items.ForEach(x => x.Depth = -1);
        }

        private static List<string> GetOptions() => new List<string>()
        {
            LocalizationManager.Get("Screen_Editor_All"),
            LocalizationManager.Get("Screen_Editor_Files"),
            LocalizationManager.Get("Screen_Editor_HitObjects"),
            LocalizationManager.Get("Screen_Editor_Mapset"),
            LocalizationManager.Get("Screen_Editor_Metadata"),
            LocalizationManager.Get("Screen_Editor_ScrollVelocities"),
            LocalizationManager.Get("Screen_Editor_TimingPoints")
        };
    }
}
