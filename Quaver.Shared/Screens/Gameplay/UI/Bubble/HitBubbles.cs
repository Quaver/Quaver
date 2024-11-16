using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collections;
using Quaver.API.Enums;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Bubble;

public class HitBubbles : Container
{
    private readonly Deque<HitBubble> _bubbles = new();
    private int _maxBubbleCount;
    private readonly HitBubblesType _hitBubblesType;
    private AnimatableSprite _background;
    private readonly float _hitBubblePadding;
    private readonly float _borderPadding;
    private GameplayScreen Screen { get; }

    private bool HideHitBubbles =>
        (Screen?.IsCalibratingOffset ?? false)
        || SkinKeys.HitBubblesScale == 0
        || !ConfigManager.DisplayHitBubbles.Value;

    private HitBubbleRecordedJudgement RecordedJudgements =>
        ModManager.IsActivated(ModIdentifier.Autoplay) || (Screen.IsPlayTesting && Screen.InReplayMode)
            ? HitBubbleRecordedJudgement.All
            : SkinKeys.HitBubblesRecordedJudgements;

    private static SkinKeys SkinKeys => SkinManager.Skin.Keys[MapManager.Selected.Value.Mode];

    private Texture2D GetTexture()
    {
        return SkinManager.Skin.HitBubbles;
    }

    public HitBubbles(float hitBubblesScale, HitBubblesType hitBubblesType, GameplayScreen screen)
    {
        _hitBubblesType = hitBubblesType;
        Screen = screen;

        if (HideHitBubbles)
            return;

        _hitBubblePadding = SkinKeys.HitBubblePadding * hitBubblesScale;
        _borderPadding = SkinKeys.HitBubbleBorderPadding * hitBubblesScale;

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
        if (HideHitBubbles)
            return;

        var texture = GetTexture();
        var size = GetTextureSize(texture);
        _maxBubbleCount = _hitBubblesType switch
        {
            HitBubblesType.FallLeft or HitBubblesType.FallRight =>
                (int)((Size.X.Value - 2 * _borderPadding) / (size.X + _hitBubblePadding)),
            HitBubblesType.FallUp or HitBubblesType.FallDown =>
                (int)((Size.Y.Value - 2 * _borderPadding) / (size.Y + _hitBubblePadding)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Vector2 GetTextureSize(Texture2D texture)
    {
        var scale = SkinKeys.HitBubbleScale;
        return new Vector2(texture.Width, texture.Height) * scale;
    }

    public void AddJudgement(Judgement judgement)
    {
        if (HideHitBubbles)
            return;

        if (judgement == Judgement.Ghost)
            return;

        var judgementFlag = (HitBubbleRecordedJudgement)(1 << (int)judgement);
        if (!RecordedJudgements.HasFlag(judgementFlag))
            return;

        if (_maxBubbleCount <= 0)
            return;

        var texture = GetTexture();
        while (_bubbles.Count > _maxBubbleCount + 1)
        {
            _bubbles.RemoveFromFront();
        }

        HitBubble sprite;
        var moveOffset = 0f;
        var full = _bubbles.Count > _maxBubbleCount;
        if (full)
        {
            _bubbles.RemoveFromFront(out sprite);
            if (_bubbles.GetFront(out var nextHead))
                moveOffset = sprite.TargetPosition - nextHead.TargetPosition;
        }
        else
        {
            sprite = new HitBubble(_hitBubblesType)
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
        sprite.Tint = SkinKeys.JudgeColors[judgement];

        sprite.SetTargetPosition(moveOffset + (_bubbles.GetBack(out var back)
            ? GetNextTargetPosition(back)
            : _hitBubblesType switch
            {
                HitBubblesType.FallLeft => _borderPadding,
                HitBubblesType.FallRight => -_borderPadding,
                HitBubblesType.FallUp => _borderPadding,
                HitBubblesType.FallDown => -_borderPadding,
                _ => throw new ArgumentOutOfRangeException()
            }), false);

        _bubbles.AddToBack(sprite);

        const float opacityGradientMin = 0.5f;
        for (var i = 0; i < _bubbles.Count; i++)
        {
            var bubble = _bubbles[i];
            bubble.Alpha = _bubbles.Count == 1
                ? 1
                : opacityGradientMin + (1f - opacityGradientMin) *
                    ((float)(_maxBubbleCount - _bubbles.Count + i) / (_bubbles.Count - 1));
        }

        if (_bubbles.Count < _maxBubbleCount - 1)
            return;

        foreach (var bubble in _bubbles)
        {
            if (bubble == sprite)
                continue;
            bubble.TargetPosition += moveOffset;
        }
    }

    private float GetNextTargetPosition(HitBubble back)
    {
        back.FinishAnimation();
        return _hitBubblesType switch
        {
            HitBubblesType.FallLeft => back.RelativeRectangle.Right + _hitBubblePadding,
            HitBubblesType.FallRight => back.RelativeRectangle.Left - back.Width - _hitBubblePadding,
            HitBubblesType.FallUp => back.RelativeRectangle.Bottom + _hitBubblePadding,
            HitBubblesType.FallDown => back.RelativeRectangle.Top - back.Height - _hitBubblePadding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}