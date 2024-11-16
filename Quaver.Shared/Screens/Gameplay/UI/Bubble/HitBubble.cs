using System;
using Microsoft.Xna.Framework;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Bubble;

public class HitBubble : Sprite
{
    private float _targetPosition;

    public HitBubble(HitBubblesType hitBubblesType)
    {
        HitBubblesType = hitBubblesType;
    }

    private const int AnimationLengthMs = 50;
    public HitBubblesType HitBubblesType { get; private set; }
    
    public float TargetPosition
    {
        get => _targetPosition;
        set => SetTargetPosition(value, true);
    }

    public void SetTargetPosition(float value, bool animation = true)
    {
        _targetPosition = value;
        Animations.RemoveAll(a => a.Properties is AnimationProperty.X or AnimationProperty.Y);
        switch (HitBubblesType)
        {
            case HitBubblesType.FallLeft:
            case HitBubblesType.FallRight:
                if (animation)
                    MoveToPosition(new Vector2(_targetPosition, Y), Easing.InCirc, AnimationLengthMs);
                else
                    X = _targetPosition;
                break;
            case HitBubblesType.FallUp:
            case HitBubblesType.FallDown:
                if (animation)
                    MoveToPosition(new Vector2(X, _targetPosition), Easing.InCirc, AnimationLengthMs);
                else
                    Y = _targetPosition;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void FinishAnimation()
    {
        X = Animations.Find(a => a.Properties == AnimationProperty.X)?.End ?? X;
        Y = Animations.Find(a => a.Properties == AnimationProperty.Y)?.End ?? Y;
        Animations.RemoveAll(a => a.Properties is AnimationProperty.X or AnimationProperty.Y);
    }
}