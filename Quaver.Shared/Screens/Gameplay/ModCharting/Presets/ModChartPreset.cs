using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Presets;

[MoonSharpUserData]
public abstract class ModChartPreset
{
    protected readonly ElementAccessShortcut Shortcut;
    protected ModChartTimeline Timeline => Shortcut.ModChartScript.Timeline;
    protected ModChartStage Stage => Shortcut.ModChartScript.ModChartStage;

    protected EasingDelegate EasingDelegate { get; set; }

    protected ModChartPreset(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
    }

    public virtual int DefaultDuration { get; } = 1000;

    protected abstract void PlacePreset(int startTime, int endTime);

    public void Place(int startTime, int endTime = int.MinValue)
    {
        if (endTime == int.MinValue) endTime = startTime + DefaultDuration;
        PlacePreset(startTime, endTime);
    }

    public ModChartPreset Ease(EasingDelegate easingDelegate)
    {
        EasingDelegate = easingDelegate;
        return this;
    }
}