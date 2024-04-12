using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Scripting;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Tween;

[MoonSharpUserData]
public class TweenSetters
{
    [MoonSharpVisible(false)] public ElementAccessShortcut Shortcut;

    public TweenSetters(GameplayScreenView gameplayScreenView)
    {
        Shortcut = new ElementAccessShortcut(gameplayScreenView);
    }

    public TweenPayload.SetterDelegate X(Drawable drawable)
    {
        return v => drawable.X = v;
    }
    public TweenPayload.SetterDelegate Y(Drawable drawable)
    {
        return v => drawable.Y = v;
    }
    public TweenPayload.SetterDelegate Rotation(Sprite sprite)
    {
        return v => sprite.Rotation = v;
    }
    
    public TweenPayload.SetterDelegate Alpha(Sprite sprite)
    {
        return v => sprite.Alpha = v;
    }
    
    public TweenPayload.SetterDelegate SizeX(Drawable drawable)
    {
        return v => drawable.Size = new ScalableVector2(v, drawable.Size.Y.Value);
    }
    public TweenPayload.SetterDelegate SizeY(Drawable drawable)
    {
        return v => drawable.Size = new ScalableVector2(drawable.Size.X.Value, v);
    }
}