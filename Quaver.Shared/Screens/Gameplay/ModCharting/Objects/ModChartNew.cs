using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Properties;
using Quaver.Shared.Screens.Gameplay.ModCharting.Presets;
using Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartNew : ModChartGlobalVariable
{
    public ModChartNew(ElementAccessShortcut shortcut) : base(shortcut)
    {
    }

    #region Property

    public ModChartGeneralProperty<DynValue> Property(Closure getter, Closure setter) => new(
        () => getter.SafeCall(),
        v => setter.SafeCall(v));

    public ModChartGeneralProperty<DynValue> Property(Closure getter) => new(
        () => getter.SafeCall());

    public ModChartPropertyFloat PropertyFloat(Closure getter, Closure setter) => new(
        () => getter.SafeCall().ToObject<float>(),
        v => setter.SafeCall(v));

    public ModChartPropertyFloat PropertyFloat(Closure getter) => new(
        () => getter.SafeCall().ToObject<float>());

    public ModChartPropertyVector2 PropertyVector2(Closure getter, Closure setter) => new(
        () => getter.SafeCall().ToObject<Vector2>(),
        v => setter.SafeCall(v));

    public ModChartPropertyVector2 PropertyVector2(Closure getter) => new(
        () => getter.SafeCall().ToObject<Vector2>());

    public ModChartPropertyXnaVector2 PropertyXnaVector2(Closure getter, Closure setter) => new(
        () => getter.SafeCall().ToObject<XnaVector2>(),
        v => setter.SafeCall(v));

    public ModChartPropertyXnaVector2 PropertyXnaVector2(Closure getter) => new(
        () => getter.SafeCall().ToObject<XnaVector2>());

    public ModChartPropertyVector3 PropertyVector3(Closure getter, Closure setter) => new(
        () => getter.SafeCall().ToObject<Vector3>(),
        v => setter.SafeCall(v));

    public ModChartPropertyVector3 PropertyVector3(Closure getter) => new(
        () => getter.SafeCall().ToObject<Vector3>());

    public ModChartPropertyVector4 PropertyVector4(Closure getter, Closure setter) => new(
        () => getter.SafeCall().ToObject<Vector4>(),
        v => setter.SafeCall(v));

    public ModChartPropertyVector4 PropertyVector4(Closure getter) => new(
        () => getter.SafeCall().ToObject<Vector4>());

    public ModChartPropertyColor PropertyColor(Closure getter, Closure setter) => new(
        () => getter.SafeCall().ToObject<Color>(),
        v => setter.SafeCall(v));

    public ModChartPropertyColor PropertyColor(Closure getter) => new(
        () => getter.SafeCall().ToObject<Color>());


    #endregion

    #region Drawables

    [MoonSharpHidden]
    public string GetFullPath(string path)
    {
        return Path.Combine($"{Path.GetDirectoryName(Shortcut.GameplayScreen.Map.GetBackgroundPath())}", path);
    }

    public Texture2D Texture(string relativePath)
    {
        var path = GetFullPath(relativePath);
        return AssetLoader.LoadTexture2DFromFile(path);
    }

    public Sprite Sprite(Texture2D texture2D)
    {
        return new Sprite
        {
            Image = texture2D,
            Size = new ScalableVector2(texture2D.Width, texture2D.Height)
        };
    }

    public Sprite Sprite(string path) => Sprite(Texture(path));

    public SpriteTextPlus Text(string fontName, string content, int size)
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

    public SpriteTextPlus Text(string content, int size)
    {
        return Text(Fonts.LatoRegular, content, size);
    }

    public AnimatableSprite AnimatableSprite(Texture2D spritesheet, int rows, int columns)
    {
        return new AnimatableSprite(spritesheet, rows, columns);
    }

    public AnimatableSprite AnimatableSprite(string spritesheetPath, int rows, int columns) =>
        AnimatableSprite(Texture(spritesheetPath), rows, columns);

    public AnimatableSprite AnimatableSprite(List<Texture2D> frames)
    {
        return new AnimatableSprite(frames);
    }

    public AnimatableSprite AnimatableSprite(List<string> framePaths)
    {
        return AnimatableSprite(framePaths.Select(Texture).ToList());
    }

    #endregion

    #region State Machine

    public OrthogonalStateMachine OrthogonalStateMachine(string name = "", StateMachineState parent = default) =>
        new(Shortcut.ModChartScript, name, parent);

    public StateMachine.StateMachine StateMachine(string name = "", StateMachineState entryState = null,
        StateMachineState parent = default) => new(Shortcut.ModChartScript, entryState, name, parent);

    public CustomStateMachineState State(string name = "", Action<CustomStateMachineState> updater = null,
        Action<CustomStateMachineState> onEnable = null, Action<CustomStateMachineState> onDisable = null,
        StateMachineState parent = default)
    {
        return new CustomStateMachineState(Shortcut.ModChartScript, updater, onEnable, onDisable, name, parent);
    }

    #endregion

    #region Vectors

    public Vector2 Vector2(float x, float y) => new(x, y);
    public XnaVector2 XnaVector2(float x, float y) => new(x, y);
    public Vector3 Vector3(float x, float y, float z) => new(x, y, z);
    public Vector4 Vector4(float x, float y, float z, float w) => new(x, y, z, w);
    public Color Color(float r, float g, float b, float a) => new(r, g, b, a);
    public ScalableVector2 ScalableVector2(float x, float y) => new(x, y);
    public ScalableVector2 ScalableVector2(float x, float y, float scaleX, float scaleY) => new(x, y, scaleX, scaleY);

    #endregion

    #region Shader

    public SpriteBatchOptions SpriteBatchOptions() => new();

    public SpriteBatchOptions SpriteBatchOptions(Shader shader) => new()
    {
        Shader = shader
    };

    public Shader MultiEffectShader() =>
        new(GameBase.Game.Resources.Get("Quaver.Resources/Shaders/multi_effect_shader.mgfxo"),
            new Dictionary<string, object>());

    #endregion

    #region Layer

    public Layer Layer(string name) => Shortcut.ModChartScript.ModChartLayers.NewLayer(name);

    #endregion

    #region Presets

    public SwapLanePreset SwapLanePreset(int lane1, int lane2) => new(Shortcut, lane1, lane2);

    #endregion
}