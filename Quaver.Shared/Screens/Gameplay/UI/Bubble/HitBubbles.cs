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
        SkinKeys == null
        || (Screen?.IsCalibratingOffset ?? false)
        || SkinKeys.HitBubblesScale == 0
        || !ConfigManager.DisplayHitBubbles.Value;

    private HitBubbleRecordedJudgement RecordedJudgements =>
        ModManager.IsActivated(ModIdentifier.Autoplay) || (Screen.IsPlayTesting && Screen.InReplayMode)
            ? HitBubbleRecordedJudgement.All
            : SkinKeys.HitBubblesRecordedJudgements;

    /// <summary>
    ///     Skin keys retrieval.
    ///     Doing excessive null check here because there happens to be a case where this could be null
    /// </summary>
    /// <remarks>
    ///     will crash in <see cref="OnRectangleRecalculated"/> if we skip null check
    /// </remarks>
    private static SkinKeys SkinKeys => SkinManager.Skin?.Keys?.GetValueOrDefault(MapManager.Selected?.Value?.Mode ?? GameMode.Keys4);

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
        }
        else
        {
            sprite = new HitBubble(_hitBubblesType)
            {
                Parent = this,
                Alignment = _hitBubblesType switch
                {
                    HitBubblesType.FallLeft => Alignment.MidRight,
                    HitBubblesType.FallRight => Alignment.MidLeft,
                    HitBubblesType.FallUp => Alignment.BotCenter,
                    HitBubblesType.FallDown => Alignment.TopCenter,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
        }

        moveOffset = GetSeparation();

        var size = GetTextureSize(texture);
        sprite.Size = new ScalableVector2(size.X, size.Y);
        sprite.Image = texture;
        sprite.Tint = SkinKeys.JudgeColors[judgement];

        sprite.SetTargetPosition(_hitBubblesType switch
        {
            HitBubblesType.FallLeft => -_borderPadding,
            HitBubblesType.FallRight => _borderPadding,
            HitBubblesType.FallUp => -_borderPadding,
            HitBubblesType.FallDown => _borderPadding,
            _ => throw new ArgumentOutOfRangeException()
        }, false);

        _bubbles.AddToBack(sprite);

        const float opacityGradientMin = 0.35f;
        for (var i = 0; i < _bubbles.Count; i++)
        {
            var bubble = _bubbles[i];
            bubble.Alpha = _bubbles.Count == 1
                ? 1
                : EasingFunctions.EaseOutQuad(opacityGradientMin, 1,
                ((float)(_maxBubbleCount - _bubbles.Count + i) / (_maxBubbleCount - 1)));
        }

        foreach (var bubble in _bubbles)
        {
            if (bubble == sprite)
                continue;
            bubble.TargetPosition += moveOffset;
        }
    }

    private float GetSeparation()
    {
        var tex = GetTextureSize(GetTexture());
        return _hitBubblesType switch
        {
            HitBubblesType.FallLeft => -_hitBubblePadding - tex.X,
            HitBubblesType.FallRight => _hitBubblePadding + tex.X,
            HitBubblesType.FallUp => -_hitBubblePadding - tex.Y,
            HitBubblesType.FallDown => _hitBubblePadding + tex.Y,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}