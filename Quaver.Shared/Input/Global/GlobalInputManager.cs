using System.Collections.Generic;

namespace Quaver.Shared.Input.Global;

public class GlobalInputManager : InputManager<GlobalKeybindActions>
{
    public static List<GlobalInputScopeToken> ScopeTokens { get; } = [];

    private GlobalInputManager(GlobalInputConfig globalInputConfig) : base(
        globalInputConfig, new GlobalInputHandler())
    {
        base.InputConfig.FillMissingKeys(true);
    }

    public GlobalInputManager() : this(GlobalInputConfig.LoadFromConfig())
    {
    }

    public new GlobalInputConfig InputConfig => (base.InputConfig as GlobalInputConfig)!;
}
