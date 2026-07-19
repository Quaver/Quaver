namespace Quaver.Shared.Input.Global;

public class GlobalInputManager : InputManager<GlobalKeybindActions>
{
    private GlobalInputManager(GlobalInputConfig globalInputConfig) : base(
        globalInputConfig, new GlobalInputHandler(globalInputConfig))
    {
        base.InputConfig.FillMissingKeys(true);
    }

    public GlobalInputManager() : this(GlobalInputConfig.LoadFromConfig())
    {
    }

    public new GlobalInputConfig InputConfig => (base.InputConfig as GlobalInputConfig)!;
}