using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting;

public static class ModChartScriptHelper
{
    /// <summary>
    ///     Maximum error allowed to be thrown in calling a lua function before the lua should be halted
    /// </summary>
    public const int MaxErrorCount = 100;

    /// <summary>
    ///     Maximum number of times a single call of a lua function can execute over <see cref="MaxInstructionsPerCall"/>
    ///     instructions before the lua should be halted
    /// </summary>
    public const int MaxTleCount = 0;

    /// <summary>
    ///     Maximum instructions that can be executed per lua function call
    /// </summary>
    public const int MaxInstructionsPerCall = 1000;

    /// <summary>
    ///     Number of errors thrown in a safe lua call
    /// </summary>
    public static int ErrorCount { get; private set; }

    /// <summary>
    ///     Number of times a lua call has exceeded time limit
    /// </summary>
    public static int TimeLimitExceedCount { get; private set; }

    /// <summary>
    ///     Set <see cref="ErrorCount"/> and <see cref="TimeLimitExceedCount"/> to 0
    /// </summary>
    public static void ResetCounters()
    {
        ErrorCount = TimeLimitExceedCount = 0;
    }

    /// <summary>
    ///     Either <see cref="ErrorCount"/> and <see cref="TimeLimitExceedCount"/> has exceeded limit
    /// </summary>
    public static bool CounterExceeded => ErrorCount > MaxErrorCount || TimeLimitExceedCount > MaxTleCount;

    public static void TryPerform(Action action)
    {
        TryPerform(() =>
        {
            action();
            return true;
        });
    }

    public static T TryPerform<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (ScriptRuntimeException e)
        {
            NotificationManager.Show(NotificationLevel.Error, $"Mod script: {e.DecoratedMessage}");
            Logger.Error($"Mod chart script runtime error: {e.DecoratedMessage}", LogType.Runtime);
        }
        catch (SyntaxErrorException e)
        {
            NotificationManager.Show(NotificationLevel.Error, $"Mod script: {e.DecoratedMessage}");
            Logger.Error($"Mod chart script syntax error: {e.DecoratedMessage}", LogType.Runtime);
        }
        catch (Exception e)
        {
            NotificationManager.Show(NotificationLevel.Error, $"Mod script caused an internal error: {e}");
            Logger.Error($"Mod chart script internal error: {e}", LogType.Runtime);
        }

        ErrorCount++;

        return default;
    }

    public static DynValue SafeCall(this Coroutine coroutine, params object[] args)
    {
        return TryPerform(() =>
        {
            var result = coroutine.Resume(args);
            if (result.Type != DataType.YieldRequest) return result;
            TimeLimitExceedCount++;
            return null;
        });
    }

    public static DynValue SafeCall(this Closure closure, params object[] args)
    {
        return TryPerform(() => closure.Call(args));
    }
}