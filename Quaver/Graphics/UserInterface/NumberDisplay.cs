using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface
{
    /// <inheritdoc />
    /// <summary>
    ///     Sprite that displays numbers as textures.
    /// </summary>
    internal class NumberDisplay : QuaverSprite
    {
        /// <summary>
        ///     The number value for this display in string format.
        ///     If the value isn't a valid number or percentage.
        ///
        ///     Sample Inputs:
        ///         - 123
        ///         - 1000000
        ///         - 40.23
        ///         - 99.12%
        /// </summary>
        private string _value;
        internal string Value
        {
            get => _value;
            set
            {
                // Here we run a check to see if the value incoming isn't the same as the
                // already set one. If is then we skip over it. If not, we set the new value,
                // and re-initialize the digits. This is so that we aren't looping more times than
                // we should per frame.
                if (_value == value) 
                    return;
                
                _value = value;
                
                // Only initialize if Digits has already been created.
                if (Digits != null)
                    InitializeDigits();
            } 
        }
        
        /// <summary>
        ///     The type of number display this is.
        /// </summary>
        private NumberDisplayType Type { get; }

        /// <summary>
        ///     Regular expression for all the allowed characters.
        /// </summary>
        private static Regex AllowedCharacters { get; } = new Regex(@"(\d|%|\.)+");

        /// <summary>
        ///     The digits in the number display.
        /// </summary>
        private List<QuaverSprite> Digits { get; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startingValue"></param>
        internal NumberDisplay(NumberDisplayType type, string startingValue)
        {
            Value = startingValue;
            Type = type;
            
            // First validate the initial value to see if everything is correct.
            Validate();
            
            // Create and initialize the digits.
            Digits = new List<QuaverSprite>();
            InitializeDigits();
        }

        /// <summary>
        ///     Validates the current value to see if it is a correct number.
        ///     If it isn't, it'll throw an exception.
        /// </summary>
        private void Validate()
        {
            foreach (var c in Value)
            {
                if (!AllowedCharacters.IsMatch(c.ToString()))
                    throw new ArgumentException($"{c} is not a valid value for NumberDisplay.");             
            }
        }

        /// <summary>
        ///   Goes through each character in the value and either initializes the sprite
        ///   or updates the texture of it.
        /// </summary>
        private void InitializeDigits()
        {
            // Go through each character and either initialize/update the sprite with the correct
            // texture.
            for (var i = 0; i < Value.Length; i++)
            {
                // If the digit doesn't already exist, we need to create it.
                if (i >= Digits.Count)
                {             
                    Digits.Add( new QuaverSprite
                    {
                        Parent = this,
                        Image = CharacterToTexture(Value[i]),
                        Size = new UDim2D(30, 30),
                        PosX = i * 60
                    });
                }
                // If the digit already exists, then we need to just update the texture of it.
                else
                {
                    Digits[i].Image = CharacterToTexture(Value[i]);
                }

                // Reset the sprite to be visible.
                Digits[i].Visible = true;
            }
            
            // Now check if the length of the digits matches the one of the value,
            // if it doesn't then we need to handle some of the extra lost digits.
            if (Value.Length == Digits.Count) 
                return;
            
            // Make the extra ones invisible.
            for (var i = Value.Length; i < Digits.Count; i++)
                Digits[i].Visible = false;
                
        }

        /// <summary>
        ///     Converts a single character into a texture.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static Texture2D CharacterToTexture(char c)
        {
            switch (c)
            {
                case '0':
                    return FontAwesome.Archive;
                case '1':
                    return FontAwesome.CaretDown;
                case '2':
                    return FontAwesome.ChevronDown;
                case '3':
                    return FontAwesome.CircleClosed;
                case '4':
                    return FontAwesome.CircleOpen;
                case '5':
                    return FontAwesome.Cloud;
                case '6':
                    return FontAwesome.Coffee;
                case '7':
                    return FontAwesome.Cog;
                case '8':
                    return FontAwesome.Copy;
                case '9':
                    return FontAwesome.Desktop;
                case '.':
                    return FontAwesome.Discord;
                case '%':
                    return FontAwesome.Volume;
                default:
                    throw new ArgumentException($"Invalid character {c} specified.");
            }
        }
    }

    /// <summary>
    ///     Enum that dictates which type of display it is.
    ///     Some textures are used versus others, so specifying the type here
    ///     allows us to use the correct one.
    /// </summary>
    internal enum NumberDisplayType
    {
        Score,
        Accuracy,
        Combo
    }
}