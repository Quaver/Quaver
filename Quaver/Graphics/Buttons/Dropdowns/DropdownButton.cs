using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Buttons.Dropdowns
{
    internal class DropdownButton : TextButton
    {
        /// <summary>
        ///     Reference to the dropdown this belongs to.
        /// </summary>
        internal Dropdown Dropdown { get; set; }

        /// <summary>
        ///     If this button is the one that's selected. We'll want to perform different actions
        ///     if this is true.
        /// </summary>
        internal bool IsSelected { get; set; }

        /// <summary>
        ///     The chevron icon displayed on the selected dropdown button. 
        /// </summary>
        private Sprite ChevronDownIcon { get; }

        /// <summary>
        ///     The chevron (right facing) icon displayed at the left of the text.
        /// </summary>
        private Sprite ChevronRightIcon { get; }

        /// <summary>
        ///     Overriden property for the clicked event. We pass in the actual button text here.
        /// </summary>
        internal new EventHandler<DropdownButtonClickedEventArgs> Clicked { get; set; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="dropdown"></param>
        /// <param name="buttonSize"></param>
        /// <param name="buttonText"></param>
        internal DropdownButton(Dropdown dropdown, Vector2 buttonSize, string buttonText) : base(buttonSize, buttonText)
        {
            Dropdown = dropdown;
              
            // Set tint + alignment.
            Tint = new Color(0f, 0f, 0f, 0.90f);
            Alignment = Alignment.MidCenter;
            
            // Align the text and set it up properly.
            TextSprite.TextAlignment = Alignment.MidLeft;
            TextSprite.TextColor = Color.White;

            // Add sprite for the chevron displayed at the left of the text.
            ChevronRightIcon = new Sprite()
            {
                Parent = this,
                Image = FontAwesome.ChevronDown,
                SizeY = buttonSize.Y / 3,
                SizeX = buttonSize.Y / 3,
                Alignment = Alignment.MidLeft,
                Rotation = 270,
                PosX = 5,
                Alpha = 0.1f
            };
            
            // Add sprite for the chevron (displayed on the selected dropdown button.)
            ChevronDownIcon = new Sprite()
            {
                Parent = this,
                Image = FontAwesome.ChevronDown,
                SizeY = buttonSize.Y / 2,
                SizeX = buttonSize.Y / 2,
                Alignment = Alignment.MidRight,
                PosX = -10,
                Visible = false
            };
            
            // Set the position of the text, brotha.
            TextSprite.PosX = ChevronRightIcon.PosX + ChevronRightIcon.SizeX + 5;
        }

        internal override void Update(double dt)
        {
            // If we're on the top level and selected, we'll want to make this icon visible.
            if (IsSelected)
                ChevronDownIcon.Visible = true;
            
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        ///     Whenever the button is clicked.
        /// </summary>
        protected override void OnClicked()
        {
            // Play sound effect here.
            GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundClick);
            
            // If this button is the one that's selected, we want to toggle the dropdown open/close
            if (IsSelected)
                ToggleOpen();
            // Otherwise, emit the event as normal.
            else
            {
                // Emit the event.
                Clicked?.Invoke(this, new DropdownButtonClickedEventArgs(TextSprite.Text, Dropdown.Options.IndexOf(this)));
                
                // Swap the indexes in the list.
                ListHelper.Swap(Dropdown.Options, Dropdown.Options.IndexOf(this), Dropdown.Options.FindIndex(x => x.IsSelected));

                // Change this current button to selected
                IsSelected = true;
                
                // Make the old button not selected anymore.
                var oldBtn = Dropdown.Options.Find(x => x.IsSelected && x != this);
                if (oldBtn != null)
                    oldBtn.IsSelected = false;
                
                // Order the list, taking into account that the selected must be the first item.
                Dropdown.Options = Dropdown.Options.OrderByDescending(x => x.IsSelected).ThenBy(x => x.TextSprite.Text).ToList();
            
                // Re-set up the button positions now that it's reordered.
                Dropdown.SetButtonPositions();
                
                // Make sure the dropdown is toggled back to close afterwards.
                ToggleOpen();
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Mouse Out
        /// </summary>
        protected override void MouseOut()
        {
            Tint = new Color(0, 0, 0, 0.90f);
            ChevronRightIcon.Alpha = 0.1f;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Mouse Over
        /// </summary>
        protected override void MouseOver()
        {
            Tint = QuaverColors.MainAccent;
            Alpha = 0.90f;
            ChevronRightIcon.Alpha = 1f;
        }
        
        /// <summary>
        ///     Called whenever the user clicks the top level button and we want to toggle the display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleOpen()
        {
            Dropdown.IsOpen = !Dropdown.IsOpen;

            // If the dropdown is open, display all of the non-selected onces.
            if (Dropdown.IsOpen)
                Dropdown.Options.ForEach(x => x.Visible = true);
            // Otherwise, hide every button except for the selected one.
            else
                Dropdown.Options.FindAll(x => !x.IsSelected).ToList().ForEach(x => x.Visible = false);            
        }

        /// <summary>
        ///     Properly sets the icons of this button.
        /// </summary>
        internal void SetIconVisibility()
        {
            if (!IsSelected)
                ChevronDownIcon.Visible = false;
        }
    }

    /// <summary>
    ///     EventArgs, emitted when the user clicks a dropdown button.
    /// </summary>
    internal class DropdownButtonClickedEventArgs : EventArgs
    {
        /// <summary>
        ///     The text of the button that got clicked
        /// </summary>
        internal string ButtonText { get; }

        /// <summary>
        ///     The index of the button that was clicked.
        /// </summary>
        internal int Index { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="text"></param>
        internal DropdownButtonClickedEventArgs(string text, int index)
        {
            ButtonText = text;
            Index = index;
        }
    }
}