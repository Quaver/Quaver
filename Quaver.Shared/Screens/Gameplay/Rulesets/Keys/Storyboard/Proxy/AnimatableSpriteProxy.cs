using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Proxy;

public class AnimatableSpriteProxy : SpriteProxy
{
    private AnimatableSprite _sprite;

    public AnimatableSpriteProxy(AnimatableSprite container) : base(container)
    {
        _sprite = container;
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
    public void ChangeTo(int i) => _sprite.ChangeTo(i);
    public void ChangeToNext() => _sprite.ChangeToNext();
    public void ChangeToPrevious() => _sprite.ChangeToPrevious();

    public void StartLoop(Direction direction, int fps, int timesToLoop = 0) =>
        _sprite.StartLoop(Direction, fps, timesToLoop);

    public void StopLoop() => _sprite.StopLoop();
    
}