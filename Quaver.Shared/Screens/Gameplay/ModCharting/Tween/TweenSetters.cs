using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Tween;

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
    
    public TweenPayload.SetterDelegate Width(Drawable drawable)
    {
        return v => drawable.Width = v;
    }

    public TweenPayload.SetterDelegate Height(Drawable drawable)
    {
        return v => drawable.Height = v;
    }
}