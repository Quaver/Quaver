// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

public static class EventHelper
{
    public const int SpecificTypeWidth = 32;
    public const ulong EventSpecificTypeMask = (1UL << SpecificTypeWidth) - 1;

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

    public static IEnumerable<ModChartEventType> GetCategories(this ModChartEventType eventType)
    {
        for (var i = SpecificTypeWidth; i < 64; i++)
        {
            var testCategory = (ModChartEventType)(1UL << i);
            if (eventType.HasFlag(testCategory)) yield return testCategory;
        }
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