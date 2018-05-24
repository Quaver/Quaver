using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsMenuBar
    {
        /// <summary>
        ///     The container for the menu bar.
        /// </summary>
        private Sprite Container { get; }

        /// <summary>
        ///     All of the section buttons for each type.
        /// </summary>
        private Dictionary<OptionsType, TextButton> SectionButtons { get; }

        /// <summary>
        ///     Reference to the parent overlay.
        /// </summary>
        private OptionsOverlay Overlay { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="overlay"></param>
        internal OptionsMenuBar(OptionsOverlay overlay)
        {
            Overlay = overlay;
            
            // Create container.
            Container = new Sprite()
            {
                Position = new UDim2D(0, Overlay.Header.Underline.PosY + 60),
                Size = new UDim2D(500, 60),
                Alignment = Alignment.TopCenter,
                Tint = new Color(0f, 0f, 0f, 1f),
                Parent = Overlay,
                Visible = true
            };
            
            SectionButtons = new Dictionary<OptionsType, TextButton>();
        }

         /// <summary>
        ///     Adds a button for a given type.
        /// </summary>
        /// <param name="type"></param>
        internal void AddButton(OptionsType type, string name, Texture2D icon)
         {
             // Create the button
             SectionButtons[type] = new TextButton(new Vector2(70, 30), name)
             {
                Parent = Container,
                Alignment = Alignment.MidLeft,
                PosX = SectionButtons.Count * 100
             };

             // Add a click handler (move to the new section.)
             SectionButtons[type].Clicked += (sender, args) =>
             {
                 GameBase.AudioEngine.PlaySoundEffect(GameBase.LoadedSkin.SoundClick);
                 
                 Overlay.SelectedSection = Overlay.Sections[type];
                 Overlay.RefreshSections();
             };
         }
    }
}