using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics.Base;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;
using AudioEngine = Quaver.Audio.AudioEngine;

namespace Quaver.Graphics.Buttons.Sliders
{
    internal class QuaverSlider : QuaverButton
    {
        /// <summary>
        ///     The value that this slider binds to.
        /// </summary>
        private BindedInt BindedValue { get; }

        /// <summary>
        ///     The progress slider image.
        /// </summary>
        private QuaverSprite ProgressImage { get; }

        /// <summary>
        ///     If the mouse is held down and hasn't let go yet.
        /// </summary>
        private bool MouseInHoldSequence { get; set; }

        /// <summary>
        ///     The last percentage that the slider has changed the BindedInt to.
        /// </summary>
        private int LastPercentage { get; set; }

        /// <summary>
        ///     The mouse state of the previous frame.
        /// </summary>
        private MouseState PreviousMouseState { get; set; }

        /// <summary>
        ///     The original size of the progress image
        /// </summary>
        private UDim2D ProgressImageSize { get; } = new UDim2D(28, 28);

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new SliderButton. Takes in a BindedInt as an argument.
        /// </summary>
        /// <param name="binded"></param>
        /// <param name="size"></param>
        /// <param name="lineColor"></param>
        /// <param name="progressColor"></param>
        /// <param name="progressTexture"></param>
        internal QuaverSlider(BindedInt binded, Vector2 size, Color lineColor, Color progressColor, Texture2D progressTexture = null)
        {
            BindedValue = binded;
            Held += MouseHeld;
            
            Size.X.Offset = size.X;
            Size.Y.Offset = size.Y;
            Tint = lineColor;
            
            // Default the texture to just a simple circle if not specified.
            if (progressTexture == null)
                progressTexture = FontAwesome.Circle;
            
            // Create the progress sliding thing.
            ProgressImage = new QuaverSprite()
            {
                Alignment = Alignment.TopLeft,
                Image = progressTexture,
                Size = ProgressImageSize,
                Tint = progressColor,
                Parent = this
            };
            
            SetProgressPosition();

            // Whenever the value changes, we need to 
            BindedValue.OnValueChanged += OnValueChanged;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            if (GameBase.MouseState.LeftButton == ButtonState.Released)
                MouseInHoldSequence = false;

            // Handle the changing of the value for this button.
            if (MouseInHoldSequence)
                HandleSliderValueChanges();

            PreviousMouseState = GameBase.MouseState;
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Destroy
        /// </summary>
        internal override void Destroy()
        {
            // Remove the event handler for the binded value.
            BindedValue.OnValueChanged -= OnValueChanged;
            
            base.Destroy();
        }

        /// <summary>
        ///     Gets the new value of the slider and sets it to the binded value.
        /// </summary>
        private void HandleSliderValueChanges()
        {
            // Get the percentage of the mouse position, to the size of the slider.
            var percentage = (int)((GameBase.MouseState.X - AbsolutePosition.X) / AbsoluteSize.X * 100);

            // If the percentage of the MouseX/SliderX is 0% or lower, set the binded value to the minimum.
            if (percentage <= 0 && LastPercentage > 0)
                BindedValue.Value = BindedValue.MinValue;
            // If the percentage of the MouseX/SliderX is 100% or higher set the binded value to the maximum.
            else if (percentage >= 100 && LastPercentage < 100)
                BindedValue.Value = BindedValue.MaxValue;             
            // If the percentage is anything else, set it accordingly.
            else if (percentage > 0 && percentage < 100 && LastPercentage != percentage)
                BindedValue.Value = (int)(percentage / 100f * BindedValue.MaxValue);
         
            LastPercentage = percentage;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Gets the click area of the slider.
        /// </summary>
        /// <returns></returns>
        protected override bool GetClickArea()
        {
            // The RectY increase of the click area.
            const int offset = 40;
          
            var clickArea = new DrawRectangle(GlobalRectangle.X, GlobalRectangle.Y - offset, GlobalRectangle.Width, GlobalRectangle.Height + offset);
            return GraphicsHelper.RectangleContains(clickArea, GraphicsHelper.PointToVector2(GameBase.MouseState.Position));
        }

        /// <summary>
        ///     When the button is moused over and held down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseHeld(object sender, EventArgs e)
        {
            if (PreviousMouseState.LeftButton == ButtonState.Released)
                MouseInHoldSequence = true;   
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOver()
        {
            // Increase progress thing's size
            ProgressImage.Size = new UDim2D(ProgressImageSize.X.Offset + 5, ProgressImageSize.Y.Offset + 5);
            SetProgressPosition();
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOut()
        {
            // Set the progress thing's size back to the original.
            ProgressImage.Size = ProgressImageSize;
            SetProgressPosition();
        }

        /// <summary>
        ///     Sets the correct position of the progress image
        /// </summary>
        private void SetProgressPosition()
        {
            var percentage = BindedValue.Value - BindedValue.MinValue / BindedValue.MaxValue * 100;
            ProgressImage.Position = new UDim2D(percentage / 100f * Size.X.Offset, Size.Y.Offset / 2 - ProgressImage.SizeY / 2, 1, 0);
        }

        /// <summary>
        ///     This method is an event handler specifically for handling the case of when the value of the binded value
        ///     has changed. This will automatically set the progress position.
        /// 
        ///     This is mainly for cases such as volume, where it can be controlled through means other than the slider
        ///     (example: keybinds), and if the slider is displayed, it should update as well.
        ///  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValueChanged(object sender, BindedValueEventArgs<int> e)
        {
            // Automatically set the progress once the value has changed.
            SetProgressPosition();
        }
    }
}