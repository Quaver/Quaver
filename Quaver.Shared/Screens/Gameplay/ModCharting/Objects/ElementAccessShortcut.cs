using MoonSharp.Interpreter;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

public class ElementAccessShortcut
{
    public GameplayScreenView GameplayScreenView { get; }

    public GameplayScreen GameplayScreen { get; }


    public GameplayPlayfieldKeys GameplayPlayfieldKeys { get; private set; }


    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage { get; private set; }
    
    public ModChartScript ModChartScript { get; private set; }

    public ModChartInternal Internal => ModChartScript.Internal;

    public ModChartEvents ModChartEvents => ModChartScript.ModChartEvents;

    public ElementAccessShortcut(GameplayScreen gameplayScreen, ModChartScript modChartScript)
    {
        GameplayScreenView = (GameplayScreenView)gameplayScreen.View;
        GameplayScreen = gameplayScreen;
        ModChartScript = modChartScript;
        InitOtherMembers();
    }

    public ElementAccessShortcut(GameplayScreenView gameplayScreenView, ModChartScript modChartScript)
    {
        GameplayScreenView = gameplayScreenView;
        GameplayScreen = gameplayScreenView.Screen;
        ModChartScript = modChartScript;
        InitOtherMembers();
    }

    private void InitOtherMembers()
    {
        GameplayPlayfieldKeys = (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;
        GameplayPlayfieldKeysStage = GameplayPlayfieldKeys.Stage;
    }
}