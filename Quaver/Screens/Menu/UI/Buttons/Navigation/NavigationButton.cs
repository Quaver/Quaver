using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Screens.Menu.UI.Buttons.Navigation
{
    public class NavigationButton : Button
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
            Size = new ScalableVector2(size.X, size.Y);
            Alpha = 0;
            OnClick = onClick;
            CallActionImmediately = callActionImmediately;

            Header = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Tint = Colors.DarkGray,
                Size = new ScalableVector2(Width, 45)
            };

            HeaderText = new SpriteText(Fonts.Exo2Regular24, headerText)
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                TextScale = 0.58f,
                X = 20
            };

            ButtonImage = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, Height - Header.Height),
                Y = Header.Height,
                Image = image
            };

            if (footerText == null)
                return;

            FooterBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 0),
                Tint = Color.Black,
                Alpha = 0.70f
            };

            FooterText = new SpriteText(Fonts.Exo2Regular24, footerText)
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                TextScale = 0.42f
            };

            Clicked += (sender, args) => OnClicked();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>>
        public override void Update(GameTime gameTime)
        {
            if (FooterBackground != null)
            {
                if (IsHovered || FooterAlwaysShown)
                {
                    FooterText.Visible = true;
                    FooterBackground.Height = MathHelper.Lerp(FooterBackground.Height, 40, (float) Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));
                }
                else
                {
                    FooterText.Visible = false;
                    FooterBackground.Height = MathHelper.Lerp(FooterBackground.Height, 0, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));
                }
            }

            if (IsHovered)
            {
                if (!HoverSoundPlayed)
                {
                    SkinManager.Skin.SoundHover.CreateChannel(ConfigManager.VolumeEffect.Value).Play();
                    HoverSoundPlayed = true;
                }
            }
            else
            {
                HoverSoundPlayed = false;
            }

            base.Update(gameTime);
        }

        /// <summary>
        ///     When the button is clicked, it will call the action given in the constructor.
        /// </summary>
        private void OnClicked()
        {
            if (!IsClickable)
                return;

            SkinManager.Skin.SoundClick.CreateChannel(ConfigManager.VolumeEffect.Value).Play();

            // If the button doesn't have a container, we'll need to handle its action here.
            if (Parent.GetType() == typeof(NavigationButtonContainer))
                return;

            // Throw an exception if a developer decided that they wouldn't call the action immediately
            // even if there wasn't a container.
            if (!CallActionImmediately)
                throw new ArgumentException("CallActionImmediately is false but the button does not " +
                                            "have a NavigationButtonContainer. Either set to true, or " +
                                            "add a container.");

            OnClick();
        }
    }
}
