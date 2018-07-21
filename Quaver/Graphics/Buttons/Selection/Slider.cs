using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Bindables;
using Quaver.Config;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Buttons.Selection
{
    internal class Slider : Button
    {
        /// <summary>
        ///     Global property that dictates if **any** slider is actually held.
        /// </summary>
        internal static bool SliderAlreadyHeld { get; set; }

        /// <summary>
        ///     The value that this slider binds to.
        /// </summary>
        internal BindedInt BindedValue { get; }

        /// <summary>
        ///     The progress slider image.
        /// </summary>
        internal Sprite ProgressBall { get; }

        /// <summary>
        ///     If the mouse is held down and hasn't let go yet.
        /// </summary>
        internal bool MouseInHoldSequence { get; set; }
       
        /// <summary>
        ///     If the slider is vertical or not.
        ///         True = Vertical
        ///         False = Horizontal
        /// </summary>
        private bool IsVertical { get; }
        
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
        private UDim2D ProgressBallSize { get; } = new UDim2D(15, 15);

        /// <summary>
        ///     Dictates whether the MouseOver sound has already been played for this button.
        /// </summary>
        private bool MouseOverSoundPlayed { get; set; }

        /// <summary>
        ///     The previous value that we have stored.
        /// </summary>
        private int PreviousValue { get; set; }

         /// <summary>
        ///     The percentage of the current slider value.
        /// </summary>
        private int ProgressPercentage => (BindedValue.Value - BindedValue.MinValue) * 100 / BindedValue.MaxValue - BindedValue.MinValue * 100;

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new SliderButton. Takes in a BindedInt as an argument.
        /// </summary>
        /// <param name="binded"></param>
        /// <param name="size"></param>
        /// <param name="lineColor"></param>
        /// <param name="progressColor"></param>
        /// <param name="progressTexture"></param>
        internal Slider(BindedInt binded, Vector2 size, bool vertical = false)
        {
            BindedValue = binded;
            IsVertical = vertical;
            Held += MouseHeld;
            
            Size.X.Offset = size.X;
            Size.Y.Offset = size.Y;
            Tint = Color.White;
                      
            // Create the progress sliding thing.
            ProgressBall = new Sprite()
            {
                Alignment = Alignment.TopLeft,
                Image = FontAwesome.CircleClosed,
                Size = ProgressBallSize,
                Tint = Color.White,
                Parent = this
            };
              
            // Whenever the value changes, we need to update the slider accordingly,
            // so hook onto this event with a handler.
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
            {
                MouseInHoldSequence = false;
                SliderAlreadyHeld = false;
            }

            // Handle the changing of the value for this button.
            if (MouseInHoldSequence)
                HandleSliderValueChanges();

            PreviousMouseState = GameBase.MouseState;
            SetProgressPosition(dt);
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
            int percentage;
            if (IsVertical)
                percentage = 100 - (int)((GameBase.MouseState.Y - AbsolutePosition.Y) / AbsoluteSize.Y * 100);
            else
                percentage = (int)((GameBase.MouseState.X - AbsolutePosition.X) / AbsoluteSize.X * 100);

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

            DrawRectangle clickArea;
            
            if (IsVertical)
                clickArea = new DrawRectangle(GlobalRectangle.X - offset / 2f, GlobalRectangle.Y, GlobalRectangle.Width + offset, GlobalRectangle.Height);
            else
                clickArea = new DrawRectangle(GlobalRectangle.X, GlobalRectangle.Y - offset / 2f, GlobalRectangle.Width, GlobalRectangle.Height + offset);
            
            return GraphicsHelper.RectangleContains(clickArea, GraphicsHelper.PointToVector2(GameBase.MouseState.Position));
        }

        /// <summary>
        ///     When the button is moused over and held down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseHeld(object sender, EventArgs e)
        {
            if (SliderAlreadyHeld || PreviousMouseState.LeftButton != ButtonState.Pressed)
                return;
            
            MouseInHoldSequence = true;
            SliderAlreadyHeld = true;
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOver()
        {
            // Play sound effect if necessary
            if (!MouseOverSoundPlayed && !SliderAlreadyHeld)
            {
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHover);
                MouseOverSoundPlayed = true;
            }         
        }
        
        /// <inheritdoc />
        /// <summary>
        ///     MouseOut
        /// </summary>
        protected override void MouseOut()
        {           
            // Reset MouseOverSoundPlayed for this particular button now that we've moused out.
            MouseOverSoundPlayed = false;
        }

        /// <summary>
        ///     Changes the color of both the slider line + progress ball.
        /// </summary>
        /// <param name="color"></param>
        internal void ChangeColor(Color color)
        {
            Tint = color;
            ProgressBall.Tint = color;
        }
        
        /// <summary>
        ///     Sets the correct position of the progress image
        /// </summary>
        private void SetProgressPosition(double dt)
        {
            if (IsVertical)
            {
                ProgressBall.PosX = GraphicsHelper.Tween(Size.X.Offset / 2 - ProgressBall.SizeX / 2, ProgressBall.PosX, Math.Min(dt / 30, 1));
                ProgressBall.PosY = GraphicsHelper.Tween((100 - ProgressPercentage) / 100f * Size.Y.Offset, ProgressBall.PosY, Math.Min(dt / 30, 1));
;            }
            else
            {
                ProgressBall.PosX = GraphicsHelper.Tween(ProgressPercentage / 100f * Size.X.Offset, ProgressBall.PosX, Math.Min(dt / 30, 1));
                ProgressBall.PosY = GraphicsHelper.Tween(Size.Y.Offset / 2 - ProgressBall.SizeY / 2, ProgressBall.PosY, Math.Min(dt / 30, 1));
            }
        }

        /// <summary>
        ///     Plays a sound effect at a given value based on the previously captured binded val.
        /// </summary>
        /// <param name="val"></param>
        private void PlaySoundEffectWhenChanged(int val)
        {            
            // Set the min and max based on the direction we're going.
            var max = val > PreviousValue ? 1f : 0;
            var min = val > PreviousValue ? 0 : -1f;
            
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHover, ProgressPercentage* (max - min) / 100f + min);
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
            // Play a sound effect
            //PlaySoundEffectWhenChanged(e.Value);

            // Update the previous value.
            PreviousValue = e.Value;
        }
    }
}