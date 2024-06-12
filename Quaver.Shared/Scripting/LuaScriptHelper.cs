using System;
using MoonSharp.Interpreter;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Logging;

namespace Quaver.Shared.Scripting;

public static class LuaScriptHelper
{
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
            NotificationManager.Show(NotificationLevel.Error, $"Lua script: {e.DecoratedMessage}");
            Logger.Error($"Lua chart script runtime error: {e.DecoratedMessage}", LogType.Runtime);
        }
        catch (SyntaxErrorException e)
        {
            NotificationManager.Show(NotificationLevel.Error, $"Lua script: {e.DecoratedMessage}");
            Logger.Error($"Lua chart script syntax error: {e.DecoratedMessage}", LogType.Runtime);
        }
        catch (Exception e)
        {
            NotificationManager.Show(NotificationLevel.Error, $"Lua script caused an internal error: {e}");
            Logger.Error($"Lua chart script internal error: {e}", LogType.Runtime);
        }

        return default;
    }

    public static DynValue SafeCall(this Closure closure, params object[] args)
    {
        return TryPerform(() => closure.Call(args));
    }
}