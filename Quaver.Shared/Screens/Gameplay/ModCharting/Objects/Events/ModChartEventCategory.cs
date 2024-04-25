using System;

[Flags]
public enum ModChartEventCategory
{
    None = 0,
    Custom = 1 << 0,
    /// <summary>
    ///     Note becoming visible/invisible
    /// </summary>
    Note = 1 << 1,
    /// <summary>
    ///     User inputs something
    /// </summary>
    Input = 1 << 2
}