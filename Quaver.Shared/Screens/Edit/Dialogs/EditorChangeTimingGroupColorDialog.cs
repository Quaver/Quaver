using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Actions;

namespace Quaver.Shared.Screens.Edit.Dialogs;

public class EditorChangeTimingGroupColorDialog : ColorDialog
{
    public EditorChangeTimingGroupColorDialog(string id, TimingGroup timingGroup, EditorActionManager editorActionManager)
        : base("CHANGE TIMING GROUP COLOR",
            "Enter a new RGB color for your timing group...")
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