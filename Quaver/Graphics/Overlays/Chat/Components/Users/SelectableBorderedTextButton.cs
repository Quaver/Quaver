using System;
using Microsoft.Xna.Framework;
using Quaver.Screens.Menu.UI.Navigation.User;

namespace Quaver.Graphics.Overlays.Chat.Components.Users
{
    public class SelectableBorderedTextButton : BorderedTextButton
    {
        /// <summary>
        ///     The color of the button when it is deselected.
        /// </summary>
        public Color DeselectedColor { get; set; }

        /// <summary>
        ///     The color of the button when it is selected
        /// </summary>
        public Color SelectedColor { get; set; } = Color.White;

        /// <summary>
        ///     Determines if the button is currently "selected"
        /// </summary>
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;

                if (_selected)
                    OriginalColor = SelectedColor;
                else
                    OriginalColor = DeselectedColor;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        public SelectableBorderedTextButton(string text, Color color, bool selected, EventHandler clickAction = null)
            : base(text, color, clickAction)
        {
            DeselectedColor = color;
            Selected = selected;
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="deselectedColor"></param>
        /// <param name="selectedColor"></param>
        /// <param name="selected"></param>
        /// <param name="clickAction"></param>
        public SelectableBorderedTextButton(string text, Color deselectedColor, Color selectedColor, bool selected, EventHandler clickAction = null)
            : base(text, deselectedColor, clickAction)
        {
            DeselectedColor = deselectedColor;
            SelectedColor = selectedColor;
            Selected = selected;
        }
    }
}