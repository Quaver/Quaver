namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

public class ModChartEventInstance
{
    public ModChartEventType Type { get; private set; }
    public object[] Arguments { get; private set; }

    public ModChartEventInstance(ModChartEventType type, object[] arguments)
    {
        Type = type;
        Arguments = arguments;
    }

    public void Dispatch(ModChartEvents modChartEvents)
    {
        modChartEvents.Invoke(Type, Arguments);
    }
}