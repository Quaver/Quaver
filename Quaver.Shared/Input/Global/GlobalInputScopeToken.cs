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
        lock (GlobalInputManager.ScopeTokensLock)
            GlobalInputManager.ScopeTokens.Add(this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (GlobalInputManager.ScopeTokensLock)
            GlobalInputManager.ScopeTokens.Remove(this);
        GC.SuppressFinalize(this);
    }
}