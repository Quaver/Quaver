using MoonSharp.Interpreter;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartGlobalVariable
{
    [MoonSharpHidden]
    protected ElementAccessShortcut Shortcut { get; set; }

    public ModChartGlobalVariable(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
    }
}