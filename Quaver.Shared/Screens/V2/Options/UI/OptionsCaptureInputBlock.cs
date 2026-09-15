using System;
using Quaver.Shared.Input;
using Quaver.Shared.Input.Global;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     Blocks every global keybind while a field is waiting for key presses so the pressed key
    ///     does not also run its action (for example Escape closing the menu).
    /// </summary>
    internal sealed class OptionsCaptureInputBlock : IDisposable
    {
        private GlobalInputScopeToken Token { get; set; }

        internal bool IsHeld => Token != null;

        internal void Acquire() => Token ??= new BlockEverythingToken();

        /// <summary>
        ///     Removes the block once no keys are held, so the key that finished the capture
        ///     does not reach the game either.
        /// </summary>
        internal void ReleaseWhenKeysAreUp()
        {
            if (Token == null || GenericKeyManager.GetPressedKeys().Count != 0)
                return;

            Dispose();
        }

        public void Dispose()
        {
            Token?.Dispose();
            Token = null;
        }

        private sealed class BlockEverythingToken : GlobalInputScopeToken
        {
            public override GlobalInputScope Scope => GlobalInputScope.Options;

            public override GlobalInputHandleResult Handle(GlobalKeybindActions action, bool isKeyPress = true,
                bool isRelease = false) => GlobalInputHandleResult.Consumed;
        }
    }
}
