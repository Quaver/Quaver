using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

public class ElementAccessShortcut
{
    public GameplayScreenView GameplayScreenView { get; }

    public GameplayScreen GameplayScreen { get; }


    public GameplayPlayfieldKeys GameplayPlayfieldKeys { get; private set; }


    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage { get; private set; }

    public ElementAccessShortcut(GameplayScreen gameplayScreen)
    {
        GameplayScreenView = (GameplayScreenView)gameplayScreen.View;
        GameplayScreen = gameplayScreen;
        InitOtherMembers();
    }

    public ElementAccessShortcut(GameplayScreenView gameplayScreenView)
    {
        GameplayScreenView = gameplayScreenView;
        GameplayScreen = gameplayScreenView.Screen;
        InitOtherMembers();
    }

    private void InitOtherMembers()
    {
        GameplayPlayfieldKeys = (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;
        GameplayPlayfieldKeysStage = GameplayPlayfieldKeys.Stage;
    }
}