using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;

namespace Quaver.Shared.Screens.Edit.Plugins;

public class EditorPluginStorageValue
{
    public bool IsTable;
    public object Value;
    private readonly Script _script;

    public EditorPluginStorageValue(Script script)
    {
        _script = script;
    }

    public void Set(DynValue value)
    {
        switch (value.Type)
        {
            case DataType.Nil:
            case DataType.Boolean:
            case DataType.Number:
            case DataType.String:
                IsTable = false;
                Value = value.ToObject();
                break;
            case DataType.Table:
                IsTable = true;
                Value = value.Table.TableToJson();
                break;
            default:
                throw new ScriptRuntimeException($"Invalid data type for storage: {value.Type}");
        }
    }

    public DynValue Get()
    {
        return DynValue.FromObject(_script,
            IsTable
                ? JsonTableConverter.JsonToTable((string)Value)
                : Value);
    }
}