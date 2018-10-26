using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Resources;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Screens.Options.Keybindings;
using Quaver.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Options
{
    public class OptionsDialog : DialogScreen
    {
        /// <summary>
        ///     The background in which the content lives.
        /// </summary>
        public Sprite ContentContainer { get; private set; }

        #region HEADER

        /// <summary>
        ///     The container for the header of the options menu
        /// </summary>
        public Sprite HeaderContainer { get; private set; }

        /// <summary>
        ///     A flag icon displayed at the left of the header for stylistic purposes
        /// </summary>
        private Sprite HeaderFlag { get; set; }

        /// <summary>
        ///     The text for the header
        /// </summary>
        private SpriteText HeaderText { get; set; }

        /// <summary>
        ///     The button to exit the dialog.
        /// </summary>
        private ImageButton ExitButton { get; set; }

        #endregion

        #region FOOTER

        /// <summary>
        ///     The container for the footer of the options menu.
        /// </summary>
        private Sprite FooterContainer { get; set; }

        /// <summary>
        ///     The button to confirm changes for the options.
        /// </summary>
        private TextButton FooterOkButton { get; set; }

        /// <summary>
        ///     The button to cancel and close down the options menu.
        /// </summary>
        private TextButton FooterCancelButton { get; set; }

        #endregion

        #region SECTIONS

        /// <summary>
        ///     The list of options sections.
        /// </summary>
        public List<OptionsSection> Sections { get; private set; }

        /// <summary>
        ///     The index of the currently selected section.
        /// </summary>
        private OptionsSection SelectedSection { get; set; }

        /// <summary>
        ///     A vertical divider line to separate the section buttons from the actual section.
        /// </summary>
        public Sprite DividerLineVertical { get; private set; }

        #endregion

        /// <summary>
        ///     If not equivalent to the current window's resolution,
        ///     it will change to this one once the user presses OK.
        /// </summary>
        private Point ChangedResolution { get; set; }

        /// <summary>
        ///     if this is set to something other than the skin the user
        ///     currently has, it will reload the skin.
        /// </summary>
        private string NewSkinToLoad { get; set; }

        /// <summary>
        ///     If this is set to something other than our current default skin,
        ///     it'll reload the skin when the user presses OK
        /// </summary>
        private DefaultSkins NewDefaultSkinToLoad { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="backgroundAlpha"></param>
        public OptionsDialog(float backgroundAlpha) : base(backgroundAlpha) => CreateContent();

        /// <summary>
        ///     Change the selected section.
        /// </summary>
        /// <param name="section"></param>
        public void ChangeSection(OptionsSection section)
        {
            var oldSelected = SelectedSection;

            // Make sure the current icon is not selected anymore.
            oldSelected.Icon.Deselect();

            // Move the container off-screen.
            oldSelected.Container.X = WindowManager.Width;

            // Change the selected button
            SelectedSection = section;

            // Make the icon button in a selected state
            SelectedSection.Icon.Select();
            SelectedSection.Container.Position = new ScalableVector2(0, 0);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            // Set default values that require an "OK" in order to change.
            ChangedResolution = new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);
            NewSkinToLoad = ConfigManager.Skin.Value;
            NewDefaultSkinToLoad = ConfigManager.DefaultSkin.Value;

            // Create the container for the entire options menu.
            ContentContainer = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width / 1.4f, WindowManager.Height / 1.4f),
                Tint = Color.LightSlateGray,
            };

            CreateHeader();
            CreateFooter();

            // Create all of the options sections.
            Sections = new List<OptionsSection>()
            {
                CreateVideoSection(),
                CreateAudioSection(),
                CreateGameplaySection(),
                CreateKeybindSection(),
                CreateMiscSection()
            };

            CreateSections();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss();
        }

        /// <summary>
        ///     Creates the header for the options menu.
        /// </summary>
        private void CreateHeader()
        {
            HeaderContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 45),
                Tint = Color.Black
            };

            HeaderFlag = new Sprite()
            {
                Parent = HeaderContainer,
                Size = new ScalableVector2(5, HeaderContainer.Height),
                Tint = Color.LightGray
            };

            HeaderText = new SpriteText(BitmapFonts.Exo2Regular, "Options Menu", 22)
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = HeaderFlag.X + 5
            };


            ExitButton = new ImageButton(FontAwesome.Get(FontAwesomeIcon.fa_times), (sender, args) => DialogManager.Dismiss())
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(25, 25)
            };

            ExitButton.X -= ExitButton.Width / 2f + 5;
        }

        /// <summary>
        ///     Creates the footer of the options dialog.
        /// </summary>
        private void CreateFooter()
        {
            FooterContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 50),
                Tint = Colors.DarkGray,
                Alignment = Alignment.BotLeft,
                Y = 1
            };

            FooterOkButton = new TextButton(UserInterface.BlankBox, BitmapFonts.Exo2Regular, "OK", 18,
                (sender, args) => ConfirmChanges())
            {
                Parent = FooterContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(200, 30),
                X = -20,
                Tint = Colors.SecondaryAccent,
                Text =
                {
                    Tint = Color.Black
                }
            };

            FooterCancelButton = new TextButton(UserInterface.BlankBox, BitmapFonts.Exo2Regular, "Cancel", 18,
                (sender, args) => DialogManager.Dismiss())
            {
                Parent = FooterContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(200, 30),
                X = FooterOkButton.X - FooterOkButton.Width - 20,
                Tint = Colors.SecondaryAccent,
                Text =
                {
                    Tint = Color.Black
                }
            };
        }

        /// <summary>
        ///     Creates the options sections.
        /// </summary>
        private void CreateSections()
        {
            CreateSectionButtons();
            CreateVerticalDividerLine();

            // Set the selected section.
            SelectedSection = Sections.First();

            Sections.ForEach(section =>
            {
                for (var i = 0; i < section.Items.Count; i++)
                {
                    // Set the item's parent to the section container.
                    section.Items[i].Parent = section.Container;

                    // However keep the parent of the icon as the overall ContentContainer
                    // so that it still displays on-screen whenever we move the non-selected containers.
                    section.Icon.Parent = ContentContainer;

                    // Align the items in the container.
                    if (i > 0)
                        section.Items[i].Y += i * (section.Items[i].Height + 5);
                }

                // If the section is't the selected one, then move it off-screen so it isn't visible.
                if (section != SelectedSection)
                    section.Container.X = WindowManager.Width;
            });
        }

        /// <summary>
        ///     Creates the options section buttons
        /// </summary>
        private void CreateSectionButtons()
        {
            for (var i = 0; i < Sections.Count; i++)
            {
                var section = Sections[i];

                // Add the section icon to the container.
                section.Icon.Parent = Sections[i].Container;
                section.Icon.X = 30;
                section.Icon.Y = HeaderContainer.Height + 40 + i * ( section.Icon.Height + 10 );
            }
        }

        /// <summary>
        ///     Creates a vertical divider line.
        /// </summary>
        private void CreateVerticalDividerLine() => DividerLineVertical = new Sprite()
        {
            Parent = ContentContainer,
            Size = new ScalableVector2(1, ContentContainer.Height - HeaderContainer.Height - FooterContainer.Height - 20),
            X = Sections[0].Icon.X + Sections[0].Icon.Width + 25,
            Alignment = Alignment.MidLeft
        };

        /// <summary>
        ///     Confirms all the changes that the user wants. For bigger operations so the user doesn't get annoyed.
        /// </summary>
        private void ConfirmChanges()
        {
            if (ChangedResolution.X != ConfigManager.WindowWidth.Value || ChangedResolution.Y != ConfigManager.WindowHeight.Value)
            {
                ConfigManager.WindowWidth.Value = ChangedResolution.X;
                ConfigManager.WindowHeight.Value = ChangedResolution.Y;

                WindowManager.ChangeScreenResolution(ChangedResolution);
            }

            // Dictates if the skin needs to reload.
            var skinNeedsReloading = false;

            // Custom SKin
            if (NewSkinToLoad != ConfigManager.Skin.Value)
            {
                ConfigManager.Skin.Value = NewSkinToLoad;
                skinNeedsReloading = true;
            }

            // Default skin selection
            if (NewDefaultSkinToLoad != ConfigManager.DefaultSkin.Value)
            {
                ConfigManager.DefaultSkin.Value = NewDefaultSkinToLoad;
                skinNeedsReloading = true;
            }

            // Reload skin if we need to reload.
            if (skinNeedsReloading)
                SkinManager.Load();
        }

        /// <summary>
        ///     Creates the video section of the options menu
        /// </summary>
        /// <returns></returns>
        private OptionsSection CreateVideoSection()
        {
            var currentResolution = $"{ConfigManager.WindowWidth.Value}x{ConfigManager.WindowHeight.Value}";

            // 16:9 resolutions. Quaver looks shit on 4:3 with the reference resolution.
            var commonResolutions = new List<string>()
            {
                "1024x576",
                "1152x648",
                "1280x720",
                "1366x768",
                "1600x900",
                "1920x1080",
                "2560x1440"
            };

            var resolutionIndex = commonResolutions.FindIndex(x => x == currentResolution);

            // If the resolution isn't there, then we need to add the res to the list
            // and set the resolution index manually.
            if (resolutionIndex == -1)
            {
                commonResolutions.Add(currentResolution);
                resolutionIndex = commonResolutions.Count - 1;
            }

            // Create the option's section
            return new OptionsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_desktop_monitor), new List<OptionsItem>
            {
                // Window Resolution
                new OptionsItem(this, "Resolution", new HorizontalSelector(commonResolutions, new ScalableVector2(50, 10),
                    BitmapFonts.Exo2Regular, 16, UserInterface.LeftButtonSquare, UserInterface.RightButtonSquare, new ScalableVector2(10, 10),
                    10, (val, index) =>
                    {
                        SkinManager.Skin.SoundClick.CreateChannel().Play();

                        var resolutionSplit = val.Split('x');
                        ChangedResolution = new Point(int.Parse(resolutionSplit[0]), int.Parse(resolutionSplit[1]));
                    }, resolutionIndex)
                ),

                // Full-screen
                new OptionsItem(this, "Fullscreen", new Checkbox(ConfigManager.WindowFullScreen, new Vector2(20, 20),
                        FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false),
                    () =>
                    {
                        GameBase.Game.Graphics.IsFullScreen = ConfigManager.WindowFullScreen.Value;
                    }),

                // Background Brightness
                new OptionsItem(this, "Background Brightness", new Slider(ConfigManager.BackgroundBrightness, Vector2.One, FontAwesome.Get(FontAwesomeIcon.fa_circle))),

                // Background Blur
                new OptionsItem(this, "Background Blur", new Slider(ConfigManager.BackgroundBlur, Vector2.One, FontAwesome.Get(FontAwesomeIcon.fa_circle))),

                // Display FPS counter.
                new OptionsItem(this, "Display FPS Counter", new Checkbox(ConfigManager.FpsCounter, new Vector2(20, 20),
                        FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false), () => {}),
            }, true);
        }

        /// <summary>
        ///     Creates the audio section of the options menu.
        /// </summary>
        /// <returns></returns>
        private OptionsSection CreateAudioSection() => new OptionsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_volume_up_interface_symbol),
            new List<OptionsItem>()
        {
            new OptionsItem(this, "Master Volume", new Slider(ConfigManager.VolumeGlobal, Vector2.One, FontAwesome.Get(FontAwesomeIcon.fa_circle))),
            new OptionsItem(this, "Music Volume", new Slider(ConfigManager.VolumeMusic, Vector2.One, FontAwesome.Get(FontAwesomeIcon.fa_circle))),
            new OptionsItem(this, "Effect Volume", new Slider(ConfigManager.VolumeEffect, Vector2.One, FontAwesome.Get(FontAwesomeIcon.fa_circle))),

            // Pitch Audio w/ Rate.
            new OptionsItem(this, "Pitch Audio With Rate", new Checkbox(ConfigManager.Pitched, new Vector2(20, 20),
                FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false), () => {}),
        });

        /// <summary>
        ///     Creates the misc section of the options menu
        /// </summary>
        /// <returns></returns>
        private OptionsSection CreateMiscSection() => new OptionsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_question_sign), new List<OptionsItem>()
        {
            // Select osu!.db file
            /*new OptionsItem(this, "Select peppy!.db file", new TextButton(UserInterface.BlankBox, BitmapFonts.Exo2Regular, "Select", 0.50f,
            (sender, e) =>
            {
                // Create the openFileDialog object.
                var openFileDialog = new OpenFileDialog()
                {
                    InitialDirectory = "c:\\",
                    Filter = "Osu Database File (*.db)| *.db",
                    FilterIndex = 0,
                    RestoreDirectory = true,
                    Multiselect = false
                };

                // If the dialog couldn't be shown, that's an issue, so we'll return for now.
                if (openFileDialog.ShowDialog() != DialogResult.OK)
                    return;

                ConfigManager.OsuDbPath.Value = openFileDialog.FileName;

                NotificationManager.Show(NotificationLevel.Success, $".db file has been set. You can now \"Load peppy! beatmaps\"");
            })),*/

            // Load osu! beatmaps, although we can't use osu! as actual text because... trademark :thumbsup:
            new OptionsItem(this, "Load peppy! beatmaps", new Checkbox(ConfigManager.AutoLoadOsuBeatmaps, new Vector2(20, 20),
                FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false), MapCache.LoadAndSetMapsets),

            // Select Etterna Cache Folder
            /*new OptionsItem(this, "Select Etterna Cache Folder", new TextButton(UserInterface.BlankBox, BitmapFonts.Exo2Regular, "Select", 0.50f,
            (sender, e) =>
            {
                using(var fbd = new FolderBrowserDialog())
                {
                    var result = fbd.ShowDialog();

                    ConfigManager.EtternaCacheFolderPath.Value = fbd.SelectedPath;
                }

                NotificationManager.Show(NotificationLevel.Success, $"Etterna Cache Folder has been set. You can now \"Load Etterna Charts\"");
            })),*/

            // Load charts from etterna.
            new OptionsItem(this, "Load Etterna Charts", new Checkbox(ConfigManager.AutoLoadEtternaCharts, new Vector2(20, 20),
                FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false), MapCache.LoadAndSetMapsets),
        });

        /// <summary>
        ///     Creates the gameplay section of the options menu
        /// </summary>
        /// <returns></returns>
        private OptionsSection CreateGameplaySection() =>
            // Create section
            new OptionsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), new List<OptionsItem>()
            {
                // Custom skin selection
                CreateCustomSkinSelectionItem(),

                // Default skin selection
                CreateDefaultSkinSelection(),

                // Use Default Skin
                new OptionsItem(this, "Use Default Skin", new Checkbox(new Bindable<bool>(string.IsNullOrEmpty(ConfigManager.Skin.Value),
                (sender, e) =>
                {
                    // If set to true, we want to set the skin the user is using to true,
                    if (!e.Value)
                        return;

                    ConfigManager.Skin.Value = "";
                    SkinManager.Load();
                }), new Vector2(20, 20), FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), true)),

                // DownScroll 4K
                new OptionsItem(this, "Notes Scroll Down - 4K", new Checkbox(ConfigManager.DownScroll4K, new Vector2(20, 20),
                    FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false)),

                // DownScroll 7K
                new OptionsItem(this, "Notes Scroll Down - 7K", new Checkbox(ConfigManager.DownScroll7K, new Vector2(20, 20),
                    FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false)),

                // Song Time Progress
                new OptionsItem(this, "Display Song Time Progress", new Checkbox(ConfigManager.DisplaySongTimeProgress, new Vector2(20, 20),
                    FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false)),

                // Anim Judge Counter
                new OptionsItem(this, "Animate Judgement Counter", new Checkbox(ConfigManager.AnimateJudgementCounter, new Vector2(20, 20),
                    FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false)),

                // Bots Enabled
                new OptionsItem(this, "Bots Enabled", new Checkbox(ConfigManager.BotsEnabled, new Vector2(20, 20),
                    FontAwesome.Get(FontAwesomeIcon.fa_circle), FontAwesome.Get(FontAwesomeIcon.fa_circle_shape_outline), false)),
            });

        /// <summary>
        ///     Create custom skin selection.
        /// </summary>
        /// <returns></returns>
        private OptionsItem CreateCustomSkinSelectionItem()
        {
            var availableSkins = new DirectoryInfo(ConfigManager.SkinDirectory.Value).GetDirectories().ToList();

            // make a list with all of the available skins.
            var skinsList = new List<string>();
            availableSkins.ForEach(x => skinsList.Add(x.Name));

            int selectedSkinIndex;

            if (skinsList.Count == 0 || ConfigManager.Skin.Value == "")
            {
                skinsList.Add("Default");
                selectedSkinIndex = skinsList.Count - 1;
            }
            else
            {
                selectedSkinIndex = skinsList.FindIndex(x => x == ConfigManager.Skin.Value);

                if (selectedSkinIndex == -1)
                    selectedSkinIndex = 0;
            }

            return new OptionsItem(this, "Custom Skin", new HorizontalSelector(skinsList, new ScalableVector2(50, 10),
                BitmapFonts.Exo2Regular, 16, UserInterface.LeftButtonSquare, UserInterface.RightButtonSquare, new ScalableVector2(10, 10),
                10, (val, index) =>
                {
                    SkinManager.Skin.SoundClick.CreateChannel().Play();

                    if (val == ConfigManager.Skin.Value)
                        return;

                    NewSkinToLoad = val == "Default" ? "" : val;
                }, selectedSkinIndex)
            );
        }

        /// <summary>
        ///     Creates the default skin selection item.
        /// </summary>
        private OptionsItem CreateDefaultSkinSelection()
        {
            var defaultSkins = Enum.GetNames(typeof(DefaultSkins)).ToList();
            var selectedIndex = defaultSkins.FindIndex(x => x == ConfigManager.DefaultSkin.ToString());

            // Create Item
            return new OptionsItem(this, "Default Skin", new HorizontalSelector(defaultSkins, new ScalableVector2(50, 10),
                                    BitmapFonts.Exo2Regular, 16, UserInterface.LeftButtonSquare, UserInterface.RightButtonSquare,
                                    new ScalableVector2(10, 10), 10,
            (val, index) =>
            {
                SkinManager.Skin.SoundClick.CreateChannel().Play();

                if (val == ConfigManager.DefaultSkin.Value.ToString())
                    return;

                NewDefaultSkinToLoad = ConfigHelper.ReadDefaultSkin(DefaultSkins.Bar, val);
            }, selectedIndex));
        }

        /// <summary>
        ///    Creates the keybinds section of the option's menu.
        /// </summary>
        /// <returns></returns>
        private OptionsSection CreateKeybindSection() => new OptionsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_keyboard), new List<OptionsItem>
        {
            // 4K
            new OptionsItem(this, "4K Layout", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("4K Lane 1", ConfigManager.KeyMania4K1),
                        new KeybindingOptionStore("4K Lane 2", ConfigManager.KeyMania4K2),
                        new KeybindingOptionStore("4K Lane 3", ConfigManager.KeyMania4K3),
                        new KeybindingOptionStore("4K Lane 4", ConfigManager.KeyMania4K4),
                    }, 0.85f));
                })),

            // 7K
            new OptionsItem(this, "7K Layout", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("7K Lane 1", ConfigManager.KeyMania7K1),
                        new KeybindingOptionStore("7K Lane 2", ConfigManager.KeyMania7K2),
                        new KeybindingOptionStore("7K Lane 3", ConfigManager.KeyMania7K3),
                        new KeybindingOptionStore("7K Lane 4", ConfigManager.KeyMania7K4),
                        new KeybindingOptionStore("7K Lane 5", ConfigManager.KeyMania7K5),
                        new KeybindingOptionStore("7K Lane 6", ConfigManager.KeyMania7K6),
                        new KeybindingOptionStore("7K Lane 7", ConfigManager.KeyMania7K7),
                    }, 0.85f));
                })),

            // Change Scroll Speed
            new OptionsItem(this, "Decrease/Increase Scroll Speed", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("Decrease Scroll Speed", ConfigManager.KeyDecreaseScrollSpeed),
                        new KeybindingOptionStore("Increase Scroll Speed", ConfigManager.KeyIncreaseScrollSpeed)
                    }, 0.85f));
                })),

            // Pause
            new OptionsItem(this, "Pause", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("Pause", ConfigManager.KeyPause),
                    }, 0.85f));
                })),

            // Skip Song Intro
            new OptionsItem(this, "Skip Song Intro", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("Skip Song Intro", ConfigManager.KeySkipIntro),
                    }, 0.85f));
                })),

            // Restart Map
            new OptionsItem(this, "Quick Restart", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("Quick Restart", ConfigManager.KeyRestartMap),
                    }, 0.85f));
                })),

            // Quick Exit
            new OptionsItem(this, "Quick Exit", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("Quick Exit", ConfigManager.KeyQuickExit),
                    }, 0.85f));
                })),

            // Restart Map
            new OptionsItem(this, "Toggle Scoreboard Display", new TextButton(UserInterface.BlankBox,
                BitmapFonts.Exo2Regular, "Change", 14,
                (sender, e) =>
                {
                    DialogManager.Show(new KeybindDialog(new List<KeybindingOptionStore>
                    {
                        new KeybindingOptionStore("Toggle Scoreboard Display", ConfigManager.KeyScoreboardVisible),
                    }, 0.85f));
                })),
        });
    }
}
