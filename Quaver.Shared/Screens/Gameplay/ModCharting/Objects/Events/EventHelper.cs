// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

public static class EventHelper
{
    public const ulong EventSpecificTypeMask = 0xFFFF_FFFFUL;

    public static int GetSpecificType(this ModChartEventType eventType)
    {
        return (int)((ulong)eventType & EventSpecificTypeMask);
    }

    public static ModChartEventType WithSpecificType(this ModChartEventType eventType, int specificType)
    {
        return eventType | (ModChartEventType)specificType;
    }

    public static ModChartEventType GetCategory(this ModChartEventType eventType)
    {
        return eventType & (ModChartEventType)~ EventSpecificTypeMask;
    }

    public static bool HasFlag(this ModChartEventType eventType, ModChartEventType flag)
    {
        return (eventType & flag.GetCategory()) != ModChartEventType.None;
    }

    public static void Deconstruct(this ModChartEventType eventType, out ModChartEventType category,
        out int specificType)
    {
        category = GetCategory(eventType);
        specificType = (int)(eventType ^ category);
    }
}