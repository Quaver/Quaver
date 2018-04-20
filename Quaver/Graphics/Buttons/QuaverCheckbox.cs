using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Graphics.Base;
using Quaver.Graphics.Colors;
using Quaver.Main;

namespace Quaver.Graphics.Buttons
{
    internal class QuaverCheckbox : QuaverButton
    {
        /// <summary>
        ///     The binded value for this checkbox.
        /// </summary>
        internal BindedValue<bool> BindedValue { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="bindedValue"></param>
        /// <param name="size"></param>
        internal QuaverCheckbox(BindedValue<bool> bindedValue, Vector2 size, DrawRectangle clickArea = null)
        {
            BindedValue = bindedValue;

            Size.X.Offset = size.X;
            Size.Y.Offset = size.Y;
            
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
            
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        ///     When the checkbox is clicked, set the value to its opposite. 
        /// </summary>
        protected override void OnClicked()
        {
            BindedValue.Value = !BindedValue.Value;
            GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundClick);
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
    }
}