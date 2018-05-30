using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Sprites;

namespace Quaver.Graphics.Buttons.Dropdowns
{
    internal class Dropdown : Sprite
    {
        /// <summary>
        ///     The options for the dropdown buttons.
        /// </summary>
        internal List<DropdownButton> Options { get; set; }

        /// <summary>
        ///     The list of options strings.
        /// </summary>
        internal List<string> OptionsStringList { get; set; }

        /// <summary>
        ///     If the dropdown is currently open or closed.
        /// </summary>
        internal bool IsOpen { get; set; }
        
        /// <summary>
        ///     The size of each button.
        /// </summary>
        private Vector2 ButtonSize { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        internal Dropdown(List<string> options, EventHandler<DropdownButtonClickedEventArgs> onClick, int width = 380, uint selectedIndex = 0)
        {
            // Only allow correct selected button.
            if (selectedIndex > options.Count - 1)
                throw new ArgumentException("Selected dropdown index must not be greater than the size of the options.");
                   
            // Set options lists
            Options = new List<DropdownButton>();
            OptionsStringList = options;
            
            // Set button size.
            SizeX = width;
            ButtonSize = new Vector2(width, 35);

            // Add selected buttons for each.
            foreach (var option in options)
            {
                // Create the new button
                var btn = new DropdownButton(this, ButtonSize, option)
                {
                    Parent = this,
                    IsSelected = options.IndexOf(option) == selectedIndex
                };

                btn.Visible = btn.IsSelected;
                                
                // Add click handler.
                btn.Clicked += onClick;
                
                // Add button option to list.
                Options.Add(btn);
            }
            
            // Set up the button positions.
            SetButtonPositions();
        }

        /// <summary>
        ///     Set up all the buttons with the correct positioning and such.
        /// </summary>
        internal void SetButtonPositions()
        {
            for (var i = 0; i < Options.Count; i++)
            {
                // The position of the first index is always 0.
                if (i == 0)
                {
                    Options[i].PosY = 0;
                    continue;
                }

                // Add the correct y offset for all other dropdown buttons other than the first.
                Options[i].PosY = Options[i - 1].PosY + Options[i - 1].SizeY;
                
                // Set the correct icon visiblility
                Options[i].SetIconVisibility();
            }
        }
    }
}