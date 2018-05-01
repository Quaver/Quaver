using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics.Base;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Buttons.Dropdowns;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
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
        internal QuaverSprite Container { get; }
        
        /// <summary>
        ///     Probably a bad name, but all of the sprites that are interactable
        ///     (Sliders, Dropdowns, Checkboxes)
        /// </summary>
        private List<Drawable> Interactables { get; }

        /// <summary>
        ///     The y spacing for each options element.
        /// </summary>
        private int SpacingY => Interactables.Count * 50;
        
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
                Size = new UDim2D(650, 450),
                Alignment = Alignment.MidCenter,
                PosY = 100,
                Tint = new Color(0f, 0f, 0f, 0f),
                Visible = false
            };     
            
            Interactables = new List<Drawable>();
        }

        /// <summary>
        ///     Adds a slider to the given container
        /// </summary>
        internal void AddSliderOption(BindedInt value, string name)
        {
            AddTextField(name);
            
            // Create the slider.
            var slider = new QuaverSlider(value, new Vector2(380, 3))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                PosY = SpacingY + 8,
                Tint = QuaverColors.MainAccentInactive,
                ProgressBall = { Tint = QuaverColors.MainAccentInactive }
            };

            // Make sure the slider's colors get updated accordingly.
            slider.OnUpdate = dt =>
            {
                if (slider.MouseInHoldSequence)
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

        /// <summary>
        ///     Adds a checkbox option 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        internal void AddCheckboxOption(BindedValue<bool> value, string name)
        {
            AddTextField(name);

            // Create the checkbox.
            var checkbox = new QuaverCheckbox(value, new Vector2(20, 20))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                PosY = SpacingY,
                Tint = QuaverColors.MainAccentInactive,
            };
            
            Interactables.Add(checkbox);
        }

        /// <summary>
        ///     Adds a dropdown option 
        /// </summary>
        internal void AddDropdownOption(QuaverDropdown dropdown, string name)
        {         
            AddTextField(name);

            dropdown.Parent = Container;
            dropdown.Alignment = Alignment.TopRight;
            dropdown.PosY = SpacingY  + 8;
            dropdown.SizeX = 380;
            
            Interactables.Add(dropdown);
        }

        /// <summary>
        ///     Adds a single keybind option
        /// </summary>
        /// <param name="key"></param>
        /// <param name="name"></param>
        internal void AddKeybindOption(BindedValue<Keys> value, string name)
        {
            AddTextField(name);

            // Create the keybind button.
            var keybind = new QuaverKeybindButton(value, new Vector2(90, 25))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                PosY = SpacingY
            };
            
            Interactables.Add(keybind);
        }

        /// <summary>
        ///     Adds multiple keybind buttons on the same row.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="name"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void AddKeybindOption(List<BindedValue<Keys>> values, string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Method that adds the text field to the left for each options element.
        /// </summary>
        /// <param name="text"></param>
        private void AddTextField(string text)
        {
            new QuaverTextbox()
            {
                TextAlignment = Alignment.TopLeft,
                Alignment = Alignment.TopLeft,
                Text = text,
                Font = QuaverFonts.Medium12,
                Parent = Container,
                TextBoxStyle = TextBoxStyle.OverflowSingleLine,
                PosY = SpacingY
            };
        }
    }
}