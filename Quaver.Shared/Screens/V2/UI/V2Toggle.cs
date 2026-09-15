using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.UI
{
    /// <summary>
    ///     Shared V2 on/off switch. An outer pill (the track) holds a smaller inner pill that says
    ///     "On" or "Off". The inner pill sits on the right when on, and on the left when off.
    /// </summary>
    public sealed class V2Toggle : RoundedButton
    {
        private SkinV2ToggleConfig Config { get; }

        /// <summary>
        ///     The visible track. It fades from an accent color under the inner pill to a dark color on
        ///     the other side. The button's own background is transparent, so only this shows.
        /// </summary>
        private GradientRoundedPanel TrackBackground { get; }

        private RoundedButton InnerPill { get; }

        public bool IsOn { get; private set; }

        public event EventHandler<bool> ToggledChanged;

        public V2Toggle(WobbleFontStore font, SkinV2ToggleConfig config, bool initialValue) : base(null)
        {
            Config = config;
            IsOn = initialValue;
            Size = new ScalableVector2(config.Width, config.Height);
            CornerRadius = config.Height / 2f;
            Tint = Color.Transparent;
            PerformHoverFade = true;

            TrackBackground = new GradientRoundedPanel(config.Height / 2f, Color.White, Color.White, false, 0.5f)
            {
                Parent = this,
                Size = Size,
                UsePreviousSpriteBatchOptions = true
            };

            var innerHeight = Math.Max(1, config.Height - config.InnerInset * 2);
            InnerPill = new RoundedButton(null)
            {
                Parent = this,
                Size = new ScalableVector2(config.InnerWidth, innerHeight),
                CornerRadius = innerHeight / 2f,
                IsClickable = false,
                PerformHoverFade = false
            };
            InnerPill.SetLabel(font, GetLabelText(), config.FontSize);

            ApplyState();
            Clicked += (sender, args) => SetOn(!IsOn, true);
        }

        public void SetOn(bool value, bool invokeEvent)
        {
            if (IsOn == value)
                return;

            IsOn = value;
            InnerPill.Label.Text = GetLabelText();
            ApplyState();

            if (invokeEvent)
                ToggledChanged?.Invoke(this, IsOn);
        }

        private void ApplyState()
        {
            var accent = SkinV2Color.Parse(IsOn ? Config.TrackOnEndColor : Config.TrackOffStartColor);
            var dark = SkinV2Color.Parse(IsOn ? Config.TrackOnStartColor : Config.TrackOffEndColor);
            TrackBackground.SetColors(accent, dark, IsOn);

            InnerPill.Tint = SkinV2Color.Parse(Config.InnerPillColor);
            InnerPill.Label.Tint = accent;
            InnerPill.Alignment = IsOn ? Alignment.MidRight : Alignment.MidLeft;
            InnerPill.X = IsOn ? -Config.InnerInset : Config.InnerInset;
        }

        private string GetLabelText() => LocalizationManager.Get(IsOn ? "Screen_Options_On" : "Screen_Options_Off");
    }
}
