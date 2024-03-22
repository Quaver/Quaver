using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.StateMachine;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class LuaStoryboardStateMachine
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut { get; }

    public LuaStoryboardStateMachine(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    public StoryboardStateMachine CreateStateMachine()
    {
        var machine = new StoryboardStateMachine();
        Shortcut.GameplayScreenView.StoryboardStateMachines.Add(machine);
        return machine;
    }

    public void RemoveStateMachine(StoryboardStateMachine machine)
    {
        Shortcut.GameplayScreenView.StoryboardStateMachines.Remove(machine);
    }
}