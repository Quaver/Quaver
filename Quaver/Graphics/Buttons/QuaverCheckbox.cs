using System;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Graphics.Base;
using Quaver.Graphics.Colors;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    internal class QuaverCheckbox : QuaverButton
    {
        /// <summary>
        ///     The binded value for this checkbox.
        /// </summary>
        internal BindedValue<bool> BindedValue { get; }

        /// <summary>
        ///     The size of the button
        /// </summary>
        internal Vector2 CheckboxSize { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="bindedValue"></param>
        /// <param name="size"></param>
        internal QuaverCheckbox(BindedValue<bool> bindedValue, Vector2 size, DrawRectangle clickArea = null)
        {
            BindedValue = bindedValue;
            CheckboxSize = size;

            Size.X.Offset = CheckboxSize.X;
            Size.Y.Offset = CheckboxSize.Y;
            
            // Hook onto this value.
            BindedValue.OnValueChanged += OnValueChanged;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Always set the checkbox image.
            SetCheckboxImage();

            // PerformHoverAnimation(dt);
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        ///     When the checkbox is clicked, set the value to its opposite. 
        /// </summary>
        protected override void OnClicked()
        {
            BindedValue.Value = !BindedValue.Value;
            GameBase.AudioEngine.PlaySoundEffect(BindedValue.Value ? GameBase.LoadedSkin.SoundClick : GameBase.LoadedSkin.SoundBack);
            
            base.OnClicked();
        }

        /// <summary>
        ///     Sets the checkbox's image based on the binded value.
        /// </summary>
        private void SetCheckboxImage()
        {
            Image = BindedValue.Value ? FontAwesome.CircleClosed : FontAwesome.CircleOpen;
            Tint = BindedValue.Value ? QuaverColors.MainAccent : QuaverColors.MainAccentInactive;
        }
        
        /// <summary>
        ///     EventHandler for the binded value. Updates the UI accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValueChanged(object sender, BindedValueEventArgs<bool> e)
        {
             SetCheckboxImage();
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     Mouse Out
        /// </summary>
        protected override void MouseOut()
        {
            // TODO: Display a tooltip, perhaps?
        }

        /// <inheritdoc />
        /// <summary>
        ///     Mouse Over
        /// </summary>
        protected override void MouseOver()
        {
            // TODO: Display a tooltip, perhaps?
        }

        /// <summary>
        ///     Performs a hover animation and increases/decreases the size of the checkbox slightly.)
        /// </summary>
        /// <param name="dt"></param>
        private void PerformHoverAnimation(double dt)
        {
            if (IsHovered)
            {
                var scale = 1.2f;
                
                // Increase the size of the checkbox slightly.
                Size.X.Offset = GraphicsHelper.Tween(CheckboxSize.X * scale, Size.X.Offset, Math.Min(dt / 30, 1));
                Size.Y.Offset = GraphicsHelper.Tween(CheckboxSize.Y * scale, Size.Y.Offset, Math.Min(dt / 30, 1));
            }
            else
            {
                // Set checkbox size back to normal.
                Size.X.Offset = GraphicsHelper.Tween(CheckboxSize.X, Size.X.Offset, Math.Min(dt / 30, 1));
                Size.Y.Offset = GraphicsHelper.Tween(CheckboxSize.Y, Size.Y.Offset, Math.Min(dt/ 30, 1));
            }         
        }
    }
}