using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The rail entry shown while a search is running. It shows the search icon, and when the rail
    ///     is open also "Search Result" and a reset icon that clears the search.
    /// </summary>
    internal sealed class OptionsSearchRailButton : RoundedButton
    {
        private SkinV2OptionsCategoryNavigationConfig Config { get; }

        private MarqueeSpriteText Marquee { get; }

        private Sprite ResetIcon { get; }

        private Action ResetAction { get; }

        internal OptionsSearchRailButton(Texture2D searchIcon, Texture2D resetIcon, WobbleFontStore font,
            SkinV2OptionsCategoryNavigationConfig config, Action resetAction)
        {
            Config = config;
            ResetAction = resetAction;
            Size = new ScalableVector2(config.ButtonHeight, config.ButtonHeight);
            CornerRadius = config.CornerRadius;
            Tint = SkinV2Color.Parse(config.RailButtonSelectedColor);
            PerformHoverFade = true;

            var foreground = SkinV2Color.Parse(config.SelectedForegroundColor);
            SetIcon(searchIcon, new Vector2(config.IconSize, config.IconSize));
            Icon.Tint = foreground;

            Marquee = new MarqueeSpriteText(font, LocalizationManager.Get("Screen_Options_SearchResultLabel"),
                config.FontSize, 1)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                Visible = false
            };
            Marquee.TextSprite.Tint = foreground;

            ResetIcon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Image = resetIcon,
                Size = new ScalableVector2(config.SearchResetIconSize, config.SearchResetIconSize),
                Tint = foreground,
                UsePreviousSpriteBatchOptions = true,
                Visible = false
            };

            Clicked += OnClicked;
            ApplyContentLayout();
        }

        public override void Update(GameTime gameTime)
        {
            Marquee.IsActive = Marquee.Visible;
            base.Update(gameTime);
            ApplyContentLayout();
        }

        /// <summary>
        ///     Shows the label and reset icon from 0 (rail closed) to 1 (rail open), in step with the
        ///     category buttons.
        /// </summary>
        internal void SetLabelExpansionProgress(float progress)
        {
            progress = MathHelper.Clamp(progress, 0, 1);
            Marquee.TextSprite.Alpha = progress;
            Marquee.Visible = progress > 0.001f;
            Marquee.IsActive = Marquee.Visible;

            // The button sets its own alpha on its child sprites when fading on hover,
            // so the reset icon is shown with Visible instead of alpha.
            ResetIcon.Visible = Marquee.Visible;
        }

        private void OnClicked(object? sender, EventArgs args)
        {
            if (ResetIcon.Visible && ResetIcon.ScreenRectangle.Contains(MouseManager.CurrentState.Position))
                ResetAction?.Invoke();
        }

        private void ApplyContentLayout()
        {
            if (Icon == null || Marquee == null || ResetIcon == null)
                return;

            Icon.Alignment = Alignment.MidLeft;
            Icon.X = Math.Max(0, (Config.ButtonHeight - Config.IconSize) / 2f);
            ResetIcon.Alignment = Alignment.MidRight;
            ResetIcon.X = -Config.HorizontalPadding;
            Marquee.Alignment = Alignment.MidLeft;
            Marquee.X = Icon.X + Config.IconSize + Config.LabelSpacing;

            var resetIconSpace = Config.HorizontalPadding * 2 + Config.SearchResetIconSize;
            Marquee.Size = new ScalableVector2(Math.Max(1, Width - Marquee.X - resetIconSpace), Height);
        }
    }
}
