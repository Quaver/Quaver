namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

public class ModChartInternal : ModChartGlobalVariable
{
    public readonly int KeyCount;
    public float[] OriginalLaneX;
    public ModChartInternal(ElementAccessShortcut shortcut) : base(shortcut)
    {
        KeyCount = shortcut.GameplayScreen.Map.GetKeyCount();
        
    }
}