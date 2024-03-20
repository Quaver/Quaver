using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardSprites
{
    
    [MoonSharpVisible(false)] public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)] public StoryboardScript Script { get; set; }
    [MoonSharpVisible(false)] public GameplayScreen GameplayScreen => GameplayScreenView.Screen;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeys GameplayPlayfieldKeys => (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage => GameplayPlayfieldKeys.Stage;

    public Sprite Receptor(int lane) => GameplayPlayfieldKeysStage.Receptors[lane - 1];
    public Sprite BgMask => GameplayPlayfieldKeysStage.BgMask;
    public Sprite Background => GameplayScreenView.Background;
}