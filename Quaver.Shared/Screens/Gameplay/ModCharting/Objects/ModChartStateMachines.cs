using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartStateMachines
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut { get; }

    public ModChartStateMachines(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    public ModChartStateMachine New()
    {
        var machine = new ModChartStateMachine();
        Shortcut.GameplayScreenView.StoryboardStateMachines.Add(machine);
        return machine;
    }

    public void Delete(ModChartStateMachine machine)
    {
        Shortcut.GameplayScreenView.StoryboardStateMachines.Remove(machine);
    }
    
    public LuaStateMachineState NewState(Closure onInitialize, Closure updater,  Closure onEnable,
        Closure onDisable)
    {
        return new LuaStateMachineState(onInitialize, updater, onEnable, onDisable);
    }
}