using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NAudio.Wave;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Buttons.Dropdowns;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Skinning;
using Keys = Microsoft.Xna.Framework.Input.Keys;

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
        internal OptionsSection SelectedSection { get; set;  }
    
        /// <summary>
       ///     Ctor - 
       /// </summary>
        internal OptionsOverlay()
        {
            Alignment = Alignment.TopLeft;
            Tint = new Color(0f, 0f, 0f, 0.9f);
            Size = new UDim2D(0, GameBase.WindowRectangle.Height, 1);
            PosY = GameBase.WindowRectangle.Height;
                        
            // Create the entire header's UI.
            Header = new OptionsHeader(this);
            
            // Create menu bar.
            MenuBar = new OptionsMenuBar(this);
            
            // Create the options sections.
            Sections = new SortedDictionary<OptionsType, OptionsSection>();
            
            // Add all of the options sections.
            AddSection(OptionsType.Audio, "Audio", FontAwesome.Volume, CreateAudioSection);
            AddSection(OptionsType.Video, "Video", FontAwesome.Desktop, CreateVideoSection);
            AddSection(OptionsType.Gameplay, "Gameplay", FontAwesome.GamePad, CreateGameplaySection);
            AddSection(OptionsType.Keybinds, "Keybinds", FontAwesome.GamePad, CreateKeybindsSection);
            AddSection(OptionsType.Misc, "Misc", FontAwesome.GiftBox, CreateMiscSection);
            
            // Default the selected section
            SelectedSection = Sections[OptionsType.Audio];   
            RefreshSections();
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
            {
                PosY = GraphicsHelper.Tween(0, PosY, Math.Min(dt / 180, 1));
                Alpha = GraphicsHelper.Tween(0.9f, Alpha, Math.Min(dt / 360, 1));
            }
            else
            {
                PosY = GraphicsHelper.Tween(GameBase.WindowRectangle.Height, PosY, Math.Min(dt / 180, 1));
                Alpha = GraphicsHelper.Tween(0, Alpha, Math.Min(dt / 360, 1));
            }
        }

        /// <summary>
        ///     Creates a section for a given type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <param name="createSection"></param>
        private void AddSection(OptionsType type, string name, Texture2D icon, Action createSection)
        {
            Sections[type] = new OptionsSection(type, this, name, icon);
            createSection();    
            MenuBar.AddButton(type, name, icon);
        }
        
        /// <summary>
        ///     Refreshes the overlay (sets all inactives to invisible, and sets th)
        /// </summary>
        internal void RefreshSections()
        {
            foreach (var item in Sections)
            {
                if (item.Value == SelectedSection)
                    continue;
                
                item.Value.Container.Visible = false;
                item.Value.Container.PosX = -500;
            }
            
            SelectedSection.Container.PosX = 0;
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
            section.AddCheckboxOption(ConfigManager.Pitched, "Toggle Music Pitching", (o ,e) => GameBase.AudioEngine.SetPitch());
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
       ///     Creates the gameplay section of the options menu.
       /// </summary>
        private void CreateGameplaySection()
        {
            var section = Sections[OptionsType.Gameplay];
            
            section.AddDropdownOption(CreateDefaultSkinDropdown(), "Default Skin");   
            section.AddDropdownOption(CreateSkinDropdown(), "Custom Skin");
            section.AddSliderOption(ConfigManager.ScrollSpeed4K, "Scroll Speed - 4 Keys");
            section.AddSliderOption(ConfigManager.ScrollSpeed7K, "Scroll Speed - 7 Keys");
            section.AddCheckboxOption(ConfigManager.HealthBarPositionTop, "Health Bar On Top");
            section.AddCheckboxOption(ConfigManager.DisplaySongTimeProgress, "Display Song Time Progress");
            section.AddCheckboxOption(ConfigManager.DownScroll4K, "Down Scroll - 4 Keys");
            section.AddCheckboxOption(ConfigManager.DownScroll7K, "Down Scroll - 7 Keys");
        }

        /// <summary>
        ///     Creates the keybinds section of the options menu.
        /// </summary>
        private void CreateKeybindsSection()
        {
            var section = Sections[OptionsType.Keybinds];
            
            section.AddKeybindOption(new List<BindedValue<Keys>>
            {
                ConfigManager.KeyMania4K1,
                ConfigManager.KeyMania4K2,
                ConfigManager.KeyMania4K3,
                ConfigManager.KeyMania4K4
            }, "Mania - 4 Keys");
            
            section.AddKeybindOption(new List<BindedValue<Keys>>
            {
                ConfigManager.KeyMania7K1,
                ConfigManager.KeyMania7K2,
                ConfigManager.KeyMania7K3,
                ConfigManager.KeyMania7K4,
                ConfigManager.KeyMania7K5,
                ConfigManager.KeyMania7K6,
                ConfigManager.KeyMania7K7
            }, "Mania - 7 Keys");  
            
            section.AddKeybindOption(ConfigManager.KeyPause, "Pause");
            section.AddKeybindOption(ConfigManager.KeySkipIntro, "Skip Song Intro");
            section.AddKeybindOption(ConfigManager.KeyTakeScreenshot, "Take Screenshot");
            section.AddKeybindOption(ConfigManager.KeyToggleOverlay, "Toggle Overlay");
            section.AddKeybindOption(ConfigManager.KeyRestartMap, "Quick Restart Map");
        }

        /// <summary>
        ///     Creates the Misc section of the options menu.
        /// </summary>
        private void CreateMiscSection()
        {
            var section = Sections[OptionsType.Misc];

            #region Peppy

            // Create the peppy button.
            var peppyButton = new QuaverTextButton(Vector2.One, "Select peppy!.db File");

            // Add click handler for peppy button.
            peppyButton.Clicked += (o, e) =>
            {
                // Create the openFileDialog object.
                var openFileDialog = new OpenFileDialog()
                {
                    InitialDirectory = "c:\\",
                    Filter = "Peppy Database File (*.db)| *.db",
                    FilterIndex = 0,
                    RestoreDirectory = true,
                    Multiselect = false
                };

                // If the dialog couldn't be shown, that's an issue, so we'll return for now.
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                /* Ignore bad file names.
                if (openFileDialog.FileName != "osu!.db")
                {
                    Logger.LogError("Invalid File", LogType.Runtime);
                    return;
                }
               */
                
                ConfigManager.OsuDbPath.Value = openFileDialog.FileName;
            };
            
            // Add peppy button.
            section.AddButton(peppyButton, "Select peppy!.db File");
            
            // Add option to load peppy beatmaps.
            section.AddCheckboxOption(ConfigManager.AutoLoadOsuBeatmaps, "Load \"peppy!\" Beatmaps", (o, e) =>
            {
                // After initializing the configuration, we want to sync the map database, and load the dictionary of mapsets.
                Task.Run(async () =>
                {
                    await MapCache.LoadAndSetMapsets();

                    // Force garbage collection
                    GC.Collect();
                }).Wait();
            });

            #endregion
            
            #region Etterna

            // Create the peppy button.
            var etternaButton = new QuaverTextButton(Vector2.One, "Etterna Cache Folder");

            // Add click handler for peppy button.
            etternaButton.Clicked += (o, e) =>
            {
                using(var fbd = new FolderBrowserDialog())
                {
                    var result = fbd.ShowDialog();

                    ConfigManager.EtternaCacheFolderPath.Value = fbd.SelectedPath;
                }
            };
            
            // Add etterna button.
            section.AddButton(etternaButton, "Select Etterna Cache Folder");
            
            // Add option to load Etterna charts.
            section.AddCheckboxOption(ConfigManager.AutoLoadEtternaCharts, "Load Etterna Charts", (o, e) =>
            {
                // After initializing the configuration, we want to sync the map database, and load the dictionary of mapsets.
                Task.Run(async () =>
                {
                    await MapCache.LoadAndSetMapsets();

                    // Force garbage collection
                    GC.Collect();
                }).Wait();
            });

            #endregion
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
                BackgroundManager.Readjust();             
            });
        }

        /// <summary>
        ///     Creates the dropdown to select the default skin.
        /// </summary>
        /// <returns></returns>
        private QuaverDropdown CreateDefaultSkinDropdown()
        {
            // Create constant variables for both default skin options.
            const string arrowText = "Default Arrow Skin";
            const string barText = "Default Bar Skin";
            
            var options = new List<string> { arrowText, barText };

            switch (ConfigManager.DefaultSkin.Value)
            {
                case DefaultSkins.Arrow:
                    break;
                case DefaultSkins.Bar:
                    ListHelper.Swap(options, 1, 0);
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }
            
            // Create the new dropdown button.
            return new QuaverDropdown(options, (o, e) =>
            {
                switch (e.ButtonText)
                {
                    case arrowText:
                        ConfigManager.DefaultSkin.Value = DefaultSkins.Arrow;
                        Skin.LoadSkin();
                        break;
                    case barText:
                        ConfigManager.DefaultSkin.Value = DefaultSkins.Bar;
                        Skin.LoadSkin();
                        break;
                }
            });
        }

        /// <summary>
        ///     Creates the dropdown to select a skin.
        /// </summary>
        /// <returns></returns>
        private QuaverDropdown CreateSkinDropdown()
        {
            // The text for the default option if the user doesn't have any skins.
            const string defaultText = "Default - Import more skins!";

            var skins = Directory.GetDirectories(ConfigManager.SkinDirectory.Value).ToList();
            for (var i = 0; i < skins.Count; i++)
                skins[i] = new DirectoryInfo(skins[i]).Name;
            
            if (skins.Count == 0)
                skins.Add(defaultText);
            
            // Create the dropdown
            return new QuaverDropdown(skins, (o, e) =>
            {
                if (e.ButtonText == defaultText) 
                    return;
                
                ConfigManager.Skin.Value = e.ButtonText;
                Skin.LoadSkin();
            });
        }
    }
}