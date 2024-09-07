using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using Quaver.API.Enums;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Bubble;

public class HitBubbles : Container
{
    private readonly Deque<Sprite> _bubbles = new();
    private int _maxBubbleCount;
    private readonly HitBubblesType _hitBubblesType;
    private AnimatableSprite _background;

    private Texture2D GetTexture(Judgement judgement)
    {
        return SkinManager.Skin.HitBubbles[judgement];
    }

    public HitBubbles(float hitBubblesScale, HitBubblesType hitBubblesType)
    {
        _hitBubblesType = hitBubblesType;

        _background = new AnimatableSprite(SkinManager.Skin.HitBubblesBackground);
        Size = new ScalableVector2(_background.Frames.First().Width * hitBubblesScale,
            _background.Frames.First().Height * hitBubblesScale);
        _background.Size = Size;
        _background.Parent = this;

        // Start animation
        _background.StartLoop(Direction.Forward, 60);
    }

    protected override void OnRectangleRecalculated()
    {
        base.OnRectangleRecalculated();
        var texture = GetTexture(Judgement.Marv);
        var size = GetTextureSize(texture);
        _maxBubbleCount = _hitBubblesType switch
        {
            HitBubblesType.FallLeft or HitBubblesType.FallRight => (int)(Size.X.Value / size.X),
            HitBubblesType.FallUp or HitBubblesType.FallDown => (int)(Size.Y.Value / size.Y),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Vector2 GetTextureSize(Texture2D texture)
    {
        var scale = (float)SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].HitBubbleScale / texture.Height;
        return new Vector2(texture.Width, texture.Height) * scale;
    }

    public void AddJudgement(Judgement judgement)
    {
        if (_maxBubbleCount <= 0)
            return;

        var texture = GetTexture(judgement);
        while (_bubbles.Count > _maxBubbleCount)
        {
            _bubbles.RemoveFromFront();
        }

        Sprite sprite;
        if (_bubbles.Count == _maxBubbleCount)
        {
            _bubbles.RemoveFromFront(out sprite);
        }
        else
        {
            sprite = new Sprite()
            {
                Parent = this,
                Alignment = _hitBubblesType switch
                {
                    HitBubblesType.FallLeft => Alignment.MidLeft,
                    HitBubblesType.FallRight => Alignment.MidRight,
                    HitBubblesType.FallUp => Alignment.TopCenter,
                    HitBubblesType.FallDown => Alignment.BotCenter,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }

        var size = GetTextureSize(texture);
        sprite.Size = new ScalableVector2(size.X, size.Y);
        sprite.Image = texture;

        sprite.Position = _bubbles.GetBack(out var back)
            ? _hitBubblesType switch
            {
                HitBubblesType.FallLeft => new ScalableVector2(back.RelativeRectangle.Right, 0),
                HitBubblesType.FallRight => new ScalableVector2(back.RelativeRectangle.Left - back.Width, 0),
                HitBubblesType.FallUp => new ScalableVector2(0, back.RelativeRectangle.Bottom),
                HitBubblesType.FallDown => new ScalableVector2(0, back.RelativeRectangle.Top - back.Height),
                _ => throw new ArgumentOutOfRangeException()
            }
            : new ScalableVector2(0, 0);

        _bubbles.AddToBack(sprite);

        if (!_bubbles.GetFront(out var front))
            return;

        switch (_hitBubblesType)
        {
            case HitBubblesType.FallLeft:
            case HitBubblesType.FallRight:
                var moveOffsetX = -front.X;

                foreach (var bubble in _bubbles)
                {
                    bubble.MoveToX(bubble.X + moveOffsetX, Easing.InCirc, 50);
                }

                break;
            case HitBubblesType.FallUp:
            case HitBubblesType.FallDown:
                var moveOffsetY = -front.Y;

                foreach (var bubble in _bubbles)
                {
                    bubble.MoveToY((int)(bubble.Y + moveOffsetY), Easing.InCirc, 50);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}