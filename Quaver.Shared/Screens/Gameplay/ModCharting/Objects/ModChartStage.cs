using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Assets;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartStage
{
    [MoonSharpVisible(false)] public readonly ElementAccessShortcut Shortcut;

    public ModChartStage(ElementAccessShortcut shortcut)
    {
        Shortcut = shortcut;
    }

    public Sprite Receptor(int lane) => Shortcut.GameplayPlayfieldKeysStage.Receptors[lane - 1];
    public Sprite BgMask => Shortcut.GameplayPlayfieldKeysStage.BgMask;
    public Sprite Background => Shortcut.GameplayScreenView.Background;
    public Container ForegroundContainer => Shortcut.GameplayPlayfieldKeys.ForegroundContainer;
    public Container PlayfieldContainer => Shortcut.GameplayPlayfieldKeys.Container;


    [MoonSharpHidden]
    public string GetFullPath(string path)
    {
        return Path.Combine($"{Path.GetDirectoryName(Shortcut.GameplayScreen.Map.GetBackgroundPath())}", path);
    }

    public Texture2D LoadTexture(string relativePath)
    {
        var path = GetFullPath(relativePath);
        return AssetLoader.LoadTexture2DFromFile(path);
    }

    public Sprite CreateSprite(Texture2D texture2D)
    {
        return new Sprite
        {
            Image = texture2D,
            Size = new ScalableVector2(texture2D.Width, texture2D.Height)
        };
    }

    public Sprite CreateSprite(string path) => CreateSprite(LoadTexture(path));

    public SpriteTextPlus CreateText(string fontName, string content, int size)
    {
        WobbleFontStore fontStore;
        if (FontManager.WobbleFonts.ContainsKey(fontName))
            fontStore = FontManager.GetWobbleFont(fontName);
        else
        {
            var fullPath = GetFullPath(fontName);
            fontStore = new WobbleFontStore(20, File.ReadAllBytes(fullPath));
        }
        return new SpriteTextPlus(fontStore, content, size);
    }
    
    public SpriteTextPlus CreateText(string content, int size)
    {
        return CreateText(Fonts.LatoRegular, content, size);
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


    /// <summary>
    ///     Width of lane (receptor alone)
    /// </summary>
    /// <returns></returns>
    public float LaneSize => Shortcut.GameplayPlayfieldKeys.LaneSize;

    /// <summary>
    ///     Padding of receptor
    /// </summary>
    /// <returns></returns>
    public float ReceptorPadding => Shortcut.GameplayPlayfieldKeys.ReceptorPadding;

    /// <summary>
    ///     Separation between lanes
    /// </summary>
    /// <returns></returns>
    public float LaneSeparationWidth => LaneSize + ReceptorPadding;

    public float HitObjectFallRotation(int lane) => Shortcut.GameplayPlayfieldKeys.HitObjectFallRotation[lane - 1];

    public void HitObjectFallRotation(int lane, float rotationRad) =>
        Shortcut.GameplayPlayfieldKeys.HitObjectFallRotation[lane - 1] = rotationRad;

    /// <summary>
    ///     Positions of each receptor
    /// </summary>
    /// <returns>Scalable vector (x, y, scale_x, scale_y) for each receptor</returns>
    public ScalableVector2[] GetReceptorPositions()
    {
        var positions = new ScalableVector2[Shortcut.GameplayScreen.Map.GetKeyCount()];
        for (var i = 0; i < Shortcut.GameplayScreen.Map.GetKeyCount(); i++)
        {
            positions[i] = Shortcut.GameplayPlayfieldKeysStage.Receptors[i].Position;
        }

        return positions;
    }

    /// <summary>
    ///     Sets the position of a receptor of a particular lane
    /// </summary>
    /// <param name="lane"></param>
    /// <param name="pos"></param>
    public void SetReceptorPosition(int lane, ScalableVector2 pos)
    {
        Shortcut.GameplayPlayfieldKeysStage.Receptors[lane - 1].Position = pos;
    }
}