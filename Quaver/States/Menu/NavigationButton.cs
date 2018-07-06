using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;

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

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="size"></param>
        /// <param name="headerText"></param>
        /// <param name="image"></param>
        internal NavigationButton(Vector2 size, string headerText, Texture2D image)
        {
            Size = new UDim2D(size.X, size.Y);
            
            Header = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Tint = Colors.DarkGray,
                Size = new UDim2D(SizeX, 50)
            };

            HeaderText = new SpriteText
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                Font = Fonts.GoodTimes16,
                Text = headerText,
                TextScale = 0.75f,
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
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void OnClicked()
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
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