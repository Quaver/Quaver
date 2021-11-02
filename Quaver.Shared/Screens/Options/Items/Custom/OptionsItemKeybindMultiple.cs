using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Helpers;
using Wobble.Input;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemKeybindMultiple : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

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
        private List<Bindable<GenericKey>> BindedKeys { get; }

        /// <summary>
        ///     The keys that are ready to be set
        /// </summary>
        private List<GenericKey> QueuedKeys { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="keys"></param>
        /// <param name="isKeybindFocused"></param>
        public OptionsItemKeybindMultiple(RectangleF containerRect, string name, List<Bindable<GenericKey>> keys) : base(containerRect, name)
        {
            BindedKeys = keys;

            Button = new IconButton(UserInterface.DropdownClosed)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(250, 35),
                Tint = ColorHelper.HexToColor("#181818"),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                QueuedKeys = new List<GenericKey>();
                SetFocusedText();
            };

            Button.ClickedOutside += (sender, args) =>
            {
                Focused = false;
                InitializeText(BindedKeys.Select(x => x.Value).ToList());
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

            InitializeText(BindedKeys.Select(x => x.Value).ToList());
        }

        /// <inheritdoc />
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
        private void InitializeText(List<GenericKey> keys)
        {
            var text = "";

            foreach (var key in keys)
                text += key.GetName() + " ";

            Text.Text = text;
        }

        /// <summary>
        /// </summary>
        private void SetFocusedText()
        {
            Focused = true;
            Text.Text = $"Press {BindedKeys.Count} keys...";
            Text.Tint = Color.Crimson;
        }

        /// <summary>
        /// </summary>
        private void HandleKeySelect()
        {
            if (!Focused)
                return;

            var keys = GenericKeyManager.GetPressedKeys();

            foreach (var key in keys)
            {
                if (keys.Count != 0 && !PreviousPressedKeys.Contains(key) && !QueuedKeys.Contains(key))
                {
                    QueuedKeys.Add(key);
                    InitializeText(QueuedKeys);
                    Text.Tint = Color.Crimson;

                    if (QueuedKeys.Count == BindedKeys.Count)
                    {
                        for (var i = 0; i < QueuedKeys.Count; i++)
                            BindedKeys[i].Value = QueuedKeys[i];

                        Focused = false;
                        InitializeText(BindedKeys.Select(x => x.Value).ToList());
                        Text.Tint = Colors.MainAccent;
                    }
                }
            }

            PreviousPressedKeys = keys;
        }
    }
}