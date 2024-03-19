using System;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

[MoonSharpUserData]
public class TweenSetters
{
    [MoonSharpVisible(false)] public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)] public StoryboardScript Script { get; set; }

    [MoonSharpVisible(false)] public GameplayScreen GameplayScreen => GameplayScreenView.Screen;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeys GameplayPlayfieldKeys => (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage => GameplayPlayfieldKeys.Stage;


    public TweenPayload.SetterDelegate ReceptorX(int lane)
    {
        return v => GameplayPlayfieldKeysStage.Receptors[lane - 1].X = v;
    }
    public TweenPayload.SetterDelegate ReceptorY(int lane)
    {
        return v => GameplayPlayfieldKeysStage.Receptors[lane - 1].Y = v;
    }
    
}