using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;

[MoonSharpUserData]
public class StoryboardSprites
{
    [MoonSharpVisible(false)] public readonly ElementAccessShortcut Shortcut;

    public StoryboardSprites(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    public Sprite Receptor(int lane) => Shortcut.GameplayPlayfieldKeysStage.Receptors[lane - 1];
    public Sprite BgMask => Shortcut.GameplayPlayfieldKeysStage.BgMask;
    public Sprite Background => Shortcut.GameplayScreenView.Background;
    public Container ForegroundContainer => Shortcut.GameplayPlayfieldKeys.ForegroundContainer;
    
    
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
    
    public Sprite CreateSprite(Texture2D texture2D)
    {
        return new Sprite
        {
            Image = texture2D
        };
    }

    public Sprite CreateSprite(string path) => CreateSprite(LoadTexture(path));

    public SpriteTextPlus CreateText(string fontName, string content, int size)
    {
        return new SpriteTextPlus(FontManager.GetWobbleFont(fontName), content, size);
    }

    public AnimatableSprite CreateAnimatableSprite(Texture2D spritesheet, int rows, int columns)
    {
        return new AnimatableSprite(spritesheet, rows, columns);
    }

    public AnimatableSprite CreateAnimatableSprite(string spritesheetPath, int rows, int columns) =>
        CreateAnimatableSprite(LoadTexture(spritesheetPath), rows, columns);

    public AnimatableSprite CreateAnimatableSprite(List<Texture2D> frames)
    {
        return new AnimatableSprite(frames);
    }

    public AnimatableSprite CreateAnimatableSprite(List<string> framePaths)
    {
        return CreateAnimatableSprite(framePaths.Select(LoadTexture).ToList());
    }
}