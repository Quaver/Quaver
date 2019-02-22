/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Graphics.Overlays.Volume
{
    public class VolumeController : Container
    {
         /// <summary>
        ///     If the volume controller is currently active.
        ///     (It's considered active if the box is visible and the user is holding down either alt key.)
        /// </summary>
        public bool IsActive => SurroundingBox.Visible && (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt)
                                                             || KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt));

        /// <summary>
        ///     The surrounding box of the volume controller.
        /// </summary>
        private Sprite SurroundingBox { get; set; }

        /// <summary>
        ///     The border for the surrounding box.
        /// </summary>
        private Sprite SurroundingBoxBorder { get; set; }

        /// <summary>
        ///     The slider that controls the master volume.
        /// </summary>
        private Slider MasterVolumeSlider { get; set; }

        /// <summary>
        ///  The icon to that's associated with the master volume.
        /// </summary>
        private Sprite MasterVolumeSliderIcon { get; set; }

        /// <summary>
        ///     The slider that controls the music volume.
        /// </summary>
        private Slider MusicVolumeSlider { get; set; }

        /// <summary>
        ///     The icon that's asscoiated with the music volume slider.
        /// </summary>
        private Sprite MusicVolumeSliderIcon { get; set; }

        /// <summary>
        ///     The slider that controls the sound effects.
        /// </summary>
        private Slider EffectVolumeSlider { get; set; }

        /// <summary>
        ///     The icon that's associated with the effect volume slider.
        /// </summary>
        private Sprite EffectVolumeSliderIcon { get; set; }

        /// <summary>
        ///     Array containing all of the sliders, so we can iterate over them.
        /// </summary>
        private List<Slider> Sliders { get; }

        /// <summary>
        ///     List containing all of the slider icons, so we can iterate over them.
        /// </summary>
        private List<Sprite> SliderIcons { get; }

        /// <summary>
        ///     The size of each slider.
        /// </summary>
        private Vector2 SliderSize { get; }

        /// <summary>
        ///     The slider that is currently "focused"
        /// </summary>
        private Slider FocusedSlider { get; set; }

        /// <summary>
        ///     The time elapsed since the last volume change.
        /// </summary>
        private double TimeElapsedSinceLastVolumeChange { get; set; } = 50;

        /// <summary>
        ///     The amount of time that the volume controller has been inactive.
        /// </summary>
        private double TimeInactive { get; set; }

        /// <summary>
        ///     Keeps track of if the volume control box is fading in.
        /// </summary>
        private bool IsFadingIn { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        public VolumeController()
        {
            SliderSize = new Vector2(350, 3);
            Sliders = new List<Slider>();
            SliderIcons = new List<Sprite>();

            Initialize();
        }

        /// <summary>
        ///     Initialize
        /// </summary>
        public void Initialize()
        {
            SurroundingBoxBorder = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(302, 153, 0.1f),
                Alignment = Alignment.TopRight,
                Y = 110,
                X = -50,
                Tint = Colors.MainAccent,
                Alpha = 0,
                SetChildrenVisibility = true,
                SetChildrenAlpha = false
            };

            // Create the surrounding box that will house the sliders.
            SurroundingBox = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(297, 147, 0.1f),
                Alignment = Alignment.TopRight,
                Y = 113,
                X =  -52,
                Tint = new Color(0f, 0f, 0f, 0.85f),
                Alpha = 0,
                SetChildrenVisibility = true,
                SetChildrenAlpha = false
            };

            #region masterVolumeSlider

            // Create master volume slider.
            MasterVolumeSlider = new Slider(ConfigManager.VolumeGlobal, SliderSize, FontAwesome.Get(FontAwesomeIcon.fa_circle))
            {
                Parent = SurroundingBox,
                Alignment = Alignment.TopLeft,
                Y = 30,
                X = 50
            };

            // Create the icon next to the slider.
            MasterVolumeSliderIcon = new Sprite()
            {
                Parent = SurroundingBox,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_volume_up_interface_symbol),
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(25, 25),
                Y = MasterVolumeSlider.Y - 10,
                X = 10
            };

            Sliders.Add(MasterVolumeSlider);
            SliderIcons.Add(MasterVolumeSliderIcon);

            #endregion

            #region musicVolumeSlider

            // Create music volume slider.
            MusicVolumeSlider = new Slider(ConfigManager.VolumeMusic, SliderSize, FontAwesome.Get(FontAwesomeIcon.fa_circle))
            {
                Parent = SurroundingBox,
                Alignment = Alignment.MidLeft,
                Y = 0,
                X = 50
            };

            // Create the icon next to the music slider.
            MusicVolumeSliderIcon = new Sprite()
            {
                Parent = SurroundingBox,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_music_note_black_symbol),
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(25, 25),
                Y = MusicVolumeSlider.Y,
                X = 10
            };

            Sliders.Add(MusicVolumeSlider);
            SliderIcons.Add(MusicVolumeSliderIcon);

            #endregion

            #region effectVolumeSlider

            // Create master volume slider.
            EffectVolumeSlider = new Slider(ConfigManager.VolumeEffect, SliderSize, FontAwesome.Get(FontAwesomeIcon.fa_circle))
            {
                Parent = SurroundingBox,
                Alignment = Alignment.BotLeft,
                Y = -30,
                X = 50
            };

            // Create the icon that's next to the effect volume slider.
            EffectVolumeSliderIcon = new Sprite()
            {
                Parent = SurroundingBox,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_music_headphones),
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(25, 25),
                Y = EffectVolumeSlider.Y + 10,
                X = 10
            };

            Sliders.Add(EffectVolumeSlider);
            SliderIcons.Add(EffectVolumeSliderIcon);

            #endregion

            SurroundingBox.Visible = false;
            FocusedSlider = MasterVolumeSlider;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Handle all needed input.
            HandleInput();

            // Update last volume change timer.
            TimeElapsedSinceLastVolumeChange += dt;

            // As long as the box is visible, start
            if (SurroundingBox.Visible)
                TimeInactive += dt;

            // Handle all fade effects when they need to happen.
            HandleFadeIn(dt);
            HandleInactiveFadeOut(dt);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Handles the overall input of the volume controller. Called every frame.
        /// </summary>
        private void HandleInput()
        {
            // Dictate which slider is the one that is currently focused.
            SetFocusedSlider();
            ChangeFocusedSliderColor();

            // Require either alt key to be pressed when changing volume.
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) && !KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
                return;

            // Activate the volume control box.
            if (KeyboardManager.IsUniqueKeyPress(Keys.Up)|| KeyboardManager.IsUniqueKeyPress(Keys.Down) ||
                KeyboardManager.IsUniqueKeyPress(Keys.Left) || KeyboardManager.IsUniqueKeyPress(Keys.Right)
                || MouseManager.CurrentState.ScrollWheelValue != MouseManager.PreviousState.ScrollWheelValue)
            {
                // Fade in the volume controller box when we want it to become active again.
                IsFadingIn = true;
                TimeInactive = 0;
            }

            // Mouse wheel input.
            if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue)
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(10);
            }
            else if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue)
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(-10);
            }

            // Keyboard input.
            if (KeyboardManager.CurrentState.IsKeyDown(Keys.Right))
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(5);
            }
            else if (KeyboardManager.CurrentState.IsKeyDown(Keys.Left))
            {
                if (TimeElapsedSinceLastVolumeChange >= 50)
                    UpdateVolume(-5);
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
            var focused = Sliders.Find(x => x.MouseInHoldSequence) ?? Sliders.Find(x => x.IsHovered);
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
                SkinManager.Skin.SoundHover.CreateChannel().Play();

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
                SkinManager.Skin.SoundHover.CreateChannel().Play();

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
            TimeElapsedSinceLastVolumeChange = 0;
            TimeInactive = 0;
        }

        /// <summary>
        ///     Makes sure the slider colours highlighted/normal for each slider.
        /// </summary>
        private void ChangeFocusedSliderColor()
        {
            // Change unfocused sliders
            foreach (var slider in Sliders.FindAll(x => x != FocusedSlider).ToList())
                slider.ChangeColor(Color.Gray);

            // Change focused sliders.
            FocusedSlider.ChangeColor(Color.White);
        }

        /// <summary>
        ///    Controls the fadeout of the volume controller after it has been inactive
        ///     for a period of time.
        /// </summary>
        private void HandleInactiveFadeOut(double dt)
        {
            if (TimeInactive >= 1500)
            {
                // Fade out all of the sliders
                SurroundingBox.Alpha = MathHelper.Lerp(SurroundingBox.Alpha, 0, (float) Math.Min(dt / 30, 1));
                SurroundingBoxBorder.Alpha = MathHelper.Lerp(SurroundingBoxBorder.Alpha, 0, (float) Math.Min(dt / 30, 1));

                // Change slider alpha
                foreach (var slider in Sliders)
                {
                    slider.Alpha = MathHelper.Lerp(slider.Alpha, 0, (float) Math.Min(dt / 30, 1));
                    slider.ProgressBall.Alpha = MathHelper.Lerp(slider.ProgressBall.Alpha, 0, (float) Math.Min(dt / 30, 1));
                }

                // Change slider icon alpha.
                foreach (var icon in SliderIcons)
                    icon.Alpha = MathHelper.Lerp(icon.Alpha, 0, (float) Math.Min(dt / 30, 1));

                if (SurroundingBoxBorder.Alpha >= 0.01f)
                    return;

                SurroundingBox.Visible = false;
                TimeInactive = 0;
            }
        }

        /// <summary>
        ///     Controls the FadeIn effect of the volume controller.
        /// </summary>
        /// <param name="dt"></param>
        private void HandleFadeIn(double dt)
        {
            if (!IsFadingIn)
                return;

            // Set the box to be visible of course.
            SurroundingBox.Visible = true;

            // Begin tweening to fade in the box and all of the sliders.
            // Fade out all of the sliders
            SurroundingBox.Alpha = MathHelper.Lerp(SurroundingBox.Alpha, 1f, (float)Math.Min(dt / 30, 1));
            SurroundingBoxBorder.Alpha = MathHelper.Lerp(SurroundingBox.Alpha, 1f, (float) Math.Min(dt / 30, 1));

            // Change slider alpha
            foreach (var slider in Sliders)
            {
                slider.Alpha = MathHelper.Lerp(slider.Alpha, 1, (float)Math.Min(dt / 30, 1));
                slider.ProgressBall.Alpha = MathHelper.Lerp(slider.ProgressBall.Alpha, 1, (float)Math.Min(dt / 30, 1));
            }

            // Change slider icon alpha.
            foreach (var icon in SliderIcons)
                icon.Alpha = MathHelper.Lerp(icon.Alpha, 1, (float)Math.Min(dt / 30, 1));

            // When the box is fully apparent, then we can stop the fade effect.
            if (SurroundingBoxBorder.Alpha >= 0.98)
                IsFadingIn = false;
        }
    }
}
