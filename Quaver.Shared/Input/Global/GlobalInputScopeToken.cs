using System;

namespace Quaver.Shared.Input.Global;

public abstract class GlobalInputScopeToken : IDisposable
{
    public abstract GlobalInputScope Scope { get; }

    public abstract GlobalInputHandleResult Handle(GlobalKeybindActions action,
        bool isKeyPress = true,
        bool isRelease = false);

    protected GlobalInputScopeToken()
    {
        GlobalInputManager.ScopeTokens.Add(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GlobalInputManager.ScopeTokens.Remove(this);
        GC.SuppressFinalize(this);
    }
}