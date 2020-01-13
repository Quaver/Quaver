using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Options.Content;
using Quaver.Shared.Screens.Options.Items;
using Quaver.Shared.Screens.Options.Items.Custom;
using Quaver.Shared.Screens.Options.Sections;
using Quaver.Shared.Screens.Options.Sidebar;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Options
{
    public class OptionsMenu : Sprite
    {
        /// <summary>
        /// </summary>
        private OptionsHeader Header { get; set; }

        /// <summary>
        /// </summary>
        private OptionsSidebar Sidebar { get; set; }

        /// <summary>
        /// </summary>
        private OptionsContent Content { get; set; }

        /// <summary>
        /// </summary>
        private List<OptionsSection> Sections { get; set; }

        /// <summary>
        /// </summary>
        public Bindable<OptionsSection> SelectedSection { get; private set; }

        /// <summary>
        /// </summary>
        private Dictionary<OptionsSection, OptionsContentContainer> ContentContainers { get; set; }

        /// <summary>
        /// </summary>
        public OptionsMenu()
        {
            Size = new ScalableVector2(1366, 768);
            Alpha = 0;

            CreateContainer();
            CreateSections();
            CreateSidebar();
            CreateContentContainers();
            CreateHeader();

            SetActiveContentContainer();
            SelectedSection.ValueChanged += OnSectionChanged;
        }

        private void OnSectionChanged(object sender, BindableValueChangedEventArgs<OptionsSection> e)
            => ScheduleUpdate(SetActiveContentContainer);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedSection.ValueChanged -= OnSectionChanged;
            SelectedSection?.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateSections()
        {
            var containerRect = Content.ScreenRectangle;

            Sections = new List<OptionsSection>
            {
                new OptionsSection("Video", FontAwesome.Get(FontAwesomeIcon.fa_desktop_monitor), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Window", new List<OptionsItem>()
                    {
                        new OptionsItemScreenResolution(containerRect, "Screen Resolution"),
                        new OptionsItemCheckbox(containerRect, "Enable Fullscreen", ConfigManager.WindowFullScreen),
                        new OptionsItemCheckbox(containerRect, "Enable Borderless Window", ConfigManager.WindowBorderless)
                    }),
                    new OptionsSubcategory("Frame Time", new List<OptionsItem>()
                    {
                        new OptionsItemFrameLimiter(containerRect, "Frame Limiter"),
                        new OptionsItemCheckbox(containerRect, "Display FPS Counter", ConfigManager.FpsCounter)
                    })
                }),
                new OptionsSection("Audio", FontAwesome.Get(FontAwesomeIcon.fa_volume_up_interface_symbol), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Volume", new List<OptionsItem>()
                    {
                        new OptionsSlider(containerRect, "Master Volume", ConfigManager.VolumeGlobal),
                        new OptionsSlider(containerRect, "Music Volume", ConfigManager.VolumeMusic),
                        new OptionsSlider(containerRect, "Effect Volume", ConfigManager.VolumeEffect),
                    }),
                    new OptionsSubcategory("Offset", new List<OptionsItem>()
                    {
                        new OptionsItemSliderGlobalOffset(containerRect, "Global Offset", ConfigManager.GlobalAudioOffset),
                        new OptionsItemCalibrateOffset(containerRect, "Calibrate Offset")
                    }),
                    new OptionsSubcategory("Effects", new List<OptionsItem>()
                    {
                       new OptionsItemCheckbox(containerRect, "Pitch Audio With Playback Rate", ConfigManager.Pitched)
                    }),
                    new OptionsSubcategory("Linux", new List<OptionsItem>()
                    {
                        new OptionsSlider(containerRect, "Audio Device Period", ConfigManager.DevicePeriod, i => $"{i} ms"),
                        new OptionsItemAudioBufferLength(containerRect, "Audio Device Buffer Length", ConfigManager.DeviceBufferLengthMultiplier,
                            ConfigManager.DevicePeriod, (multiplier, period) => $"{multiplier * period} ms"),
                    })
                }),
                new OptionsSection("Gameplay", FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Scrolling", new List<OptionsItem>()
                    {
                        new OptionsItemScrollDirection(containerRect, "4K Scroll Direction", ConfigManager.ScrollDirection4K),
                        new OptionsItemScrollDirection(containerRect, "7K Scroll Direction", ConfigManager.ScrollDirection7K),
                        new OptionsSlider(containerRect, "4K Scroll Speed", ConfigManager.ScrollSpeed4K, i => $"{i}"),
                        new OptionsSlider(containerRect, "7K Scroll Speed", ConfigManager.ScrollSpeed7K, i => $"{i}"),
                    }),
                    new OptionsSubcategory("Background", new List<OptionsItem>()
                    {
                       new OptionsSlider(containerRect, "Background Brightness", ConfigManager.BackgroundBrightness),
                       new OptionsItemCheckbox(containerRect, "Enable Background Blur", ConfigManager.BlurBackgroundInGameplay)
                    }),
                    new OptionsSubcategory("Sound", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Enable Hitsounds", ConfigManager.EnableHitsounds),
                        new OptionsItemCheckbox(containerRect, "Enable Keysounds", ConfigManager.EnableKeysounds)
                    }),
                    new OptionsSubcategory("Input", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Enable Tap To Pause", ConfigManager.TapToPause),
                        new OptionsItemCheckbox(containerRect, "Skip Results Screen After Quitting", ConfigManager.SkipResultsScreenAfterQuit)
                    }),
                    new OptionsSubcategory("User Interface", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Show Spectators", ConfigManager.ShowSpectators),
                        new OptionsItemCheckbox(containerRect, "Display Timing Lines", ConfigManager.DisplayTimingLines),
                        new OptionsItemCheckbox(containerRect, "Display Judgement Counter", ConfigManager.DisplayJudgementCounter),
                        new OptionsItemCheckbox(containerRect, "Enable Combo Alerts", ConfigManager.DisplayComboAlerts),
                        new OptionsItemCheckbox(containerRect, "Enable Accuracy Display Animations", ConfigManager.SmoothAccuracyChanges),
                    }),
                    new OptionsSubcategory("Scoreboard", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Scoreboard", ConfigManager.ScoreboardVisible),
                        new OptionsItemCheckbox(containerRect, "Display Unbeatable Scores", ConfigManager.DisplayUnbeatableScoresDuringGameplay)
                    }),
                    new OptionsSubcategory("Progress Bar", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Song Time Progress Bar", ConfigManager.DisplaySongTimeProgress),
                        new OptionsItemCheckbox(containerRect, "Display Song Time Progress Bar Time Numbers", ConfigManager.DisplaySongTimeProgressNumbers)
                    }),
                    new OptionsSubcategory("Lane Cover", new List<OptionsItem>()
                    {
                       new OptionsItemCheckbox(containerRect, "Enable Top Lane Cover", ConfigManager.LaneCoverTop),
                       new OptionsSlider(containerRect, "Top Lane Cover Height", ConfigManager.LaneCoverTopHeight),
                       new OptionsItemCheckbox(containerRect, "Enable Bottom Lane Cover", ConfigManager.LaneCoverBottom),
                       new OptionsSlider(containerRect, "Bottom Lane Cover Height", ConfigManager.LaneCoverBottomHeight),
                       new OptionsItemCheckbox(containerRect, "Display UI Elements Over Lane Covers", ConfigManager.UIElementsOverLaneCover)
                    }),
                    new OptionsSubcategory("Multiplayer", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Enable Battle Royale Alerts", ConfigManager.EnableBattleRoyaleAlerts),
                        new OptionsItemCheckbox(containerRect, "Enable Battle Royale Background Flashing", ConfigManager.EnableBattleRoyaleBackgroundFlashing)
                    })
                }),
                new OptionsSection("Skin", FontAwesome.Get(FontAwesomeIcon.fa_check), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Selection", new List<OptionsItem>()
                    {
                        new OptionsItemCustomSkin(containerRect, "Custom Skin", ConfigManager.Skin),
                        new OptionsItemDefaultSkin(containerRect, "Default Skin", ConfigManager.DefaultSkin)
                    }),
                    new OptionsSubcategory("Navigation", new List<OptionsItem>()
                    {
                        new OptionsItemOpenSkinFolder(containerRect, "Open Skin Folder")
                    })
                }),
                new OptionsSection("Input", FontAwesome.Get(FontAwesomeIcon.fa_keyboard), new List<OptionsSubcategory>
                {
                }),
                new OptionsSection("Network", FontAwesome.Get(FontAwesomeIcon.fa_earth_globe), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Login", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Automatically Log Into The Server", ConfigManager.AutoLoginToServer),
                    }),
                    new OptionsSubcategory("Notifications", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Online Friend Notifications", ConfigManager.DisplayFriendOnlineNotifications),
                        new OptionsItemCheckbox(containerRect, "Display Song Request Notifications", ConfigManager.DisplaySongRequestNotifications)
                    })
                }),
                new OptionsSection("Integration", FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Games", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Load Songs From Other Installed Games", ConfigManager.AutoLoadOsuBeatmaps)
                    }),
                }),
                new OptionsSection("Miscellaneous", FontAwesome.Get(FontAwesomeIcon.fa_question_sign), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Effects", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Menu Audio Visualizer", ConfigManager.DisplayMenuAudioVisualizer),
                    }),
                    new OptionsSubcategory("Song Select", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Failed Local Scores", ConfigManager.DisplayFailedLocalScores)
                    })
                }),
            };

            SelectedSection = new Bindable<OptionsSection>(Sections.First()) { Value = Sections.First() };
        }

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new OptionsHeader(SelectedSection, Width, Sidebar.Width)
        {
            Parent = this,
            Alignment = Alignment.TopLeft
        };

        /// <summary>
        /// </summary>
        private void CreateSidebar()
        {
            Sidebar = new OptionsSidebar(SelectedSection, Sections, new ScalableVector2(OptionsSidebar.WIDTH,
                Height - OptionsHeader.HEIGHT))
            {
                Parent = this,
                Y = OptionsHeader.HEIGHT
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            Content = new OptionsContent(new ScalableVector2(Width - OptionsSidebar.WIDTH + 2,
                Height - OptionsHeader.HEIGHT))
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                X = OptionsSidebar.WIDTH - 2,
                Y = OptionsHeader.HEIGHT
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainers()
        {
            ContentContainers = new Dictionary<OptionsSection, OptionsContentContainer>();

            foreach (var section in Sections)
                ContentContainers.Add(section, new OptionsContentContainer(section, Content.Size));
        }

        /// <summary>
        /// </summary>
        private void SetActiveContentContainer()
        {
            foreach (var container in ContentContainers)
                container.Value.Parent = SelectedSection.Value == container.Key ? Content : null;
        }
    }
}