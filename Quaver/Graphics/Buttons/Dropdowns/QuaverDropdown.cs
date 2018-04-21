using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;

namespace Quaver.Graphics.Buttons.Dropdowns
{
    internal class QuaverDropdown : QuaverSprite
    {
        /// <summary>
        ///     The options for the dropdown buttons.
        /// </summary>
        internal List<QuaverDropdownButton> Options { get; set; }

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
        internal QuaverDropdown(List<string> options, EventHandler<DropdownButtonClickedEventArgs> onClick, int width = 300, uint selectedIndex = 0)
        {
            // Only allow correct selected button.
            if (selectedIndex > options.Count - 1)
                throw new ArgumentException("Selected dropdown index must not be greater than the size of the options.");
                   
            // Set options lists
            Options = new List<QuaverDropdownButton>();
            OptionsStringList = options;
            
            // Set button size.
            SizeX = width;
            ButtonSize = new Vector2(width, 35);

            // Add selected buttons for each.
            foreach (var option in options)
            {
                // Create the new button
                var btn = new QuaverDropdownButton(this, ButtonSize, option)
                {
                    Parent = this,
                    IsSelected = options.IndexOf(option) == selectedIndex
                };

                btn.Visible = btn.IsSelected;
                
                // Set the position of the button accordingly.
                if (Options.Count == 0)
                    PosY = 0;
                else
                {
                    var last = Options.Last();
                    btn.PosY = last.PosY + last.SizeY;
                }
                
                // Add click handler.
                btn.Clicked += onClick;
                
                // Add button option to list.
                Options.Add(btn);
            }
        }
    }
}