using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Skinning.V2;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2.UI
{
    internal sealed class VolumeSlider : Container
    {
        private BindableInt Value { get; }

        private SkinV2VolumeDialogConfig Config { get; }

        private Sprite Card { get; }

        private RoundedButton Track { get; }

        private RoundedButton Fill { get; }

        private Sprite Footer { get; }

        private bool Focused { get; set; }

        public VolumeSlider(Texture2D icon, BindableInt value, SkinV2VolumeDialogConfig config)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Size = new ScalableVector2(Config.CardWidth, Config.CardHeight);

            Card = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = RoundedRectTextureCache.Get(Width, Height, Config.CardCornerRadius),
                Tint = SkinV2Color.Parse(Config.CardColor)
            };

            Track = new RoundedButton((sender, args) => UpdateValue())
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = Config.TrackTopInset,
                Size = new ScalableVector2(Config.TrackWidth, Config.TrackHeight),
                CornerRadius = Config.TrackCornerRadius,
                Tint = SkinV2Color.Parse(Config.TrackColor),
                PerformHoverFade = false
            };

            Fill = new RoundedButton
            {
                Parent = Track,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Config.TrackWidth, 1),
                CornerRadius = Config.TrackCornerRadius,
                Tint = SkinV2Color.Parse(Config.InactiveFillColor),
                PerformHoverFade = false,
                IsInteractionEnabled = false
            };

            Footer = new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Config.CardWidth, Config.FooterHeight),
                Image = RoundedRectTextureCache.Get(Config.CardWidth, Config.FooterHeight, new RoundedRectCornerRadii(0, 0, Config.CardCornerRadius, Config.CardCornerRadius)),
                Tint = SkinV2Color.Parse(Config.FooterColor),
                SetChildrenAlpha = true
            };

            var iconHeight = Math.Min(Config.IconHeight, Config.FooterHeight);
            new Sprite
            {
                Parent = Footer,
                Alignment = Alignment.MidCenter,
                Image = icon,
                Size = new ScalableVector2(icon.Width * iconHeight / icon.Height, iconHeight),
                Tint = SkinV2Color.Parse(Config.IconColor)
            };

            Value.ValueChanged += OnValueChanged;
            UpdateFill();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (Track.IsHeld && Track.MouseButtonClicked == MouseButton.Left)
                UpdateValue();
        }

        public void SetFocused(bool focused)
        {
            if (Focused == focused)
                return;

            Focused = focused;
            Fill.Tint = SkinV2Color.Parse(Focused ? Config.SelectedColor : Config.InactiveFillColor);
            Footer.Tint = SkinV2Color.Parse(Focused ? Config.SelectedColor : Config.FooterColor);
        }

        public void AdjustValue(int amount) => Value.Value += amount;

        public void FadeOut(int duration)
        {
            Card.FadeTo(0, Easing.Linear, duration);
            Track.FadeTo(0, Easing.Linear, duration);
            Footer.FadeTo(0, Easing.Linear, duration);
        }

        private void UpdateValue()
        {
            var amount = MathHelper.Clamp(1f - (MouseManager.CurrentState.Y - Track.AbsolutePosition.Y) / Track.AbsoluteSize.Y, 0, 1);
            Value.Value = (int) Math.Round(Value.MinValue + amount * (Value.MaxValue - Value.MinValue));
        }

        private void OnValueChanged(object sender, BindableValueChangedEventArgs<int> args) => UpdateFill();

        private void UpdateFill()
        {
            var range = Value.MaxValue - Value.MinValue;
            var amount = range == 0 ? 0 : (Value.Value - Value.MinValue) / (float) range;
            var height = Config.TrackHeight * MathHelper.Clamp(amount, 0, 1);
            Fill.Visible = height > 0;
            Fill.Height = Math.Max(1, height);
        }

        public override void Destroy()
        {
            Value.ValueChanged -= OnValueChanged;
            base.Destroy();
        }
    }
}
