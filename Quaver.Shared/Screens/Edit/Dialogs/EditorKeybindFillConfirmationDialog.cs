using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorKeybindFillConfirmationDialog : YesNoDialog
    {
        public EditorKeybindFillConfirmationDialog(EditScreen screen, bool fillWithDefaultBinds) : base("FILL MISSING KEYBIND ACTIONS",
            $"Are you sure you would like to fill missing\neditor keybind actions with {(fillWithDefaultBinds ? "default" : "unbound")} keybinds?")
        {
            YesAction += () => { screen.EditorInputManager.InputConfig.FillMissingKeys(fillWithDefaultBinds); };
        }
    }
}