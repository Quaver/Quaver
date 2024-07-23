using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

/// <summary>
///     Special descriptor for vector to allow it to do cool field accessing like vector.xyz = vector.zyx, as in shaders.
///     You can also use multi-indexer or a table to access arbitrary combination of the components of the vector
/// </summary>
public class ModChartVectorDescriptor : StandardUserDataDescriptor
{
    public ModChartVectorDescriptor(Type type, InteropAccessMode accessMode, string friendlyName = null) : base(type,
        accessMode, friendlyName)
    {
    }

    public override DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
    {
        if (obj is ModChartVector vector)
        {
            switch (index.Type)
            {
                case DataType.String when vector.TryParseShorthand(index.String, out var indices):
                    return indices.Length == 1
                        ? DynValue.NewNumber(vector[indices[0]])
                        : DynValue.FromObject(script, vector.GetEntries(indices));
                case DataType.Table:
                    return DynValue.FromObject(script, vector.GetEntries(DynValuesToIndices(index.Table.Values)));
            }
        }

        return base.Index(script, obj, index, isDirectIndexing);
    }

    public override bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
    {
        if (obj is ModChartVector vector)
        {
            switch (index.Type)
            {
                case DataType.String when vector.TryParseShorthand(index.String, out var indices):
                    vector.SetEntries(indices, value);
                    return true;
                case DataType.Table:
                    vector.SetEntries(DynValuesToIndices(index.Table.Values), value);
                    break;
            }
        }

        return base.SetIndex(script, obj, index, value, isDirectIndexing);
    }

    private static int[] DynValuesToIndices(IEnumerable<DynValue> dynValues)
    {
        return dynValues.Select(v => (int)v.Number).ToArray();
    }
}