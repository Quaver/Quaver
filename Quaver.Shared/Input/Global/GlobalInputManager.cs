using System.Collections.Generic;

namespace Quaver.Shared.Input.Global;

public class GlobalInputManager : InputManager<GlobalKeybindActions>
{
    public static List<GlobalInputScopeToken> ScopeTokens { get; } = [];

    /// <summary>
    ///     Screens can be built on a background thread while still loading so this lock keeps
    ///     <see cref="ScopeTokens"/> safe to use from both that thread and the main thread's input handling.
    /// </summary>
    internal static readonly object ScopeTokensLock = new();

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
