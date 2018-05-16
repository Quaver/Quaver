using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    /// <inheritdoc />
    /// <summary>
    /// This class will be inherited from every button class.
    /// </summary>
    internal class QuaverKeybindButton : QuaverButton
    {
        /// <summary>
        ///     The binded keybind value.
        /// </summary>
        private BindedValue<Keys> Keybind { get; }
        
        /// <summary>
        ///     The text sprite displayed in the key button.
        /// </summary>
        private QuaverSpriteText QuaverTextSprite { get; set; }

        /// <summary>
        ///     If the keybind button is currently selected.
        /// </summary>
        private bool Selected { get; set; }

        /// <summary>
        ///     Current tween value of the object. Used for animation.
        /// </summary>
        private float HoverCurrentTween { get; set; }

        /// <summary>
        ///     Target tween value of the object. Used for animation.
        /// </summary>
        private float HoverTargetTween { get; set; }
        
        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="keybind"></param>
        /// <param name="size"></param>
        /// <param name="???"></param>
        internal QuaverKeybindButton(BindedValue<Keys> keybind, Vector2 size)
        {
            Keybind = keybind;
            
            // Create text sprite.
            QuaverTextSprite = new QuaverSpriteText()
            {
                Text = XNAKeyHelper.GetStringFromKey(Keybind.Value),
                Size = new UDim2D(size.X, size.Y),
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Parent = this,
                TextColor = Color.White,
            };
            
            // Set size
            Size.X.Offset = size.X;
            Size.Y.Offset = size.Y;
            
            // Set color.
            Tint = QuaverColors.MainAccentInactive;

            // Hook onto when a user enters text.
            GameBase.GameWindow.TextInput += OnTextEntered;
        }

        /// <inheritdoc />
        /// <summary>
        ///     This method will be used for button logic and animation
        /// </summary>
        internal override void Update(double dt)
        {
            HoverCurrentTween = GraphicsHelper.Tween(HoverTargetTween, HoverCurrentTween, Math.Min(dt / 40, 1));
            
            base.Update(dt);
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     This method is called when the mouse hovers over the button
        /// </summary>
        protected override void MouseOver()
        {
            HoverTargetTween = 1;
        }

        /// <inheritdoc />
        /// <summary>
        ///     This method is called when the Mouse hovers out of the button
        /// </summary>
        protected override void MouseOut()
        {
            HoverTargetTween = 0;
        }

        /// <summary>
        ///     Checks for any key strokes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTextEntered(object sender, TextInputEventArgs e)
        {
            if (!Selected) 
                return;
            
            try
            {
                // Handle normal key inputs
                switch (e.Key)
                {
                    case Keys.Back:
                        break;
                    case Keys.Tab:
                        break;
                    case Keys.Delete:
                        break;
                    case Keys.Enter:
                        break;
                    default:
                        Keybind.Value = e.Key;
                        break;
                }
                    
                // Auto-unselected the button.
                Deselect();
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Could not select key: " + e.Character, LogType.Runtime);
                Logger.LogError(ex, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Deselects the button and sets it back to normal.
        /// </summary>
        private void Deselect()
        {
            Tint = QuaverColors.MainAccentInactive;
            Selected = false;
            HoverTargetTween = 0;
            QuaverTextSprite.Text = XNAKeyHelper.GetStringFromKey(Keybind.Value);
        }

        /// <inheritdoc />
        /// <summary>
        ///     When the keybind button is clicked, we'll want to register the button as "in progress"
        ///     and waits for a new keybind to be selected.
        /// </summary>
        protected override void OnClicked()
        {
            Selected = !Selected;
            
            if (!Selected) 
                return;
            
            Tint = QuaverColors.MainAccent;
            QuaverTextSprite.Text = "Key?";
            HoverTargetTween = 1;
        }

        /// <inheritdoc />
        /// <summary>
        ///     If the user clicks outside of the button, we'll want to deselect the button.
        /// </summary>
        protected override void OnClickedOutside()
        {
            if (Selected)
                Deselect();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Destroy keybind button.
        /// </summary>
        internal override void Destroy()
        {
            // Unhook the text entered event handler.
            GameBase.GameWindow.TextInput -= OnTextEntered;
            base.Destroy();
        }
    }
}
