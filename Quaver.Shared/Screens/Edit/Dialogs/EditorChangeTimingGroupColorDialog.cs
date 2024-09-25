using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;

namespace Quaver.Shared.Screens.Edit.Dialogs;

public class EditorChangeTimingGroupColorDialog : ColorDialog
{
    public EditorChangeTimingGroupColorDialog(TimingGroup timingGroup, EditorActionManager editorActionManager)
        : base("CHANGE TIMING GROUP COLOR",
            "Enter a new RGB color for your timing group...")
    {
        TimingGroup = timingGroup;
        EditorActionManager = editorActionManager;

        UpdateColor(ColorHelper.ToXnaColor(TimingGroup.GetColor()));
    }

    private TimingGroup TimingGroup { get; }

    private EditorActionManager EditorActionManager { get; }

    protected override void OnColorChange(Color newColor)
    {
        EditorActionManager.ChangeTimingGroupColor(TimingGroup, new Color(newColor.R, newColor.G, newColor.B));
    }
}