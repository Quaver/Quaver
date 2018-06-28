using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Graphics.Colors;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.Graphics.Buttons.Selection
{
    internal class HorizontalSelector : Sprite
    {
        /// <summary>
        ///     The items that are currently in the selector.
        /// </summary>
        internal List<string> Items { get; }

        /// <summary>
        ///     The index of the selected element.
        /// </summary>
        internal int SelectedIndex { get; private set; }

        /// <summary>
        ///     The text that shows what the selected item is.
        /// </summary>
        private SpriteText SelectedItemText { get;  }

        /// <summary>
        ///     Selects the option to the left.
        /// </summary>
        private BasicButton ButtonSelectLeft { get; }

        /// <summary>
        ///     Selects the option to the right.
        /// </summary>
        private BasicButton ButtonSelectRight { get; }

        /// <summary>
        ///     When the value changes, this is the method that will be called
        ///
        ///     Parameters:
        ///         string: The new value
        ///         int: The new integer.
        /// </summary>
        private Action<string, int> OnChange { get; }

        /// <summary>
        ///     Ctor 
        /// </summary>
        /// <param name="items"></param>
        /// <param name="size"></param>
        /// <param name="onChange"></param>
        /// <param name="selectedIndex"></param>
        internal HorizontalSelector(List<string> items, Vector2 size, Action<string, int> onChange, int selectedIndex = 0)
        {
            Items = items;
            
            if (Items.Count == 0)
                throw new ArgumentException("HorizontalSelector items must be greater than 0");

            OnChange = onChange;
            SelectedIndex = selectedIndex;
          
            Size = new UDim2D(size.X, size.Y);
            Tint = QuaverColors.NegativeInactive;

            SelectedItemText = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                TextAlignment = Alignment.MidCenter,
                Font = QuaverFonts.AssistantRegular16,
                Text = Items[SelectedIndex],
                TextColor = Color.White,
                TextScale = 0.85f
            };

            ButtonSelectLeft = new BasicButton
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new UDim2D(SizeY, SizeY),
                Image = FontAwesome.ChevronSignLeft,
                PosX = -SizeY - 10
            };

            ButtonSelectLeft.Clicked += (sender, e) => HandleSelection(SelectorDirection.Left);
            
            ButtonSelectRight = new BasicButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new UDim2D(SizeY, SizeY),
                Image = FontAwesome.ChevronSignRight,
                PosX = SizeY + 10
            };

            ButtonSelectRight.Clicked += (sender, e) => HandleSelection(SelectorDirection.Right);
        }

        /// <summary>
        ///     Handles the selection of the next element based on the direction.
        /// </summary>
        /// <param name="direction"></param>
        private void HandleSelection(SelectorDirection direction)
        {
            GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
            
            // Choose the newly selected index based on the direction we're going.
            switch (direction)
            {
                case SelectorDirection.Left:
                    if (SelectedIndex - 1 < Items.Count && SelectedIndex - 1 > 0)
                        SelectedIndex -= 1;
                    else
                        SelectedIndex = Items.Count - 1;
                    break;
                case SelectorDirection.Right:
                    if (SelectedIndex + 1 < Items.Count)
                        SelectedIndex += 1;
                    else
                        SelectedIndex = 0;                 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
            
            SelectedItemText.Text = Items[SelectedIndex];
            OnChange(Items[SelectedIndex], SelectedIndex);
        }
    }

    /// <summary>
    ///     The direction in which to select the next element.
    /// </summary>
    public enum SelectorDirection
    {
        Left,
        Right
    }
}