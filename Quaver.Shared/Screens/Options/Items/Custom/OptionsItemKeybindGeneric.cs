using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemKeybindGeneric : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        /// </summary>
        private Bindable<GenericKey> BindedKey { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Text { get; }

        /// <summary>
        /// </summary>
        public bool Focused { get; private set; }

        /// <summary>
        /// </summary>
        private List<GenericKey> PreviousPressedKeys { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="bindedKeys"></param>
        /// <param name="bindedKey"></param>
        /// <param name="isKeybindFocused"></param>
        public OptionsItemKeybindGeneric(RectangleF containerRect, string name, Bindable<GenericKey> bindedKey) : base(containerRect, name)
        {
            BindedKey = bindedKey;

            Button = new IconButton(UserInterface.DropdownClosed)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(250, 35),
                Tint = ColorHelper.HexToColor("#181818"),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) => SetFocusedText();

            Button.ClickedOutside += (sender, args) =>
            {
                Focused = false;
                InitializeText();
                Text.Tint = Colors.MainAccent;
            };

            Text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 22)
            {
                Parent = Button,
                UsePreviousSpriteBatchOptions = true,
                Alignment = Alignment.MidLeft,
                X = 16,
                Tint = Colors.MainAccent
            };

            InitializeText();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HandleKeySelect();
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void InitializeText() => Text.Text = BindedKey.Value.GetName();

        /// <summary>
        /// </summary>
        private void SetFocusedText()
        {
            Focused = true;
            Text.Text = "Press a key...";
            Text.Tint = Color.Crimson;
        }

        /// <summary>
        /// </summary>
        private void HandleKeySelect()
        {
            if (!Focused)
                return;

            var keys = GenericKeyManager.GetPressedKeys();

            if (keys.Count != 0 && !PreviousPressedKeys.Contains(keys[0]))
            {
                BindedKey.Value = keys[0];
                Focused = false;
                InitializeText();
                Text.Tint = Colors.MainAccent;
            }

            PreviousPressedKeys = keys;
        }
    }
}