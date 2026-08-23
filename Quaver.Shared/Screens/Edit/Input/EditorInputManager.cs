using Quaver.Shared.Config;
using Quaver.Shared.Input;

namespace Quaver.Shared.Screens.Edit.Input
{
    public class EditorInputManager : InputManager<EditorKeybindActions>
    {
        private EditorInputManager(EditScreen screen, EditorInputConfig editorInputConfig) : base(
            editorInputConfig, new EditorInputHandler(screen, editorInputConfig))
        {
            base.InputConfig.FillMissingKeys(true);
            ConfigManager.InvertEditorScrolling.TriggerChange();
            ConfigManager.EditorInvertBeatSnapScroll.TriggerChange();
        }
        public EditorInputManager(EditScreen screen) : this(screen, EditorInputConfig.LoadFromConfig())
        {
        }

        public new EditorInputConfig InputConfig => (base.InputConfig as EditorInputConfig)!;
    }
}