using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Volume
{
    public class VolumeControl : ImageButton
    {
        /// <summary>
        /// </summary>
        private static int WIDTH { get; } = 512;

        /// <summary>
        /// </summary>
        private static float SliderScale { get; } = 0.73f;

        /// <summary>
        /// </summary>
        private VolumeControlSlider FocusedSlider { get; set; }

        /// <summary>
        /// </summary>
        public double TimeInactive { get; private set; } = 2500;

        /// <summary>
        ///     The time elapsed since the last volume change.
        /// </summary>
        private double TimeElapsedSinceLastVolumeChange { get; set; } = 50;

        /// <summary>
        /// </summary>
        public List<VolumeControlSlider> Sliders { get; } = new List<VolumeControlSlider>
        {
            new VolumeControlSlider(WIDTH * SliderScale, UserInterface.MasterVolumeIcon,
                "Master", ConfigManager.VolumeGlobal),

            new VolumeControlSlider(WIDTH * SliderScale, UserInterface.MusicVolumeIcon,
                "Music", ConfigManager.VolumeMusic),

            new VolumeControlSlider(WIDTH * SliderScale, UserInterface.EffectVolumeIcon,
                "Effect", ConfigManager.VolumeEffect),
        };

        /// <summary>
        /// </summary>
        public VolumeControl() : base(UserInterface.VolumeControllerPanel)
        {
            Alignment = Alignment.BotRight;
            Size = new ScalableVector2(WIDTH, 216);
            X = Width + 50;
            Y = -95;

            FocusedSlider = Sliders.First();
            PositionSliders();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeElapsedSinceLastVolumeChange += gameTime.ElapsedGameTime.TotalMilliseconds;
            TimeInactive += gameTime.ElapsedGameTime.TotalMilliseconds;

            HandleInput(gameTime);
            HandleSliderColorChanges();

            if (TimeInactive >= 800 && Animations.Count == 0 && X < 0)
                MoveToX(Width + 50, Easing.OutQuint, 450);
            else if (TimeInactive == 0f && Animations.Count == 0 && X > 0)
                MoveToX(-30, Easing.OutQuint, 450);

            if (X > 0)
                FocusedSlider = Sliders.First();

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void PositionSliders()
        {
            for (var i = 0; i < Sliders.Count; i++)
            {
                var slider = Sliders[i];

                slider.Parent = this;
                slider.X = 16;

                if (i == 0)
                {
                    slider.Y = 16;
                    continue;
                }

                var previous = Sliders[i - 1];
                slider.Y = previous.Y + previous.Height  - 14;
            }
        }

        /// <summary>
        /// </summary>
        private void HandleInput(GameTime gameTime)
        {
            // Dictate which slider is the one that is currently focused.
            SetFocusedSlider();

            // Require either alt key to be pressed when changing volume.
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
                return;

            // Activate the volume control box.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Up)|| KeyboardManager.IsUniqueKeyPress(Keys.Down) ||
                KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Right)
                || MouseManager.CurrentState.ScrollWheelValue != MouseManager.PreviousState.ScrollWheelValue)
            {
                TimeInactive = 0;
            }

            if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
                UpdateVolume(5);
            else if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
                UpdateVolume(-5);

            if (KeyboardManager.CurrentState.IsKeyDown(Keys.Right))
            {
                if (TimeElapsedSinceLastVolumeChange >= 5)
                    UpdateVolume(1);
            }
            else if (KeyboardManager.CurrentState.IsKeyDown(Keys.Left))
            {
                if (TimeElapsedSinceLastVolumeChange >= 5)
                    UpdateVolume(-1);
            }
        }

        /// <summary>
        ///     Sets the currently focused slider out of our list of sliders.
        ///     The default focused slider is the master volume.
        /// </summary>
        private void SetFocusedSlider()
        {
            // A slider with the mouse currently hovered over it takes precedence over
            // any other action. That is automatically the focused slider.
            var focused = Sliders.Find(x => x.Slider.MouseInHoldSequence) ?? Sliders.Find(x => x.Slider.IsHovered);

            if (focused != null)
            {
                FocusedSlider = focused;
                TimeInactive = 0;
            }

            // If the user pressed the up key when determine the focused slider,
            // it becomes the one above. (If first in the list, it becomes the last.)
            if (KeyboardManager.IsUniqueKeyPress(Keys.Up) && (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt)))
            {
                // Play hover sound effect
                SkinManager.Skin?.SoundHover?.CreateChannel()?.Play();

                // Reset inactive timer.
                TimeInactive = 0;

                // If the focused slider is the first one in the list, we set it to the last.
                if (FocusedSlider == Sliders.First())
                {
                    FocusedSlider = Sliders.Last();
                    return;
                }

                // Otherwise, just set the focused to the previous in the list.
                var index = Sliders.IndexOf(FocusedSlider);
                FocusedSlider = Sliders[index - 1];
                return;
            }

            // If the user presses the down key, we switch the focused slider to the
            if (KeyboardManager.IsUniqueKeyPress(Keys.Down) && (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt)))
            {
                // Play hover sound effect
                SkinManager.Skin?.SoundHover?.CreateChannel()?.Play();

                // Reset inactive timer.
                TimeInactive = 0;

                // If the focused slider is the last one in the list, we set it to the first.
                if (FocusedSlider == Sliders.Last())
                {
                    FocusedSlider = Sliders.First();
                    return;
                }

                var index = Sliders.IndexOf(FocusedSlider);
                FocusedSlider = Sliders[index + 1];
            }
        }

        /// <summary>
        ///     Responsible for updating the sound of the focused slider.
        /// </summary>
        /// <param name="amount"></param>
        private void UpdateVolume(int amount)
        {
            FocusedSlider.BindedValue.Value += amount;
            TimeInactive = 0;
            TimeElapsedSinceLastVolumeChange = 0;
        }

        /// <summary>
        /// </summary>
        private void HandleSliderColorChanges()
        {
            for (var i = 0; i < Sliders.Count; i++)
            {
                if (Sliders[i] == FocusedSlider)
                    Sliders[i].Select();
                else
                    Sliders[i].Deselect();
            }
        }
    }
}