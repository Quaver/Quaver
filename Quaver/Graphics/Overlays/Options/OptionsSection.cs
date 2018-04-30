using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Steamworks;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsSection
    {        
        /// <summary>
        ///     The stringified name of the section.
        /// </summary>
        internal string Name { get; }

        /// <summary>
        ///     The icon displayed for this section.
        /// </summary>
        internal Texture2D Icon { get; }

        /// <summary>
        ///     The container of the options section.
        /// </summary>
        internal QuaverSprite Container { get; set; }

        /// <summary>
        ///     The y spacing for each section.
        /// </summary>
        internal int SpacingY { get; } = 40;
        
        /// <summary>
        ///     Probably a bad name, but all of the sprites that are interactable
        ///     (Sliders, Dropdowns, Checkboxes)
        /// </summary>
        private List<Drawable> Interactables { get; set; } 
            
        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal OptionsSection(Drawable overlay, string name, Texture2D icon)
        {
            Name = name;
            Icon = icon;
            
            Container = new QuaverSprite()
            {
                Parent = overlay,               
                Size = new UDim2D(800, 450),
                Alignment = Alignment.MidCenter,
                PosY = 90,
                Tint = new Color(0f, 0f, 0f, 1f),
                Visible = false
            };     
            
            Interactables = new List<Drawable>();
        }

        /// <summary>
        ///     Adds a slider to the given container
        /// </summary>
        internal void AddSliderOption(BindedInt value, string name)
        {
            var text = new QuaverTextbox()
            {
                TextAlignment = Alignment.TopLeft,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(20, 20),
                Text = name,
                Parent = Container
            };

            // Create the slider.
            var slider = new QuaverSlider(value, new Vector2(380, 4))
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                PosX = 200,
                PosY = text.PosY + text.SizeY / 2,
                Tint = QuaverColors.MainAccentInactive,
                ProgressBall = { Tint = QuaverColors.MainAccentInactive }
            };

            // Make sure the slider's colors get updated accordingly.
            slider.OnUpdate = dt =>
            {
                if (slider.MouseInHoldSequence || slider.IsHovered)
                {
                    slider.Tint = QuaverColors.MainAccent;
                    slider.ProgressBall.Tint = QuaverColors.MainAccent;
                }
                else
                {
                    slider.Tint = QuaverColors.MainAccentInactive;
                    slider.ProgressBall.Tint = QuaverColors.MainAccentInactive;
                }
            };
            
            Interactables.Add(slider);
        }
    }
}