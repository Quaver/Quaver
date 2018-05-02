using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Config;
using Quaver.Graphics.Buttons.Dropdowns;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Logging;
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
            Tint = new Color(0f, 0f, 0f, 0.7f);
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
            SelectedSection = Sections[OptionsType.Video];   
            
            // Create menu bar.
            MenuBar = new OptionsMenuBar(this);
            
            // Create all of the options sections.
            // CreateAudioSection();
            CreateVideoSection();
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
        ///     Adds interactable config options for the Audio section.
        /// </summary>
        private void CreateAudioSection()
        {         
            var section = Sections[OptionsType.Audio];
             
            section.AddSliderOption(ConfigManager.VolumeGlobal, "Master Volume"); 
            section.AddSliderOption(ConfigManager.VolumeMusic, "Music Volume");
            section.AddSliderOption(ConfigManager.VolumeEffect, "Effect Volume");
            section.AddSliderOption(ConfigManager.GlobalAudioOffset, "Audio Offset");
            section.AddCheckboxOption(ConfigManager.Pitched, "Toggle Music Pitching");
        }

        /// <summary>
        ///     Adds interactable config options for the Video section.
        /// </summary>
        private void CreateVideoSection()
        {
            var section = Sections[OptionsType.Video];
            
            // Resolution Dropdown
            section.AddDropdownOption(CreateResolutionDropdown(), "Resolution");
            
            // Fullscreen
            section.AddCheckboxOption(ConfigManager.WindowFullScreen, "Fullscreen", (o, e) =>
            {
                QuaverGame.ChangeWindow(ConfigManager.WindowFullScreen.Value, ConfigManager.WindowLetterboxed.Value);
                BackgroundManager.Readjust();
            });
            
            // Letterboxing
            section.AddCheckboxOption(ConfigManager.WindowLetterboxed, "Letterboxing", (o, e) =>
            {
                QuaverGame.ChangeWindow(ConfigManager.WindowFullScreen.Value, ConfigManager.WindowLetterboxed.Value);
                BackgroundManager.Readjust();
            });
            
            // FPS Counter
            section.AddCheckboxOption(ConfigManager.FpsCounter, "Display FPS Counter");
            
            // Background Brightness
            section.AddSliderOption(ConfigManager.BackgroundBrightness, "Background Brightness");
        }

        /// <summary>
        ///     Creates the dropdown to select the current resolution.
        /// </summary>
        private QuaverDropdown CreateResolutionDropdown()
        {
            // Create a list of the most common resolutions
            var commonResolutions = new List<Point>
            {
                new Point(800, 600), 
                new Point(1024, 768), 
                new Point(1152, 864), 
                new Point(1280, 960), 
                new Point(1024, 600),
                new Point(1280, 720), 
                new Point(1366, 768), 
                new Point(1440, 900), 
                new Point(1600, 900), 
                new Point(1680, 1080)
            };

            // If the user's current resolution isn't in the list 
            var selected = commonResolutions.Find(x => x.X == ConfigManager.WindowWidth.Value && x.Y == ConfigManager.WindowHeight.Value);
                
            // Since "Point" is a struct, it can't be null, so we check if it's (0,0) instead since that's the default.
            if (selected == Point.Zero)
            {
                // Add the current resolution
                commonResolutions.Add(new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value));
                
                // Change the selected resolution to the correct one.
                selected = commonResolutions.Last();
            }
          
            // Create a list of options <string> from the current resolution (required to create a dropdown).
            var options = new List<string>();
            commonResolutions.ForEach(x => options.Add($"{x.X}x{x.Y}"));
           
            // Swap the index of if the selected index so that the currently selected resolution is the first
            // option in the dropdown.
            ListHelper.Swap(options, commonResolutions.IndexOf(selected), 0);
            options = options.OrderByDescending(x => x == $"{selected.X}x{selected.Y}").ThenBy(x => Convert.ToInt32(x.Split('x')[0])).ToList();
            
            // Create and return the new dropdown.
            return new QuaverDropdown(options, (o, e) =>
            {
                // Split the given resolution.
                var resSplit = e.ButtonText.Split('x');
               
                // Change the game window's resolution.
                QuaverGame.ChangeWindow(ConfigManager.WindowFullScreen.Value, ConfigManager.WindowLetterboxed.Value, new Point(Convert.ToInt32(resSplit[0]), Convert.ToInt32(resSplit[1])));
       
                // Recalculate the size of the overlay.
                Size = new UDim2D(0, GameBase.WindowRectangle.Height, 1);          
                GameBase.GameOverlay.RecalculateWindow();
                BackgroundManager.Readjust();             
            });
        }
    }
}