using System;
using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public struct ModChartEventType
{
    public ModChartEventCategory Category;
    public ulong SpecificType;

    public static ModChartEventType From(Enum specificType) =>
        new(ModChartEvents.GetCategory(specificType), Convert.ToUInt64(specificType));

    public ModChartEventType(ModChartEventCategory category, ulong specificType)
    {
        Category = category;
        SpecificType = specificType;
    }
}