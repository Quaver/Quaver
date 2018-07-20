using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;
using AudioEngine = Quaver.Audio.AudioEngine;

namespace Quaver.States.Menu
{
    internal class NavigationButton : Button
    {
        /// <summary>
        ///     The header border.
        /// </summary>
        private Sprite Header { get; }

        /// <summary>
        ///     The text in the header
        /// </summary>
        private SpriteText HeaderText { get; }

        /// <summary>
        ///     The actual image to be displayed for this button.
        /// </summary>
        private Sprite ButtonImage { get; }

        /// <summary>
        ///     Keeps track of if the hover sound has played.
        /// </summary>
        private bool HoverSoundPlayed { get; set; }

        /// <summary>
        ///     The background of the footer.
        /// </summary>
        private Sprite FooterBackground { get; }

        /// <summary>
        ///     The actual footer text that describes the button.
        /// </summary>
        private SpriteText FooterText { get;  }

        /// <summary>
        ///     If the footer is always shown here.
        /// </summary>
        internal bool FooterAlwaysShown { get; set; }

        /// <summary>
        ///     The action that is called when the button is clicked.
        /// </summary>
        internal Action OnClick { get;}

        /// <summary>
        ///     If this is set to true, it'll call the action immediately.
        ///     If it isn't, it'll wait until it is handled in the navigation button container,
        ///     after any animations have been performed.
        /// </summary>
        internal bool CallActionImmediately { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="size"></param>
        /// <param name="headerText"></param>
        /// <param name="image"></param>
        /// <param name="footerText"></param>
        /// <param name="onClick"></param>
        /// <param name="callActionImmediately"></param>
        internal NavigationButton(Vector2 size, string headerText, Texture2D image, string footerText, Action onClick, bool callActionImmediately = false)
        {
            Size = new UDim2D(size.X, size.Y);
            Alpha = 0;
            OnClick = onClick;
            CallActionImmediately = callActionImmediately;

            Header = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Tint = Colors.DarkGray,
                Size = new UDim2D(SizeX, 45)
            };

            HeaderText = new SpriteText
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                Font = Fonts.Exo2Regular24,
                Text = headerText,
                TextScale = 0.58f,
                PosX = 20
            };

            ButtonImage = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(SizeX, SizeY - Header.SizeY),
                PosY = Header.SizeY,
                Image = image
            };

            if (footerText == null)
                return;

            FooterBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new UDim2D(SizeX, 0),
                Tint = Color.Black,
                Alpha = 0.70f
            };

            FooterText = new SpriteText()
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Font = Fonts.Exo2Regular24,
                Text = footerText,
                TextScale = 0.42f
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            if (FooterBackground != null)
            {
                if (IsTrulyHovered || FooterAlwaysShown)
                {
                    FooterText.Visible = true;
                    FooterBackground.SizeY = GraphicsHelper.Tween(40, FooterBackground.SizeY, Math.Min(dt / 60, 1));
                }
                else
                {
                    FooterText.Visible = false;
                    FooterBackground.SizeY = GraphicsHelper.Tween(0, FooterBackground.SizeY, Math.Min(dt / 60, 1));
                }
            }

            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void OnClicked()
        {
            if (IsClickable)
            {
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick, AudioEngine.EffectVolume - AudioEngine.EffectVolume / 2, 0.5f);

                // If the button doesn't have a container, we'll need to handle its action here.
                if (Parent.GetType() != typeof(NavigationButtonContainer))
                {
                    // Throw an exception if a developer decided that they wouldn't call the action immediately
                    // even if there wasn't a container.
                    if (!CallActionImmediately)
                        throw new ArgumentException("CallActionImmediately is false but the button does not " +
                                                    "have a NavigationButtonContainer. Either set to true, or " +
                                                    "add a container.");

                    OnClick();
                }
            }

            base.OnClicked();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOut()
        {
            HoverSoundPlayed = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void MouseOver()
        {
            if (!HoverSoundPlayed)
            {
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHover);
                HoverSoundPlayed = true;
            }
        }
    }
}