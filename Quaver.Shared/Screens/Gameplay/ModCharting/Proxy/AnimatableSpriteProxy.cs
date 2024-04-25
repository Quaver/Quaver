using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class AnimatableSpriteProxy : SpriteProxy
{
    private AnimatableSprite _sprite;

    public AnimatableSpriteProxy(AnimatableSprite drawable) : base(drawable)
    {
        _sprite = drawable;
    }

    public List<Texture2D> Frames => _sprite.Frames;
    public int TimesLooped => _sprite.TimesLooped;
    public int CurrentFrame => _sprite.CurrentFrame;
    public bool IsLooping => _sprite.IsLooping;
    public int LoopFramesPerSecond => _sprite.LoopFramesPerSecond;

    public Direction Direction
    {
        get => _sprite.Direction;
        set => _sprite.Direction = value;
    }

    public int TimesToLoop => _sprite.TimesToLoop;
    public void To(int i) => _sprite.ChangeTo(i);
    public void Next() => _sprite.ChangeToNext();
    public void Previous() => _sprite.ChangeToPrevious();

    public void Loop(Direction direction, int fps, int timesToLoop = 0) =>
        _sprite.StartLoop(direction, fps, timesToLoop);

    public void EndLoop() => _sprite.StopLoop();
    
}