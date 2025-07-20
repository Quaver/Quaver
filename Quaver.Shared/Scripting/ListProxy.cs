using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Scripting;

public class ListProxy<T>
{
    private readonly List<T> _list;

    [MoonSharpHidden]
    public ListProxy(List<T> list)
    {
        _list = list;
    }

    [MoonSharpUserDataMetamethod("__newindex")]
    public static DynValue ListNewIndex(List<T> list, DynValue arg)
    {
        if (arg is not { Type: DataType.Number, Number: >= 1 and var x } || x % 1 is not 0)
            throw new ScriptRuntimeException("Index must be a positive integer: " + arg);

        var index = (int)x - 1;
        var obj = arg.ToObject<T>();
        if (index >= 0 && index < list.Count)
            list[index] = obj;
        else if (index == list.Count)
            list.Add(obj);
        else
            throw new ScriptRuntimeException("Index out of bounds: " + x);
        return DynValue.Nil;
    }

    [MoonSharpUserDataMetamethod("__index")]
    public static DynValue ListIndex(List<T> list, DynValue arg) =>
        DynValue.FromObject(
            null,
            arg is { Type: DataType.Number, Number: >= 1 and var x } &&
            x % 1 is 0 &&
            x <= list.Count
                ? list[(int)x - 1]
                : DynValue.Nil
        );

    [MoonSharpUserDataMetamethod("__ipairs")]
    public static DynValue IPairs(List<T> list)
    {
        return DynValue.NewCallback(ListNext);

        DynValue ListNext(ScriptExecutionContext context, CallbackArguments args) =>
            (int)args[1].Number + 1 is var x && x <= list.Count
                ? DynValue.NewTuple(DynValue.NewNumber(x), DynValue.FromObject(null, list[x - 1]))
                : DynValue.Nil;
    }

    [MoonSharpUserDataMetamethod("__pairs")]
    public static DynValue Pairs(List<T> list) => IPairs(list);

    [MoonSharpUserDataMetamethod("__len")]
    public static DynValue Len(List<T> list) => DynValue.NewNumber(list.Count);
}