using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.Options
{
    internal class OptionsOverlay : QuaverSprite
    {
        /// <summary>
        ///     If the options overlay is currently active.
        /// </summary>
        internal bool Active { get; set; }

        /// <summary>
        ///     The header of the options menu.
        /// </summary>
        internal OptionsHeader Header { get; }

        /// <summary>
        ///     The menu bar to switch through different options sections.
        /// </summary>
        internal OptionsMenuBar MenuBar { get; }

        /// <summary>
       ///    All of the defined options section that'll be displayed on-screen.    
       /// </summary>
        internal SortedDictionary<OptionsType, OptionsSection> Sections { get; }

        /// <summary>
        ///     The currently selected options section.
        /// </summary>
        internal OptionsSection SelectedSection { get; }
    
        /// <summary>
       ///     Ctor - 
       /// </summary>
        internal OptionsOverlay()
        {
            Alignment = Alignment.TopLeft;
            Tint = new Color(0f, 0f, 0f, 0.6f);
            Size = new UDim2D(0, GameBase.WindowRectangle.Height, 1);
            PosY = GameBase.WindowRectangle.Height;
                        
            // Create the entire header's UI.
            Header = new OptionsHeader(this);
            
            // Create the options sections.
            Sections = new SortedDictionary<OptionsType, OptionsSection>
            {
                {OptionsType.Audio, new OptionsSection(this, "Audio", FontAwesome.Volume)},
                {OptionsType.Gameplay, new OptionsSection(this, "Gameplay", FontAwesome.GamePad)},
                {OptionsType.Video, new OptionsSection(this, "Video", FontAwesome.Desktop)},
                {OptionsType.Misc, new OptionsSection(this, "Misc", FontAwesome.GiftBox)}
            };
            
            // Default the selected section to audio.
            SelectedSection = Sections[OptionsType.Audio];   
            
            // Create menu bar.
            MenuBar = new OptionsMenuBar(this);
            
            // Create all of the options sections.
            CreateAudioSection();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Update
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            PerformShowAndHideAnimations(dt);
            Header.Update(dt);

            SelectedSection.Container.Visible = true;
       
            base.Update(dt);
        }

        /// <summary>
       ///     Performs a show and hide animation on the overlay
       /// </summary>
       /// <param name="dt"></param>
        private void PerformShowAndHideAnimations(double dt)
        {
            if (Active)
                PosY = GraphicsHelper.Tween(0, PosY, Math.Min(dt / 30, 1));
            else
                PosY = GraphicsHelper.Tween(GameBase.WindowRectangle.Height, PosY, Math.Min(dt / 30, 1));
        }

         /// <summary>
        ///     
        /// </summary>
        private void CreateAudioSection()
         {
              
            var section = Sections[OptionsType.Audio];
             
            section.AddSliderOption(ConfigManager.VolumeGlobal, "Master Volume"); 
            section.AddSliderOption(ConfigManager.VolumeMusic, "Music Volume");
            section.AddSliderOption(ConfigManager.VolumeEffect, "Effect Volume");
            section.AddSliderOption(ConfigManager.GlobalAudioOffset, "Offset");
        }
    }
}