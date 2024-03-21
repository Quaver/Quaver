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
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut;


    public StoryboardTextures(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    [MoonSharpHidden]
    public string GetTexturePath(string path)
    {
        return Path.Combine($"{Path.GetDirectoryName(Shortcut.GameplayScreen.Map.GetBackgroundPath())}", path);
    }

    public Texture2D LoadTexture(string relativePath)
    {
        var path = GetTexturePath(relativePath);
        return AssetLoader.LoadTexture2DFromFile(path);
    }
}