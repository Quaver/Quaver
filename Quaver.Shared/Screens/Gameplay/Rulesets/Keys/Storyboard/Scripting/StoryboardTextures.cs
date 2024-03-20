using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Assets;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardTextures
{
    [MoonSharpVisible(false)] public GameplayScreenView GameplayScreenView { get; set; }

    [MoonSharpVisible(false)] public StoryboardScript Script { get; set; }
    [MoonSharpVisible(false)] public GameplayScreen GameplayScreen => GameplayScreenView.Screen;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeys GameplayPlayfieldKeys => (GameplayPlayfieldKeys)GameplayScreen.Ruleset.Playfield;

    [MoonSharpVisible(false)]
    public GameplayPlayfieldKeysStage GameplayPlayfieldKeysStage => GameplayPlayfieldKeys.Stage;


    public StoryboardTextures(GameplayScreenView gameplayScreenView, StoryboardScript script)
    {
        GameplayScreenView = gameplayScreenView;
        Script = script;
    }

    [MoonSharpHidden]
    public string GetTexturePath(string path)
    {
        return Path.Combine($"{Path.GetDirectoryName(GameplayScreen.Map.GetBackgroundPath())}", path);
    }

    public Texture2D LoadTexture(string relativePath)
    {
        var path = GetTexturePath(relativePath);
        return AssetLoader.LoadTexture2DFromFile(path);
    }
}