namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager
    {
        public EditorInputConfig Keybinds { get; }

        public EditorInputManager()
        {
            Keybinds = EditorInputConfig.LoadFromConfig();
        }
    }
}