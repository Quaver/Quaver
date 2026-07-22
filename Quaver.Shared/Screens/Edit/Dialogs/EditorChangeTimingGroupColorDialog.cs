using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs;

public class EditorChangeTimingGroupColorDialog : ColorDialog
{
    public EditorChangeTimingGroupColorDialog(string id, TimingGroup timingGroup, EditorActionManager editorActionManager)
        : base(LocalizationManager.Get("Screen_Editor_ChangeTimingGroupColor"),
            LocalizationManager.Get("Screen_Editor_ChangeTimingGroupColorMessage"))
    {
        Id = id;
        EditorActionManager = editorActionManager;

        UpdateColor(ColorHelper.ToXnaColor(timingGroup.GetColor()));
    }

    private string Id { get; }

    private EditorActionManager EditorActionManager { get; }

    protected override void OnColorChange(Color newColor)
    {
        EditorActionManager.ChangeTimingGroupColor(Id, new Color(newColor.R, newColor.G, newColor.B));
    }
}
