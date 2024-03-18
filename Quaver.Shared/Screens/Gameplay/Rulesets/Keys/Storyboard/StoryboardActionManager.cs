using MoonSharp.Interpreter;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class StoryboardActionManager
{
    public void Debug(string str)
    {
        Logger.Debug(str, LogType.Runtime);
    }
}