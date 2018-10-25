using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Options.Keybindings
{
    public class KeybindSprite : Sprite
    {
        /// <summary>
        ///     The binded key.
        /// </summary>
        private Bindable<Keys> Key { get; }

        /// <summary>
        ///     The text that displays the current keybind
        /// </summary>
        private SpriteText KeyText { get; }

        /// <summary>
        ///     If the keybind sprite is selected.
        /// </summary>
        public bool Selected { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        public KeybindSprite(Bindable<Keys> key)
        {
            Key = key;
            Image = UserInterface.RoundedSquare;
            Size = new ScalableVector2(75, 75);

            KeyText = new SpriteText(BitmapFonts.Exo2Regular, XnaKeyHelper.GetStringFromKey(Key.Value), 18)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            Key.ValueChanged += OnKeybindChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(Selected ? Colors.SecondaryAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 60);
            KeyText.FadeToColor(Selected ? Colors.SecondaryAccent : Color.White, gameTime.ElapsedGameTime.TotalMilliseconds, 60);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Key.ValueChanged -= OnKeybindChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Updates the text when the key is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeybindChanged(object sender, BindableValueChangedEventArgs<Keys> args) => KeyText.Text = XnaKeyHelper.GetStringFromKey(args.Value);
    }
}