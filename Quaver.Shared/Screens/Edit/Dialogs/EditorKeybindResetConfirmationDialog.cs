using Quaver.Shared.Graphics;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public class EditorKeybindResetConfirmationDialog : YesNoDialog
    {
        public EditorKeybindResetConfirmationDialog(EditScreen screen) : base("RESET EDITOR KEYBINDS",
            $"Are you sure you would like to reset all editor keybinds?")
        {
            YesAction += () => { screen.EditorInputManager.InputConfig.ResetConfigFile(); };
        }
    }
}