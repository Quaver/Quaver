using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization;
using MoonSharp.Interpreter.Serialization.Json;

namespace Quaver.Shared.Screens.Edit.Plugins;

[MoonSharpUserData]
public class EditorPluginStorage
{
    public EditorPluginStorage(Script script, Dictionary<string, EditorPluginStorageValue> storageValues)
    {
        _storageValues = storageValues;
        _script = script;
    }

    private readonly Dictionary<string, EditorPluginStorageValue> _storageValues;

    private readonly Script _script;

    public void Set(string name, DynValue value)
    {
        if (!_storageValues.TryGetValue(name, out var storageValue))
        {
            storageValue = new EditorPluginStorageValue(_script);
            _storageValues.Add(name, storageValue);
        }
        storageValue.Set(value);
    }

    public DynValue Get(string name)
    {
        return _storageValues.GetValueOrDefault(name)?.Get() ?? DynValue.Nil;
    }
}