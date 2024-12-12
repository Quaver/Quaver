using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Options.Content;
using Quaver.Shared.Screens.Options.Items;
using Quaver.Shared.Screens.Options.Items.Custom;
using Quaver.Shared.Screens.Options.Sections;
using Quaver.Shared.Screens.Options.Sidebar;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Input;

namespace Quaver.Shared.Screens.Options
{
    public class OptionsMenu : Sprite
    {
        /// <summary>
        /// </summary>
        public static string LastOpenedSection { get; set; } = "Video";

        /// <summary>
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; } = new Bindable<string>("") { Value = "" };

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
        ///     Whether or not an option is currently focused
        /// </summary>
        public Bindable<bool> IsOptionFocused { get; } = new Bindable<bool>(false) { Value = false };

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

            SelectedSection.Value = Sections.Find(x => x.Name == LastOpenedSection) ?? Sections.First();
            SetActiveContentContainer();
            SelectedSection.ValueChanged += OnSectionChanged;
            CurrentSearchQuery.ValueChanged += OnSearchChanged;

            DestroyIfParentIsNull = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SetOptionFocusedState();
            SkinManager.HandleSkinReloading();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            SelectedSection.ValueChanged -= OnSectionChanged;
            SelectedSection?.Dispose();
            CurrentSearchQuery?.Dispose();
            IsOptionFocused?.Dispose();

            // Make sure to destroy everything that's not visible
            foreach (var section in Sections)
            {
                foreach (var subcategory in section.Subcategories)
                {
                    foreach (var item in subcategory.Items)
                    {
                        item?.Destroy();
                    }
                }
            }

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateSections()
        {
            var containerRect = Content.ScreenRectangle;

            Sections = new List<OptionsSection>
            {
                new OptionsSection("Video", UserInterface.OptionsVideo, new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Window", new List<OptionsItem>()
                    {
                        new OptionsItemScreenResolution(containerRect, "Screen Resolution"),
                        new OptionsItemCheckbox(containerRect, "Enable Fullscreen", ConfigManager.WindowFullScreen),
                        new OptionsItemCheckbox(containerRect, "Enable Borderless Window", ConfigManager.WindowBorderless)
                    }),
                    new OptionsSubcategory("Frame Time", new List<OptionsItem>()
                    {
                        new OptionsItemFrameLimiter(containerRect, "Frame Limiter")
                        {
                            Tags = new List<string> {"fps", "limited", "unlimited", "vsync", "wayland"}
                        },
                        new OptionsItemCustomFps(containerRect, "Set Custom FPS"),
                        new OptionsItemCheckbox(containerRect, "Display FPS Counter", ConfigManager.FpsCounter),
                    })
                }),
                new OptionsSection("Audio", UserInterface.OptionsAudio, new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Output", new List<OptionsItem>()
                    {
                        new OptionsItemAudioOutputDevice(containerRect, "Audio Output Device")
                    }),
                    new OptionsSubcategory("Volume", new List<OptionsItem>()
                    {
                        new OptionsSlider(containerRect, "Master Volume", ConfigManager.VolumeGlobal),
                        new OptionsSlider(containerRect, "Music Volume", ConfigManager.VolumeMusic),
                        new OptionsSlider(containerRect, "Effect Volume", ConfigManager.VolumeEffect)
                        {
                            Tags = new List<string> {"fx", "sfx"}
                        },
                    }),
                    new OptionsSubcategory("Offset", new List<OptionsItem>()
                    {
                        new OptionsItemSliderGlobalOffset(containerRect, "Global Audio Offset", ConfigManager.GlobalAudioOffset),
                        new OptionsSlider(containerRect, "Visual Offset", ConfigManager.VisualOffset, i => $"{i} ms"),
                        new OptionsItemCalibrateOffset(containerRect, "Calibrate Offset")
                    }),
                    new OptionsSubcategory("Effects", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Pitch Audio With Playback Rate", ConfigManager.Pitched)
                        {
                            Tags = new List<string> {"speed"}
                        }
                    }),
                }),
                new OptionsSection("Gameplay", UserInterface.OptionsGameplay, new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Scrolling", new List<OptionsItem>()
                    {
                        new OptionsItemScrollDirection(containerRect, "4K Scroll Direction", ConfigManager.ScrollDirection4K),
                        new OptionsItemScrollDirection(containerRect, "7K Scroll Direction", ConfigManager.ScrollDirection7K),
                        new OptionsSlider(containerRect, "4K Scroll Speed", ConfigManager.ScrollSpeed4K, i => $"{i / 10f:0.0}"),
                        new OptionsSlider(containerRect, "7K Scroll Speed", ConfigManager.ScrollSpeed7K, i => $"{i / 10f:0.0}"),
                        new OptionsSlider(containerRect, "Scroll Speed Rate Scaling Percentage", ConfigManager.ScaleScrollSpeedStrengthPercentage, i => $"{i}%"),
                    }),
                    new OptionsSubcategory("Scratch Lane", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Place 7K Scratch Lane On Left", ConfigManager.ScratchLaneLeft7K)
                    }),
                    new OptionsSubcategory("Background", new List<OptionsItem>()
                    {
                        new OptionsSlider(containerRect, "Background Brightness", ConfigManager.BackgroundBrightness),
                    }),
                    new OptionsSubcategory("Sound", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Enable Hitsounds", ConfigManager.EnableHitsounds),
                        new OptionsItemCheckbox(containerRect, "Enable Long Note Release Hitsounds", ConfigManager.EnableLongNoteReleaseHitsounds),
                        new OptionsItemCheckbox(containerRect, "Enable Keysounds", ConfigManager.EnableKeysounds)
                    }),
                    new OptionsSubcategory("Input", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Enable Tap To Pause", ConfigManager.TapToPause),
                        new OptionsItemCheckbox(containerRect, "Enable Tap To Restart", ConfigManager.TapToRestart),
                        new OptionsItemCheckbox(containerRect, "Skip Results Screen After Quitting", ConfigManager.SkipResultsScreenAfterQuit),
                        new OptionsItemCheckbox(containerRect, "Lock Windows Key during gameplay", ConfigManager.LockWinkeyDuringGameplay)
                    }),
                    new OptionsSubcategory("User Interface", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Timing Lines", ConfigManager.DisplayTimingLines),
                        new OptionsItemCheckbox(containerRect, "Display Hit Bubbles", ConfigManager.DisplayHitBubbles),
                        new OptionsItemCheckbox(containerRect, "Display Judgement Counter", ConfigManager.DisplayJudgementCounter),
                    }),
                    new OptionsSubcategory("Scoreboard", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Scoreboard", ConfigManager.ScoreboardVisible),
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
                        new OptionsItemCheckbox(containerRect, "Display UI Elements Over Lane Covers", ConfigManager.UIElementsOverLaneCover),
                        new OptionsItemCheckbox(containerRect, "Display Receptors Over Lane Covers", ConfigManager.ReceptorsOverLaneCover)
                    })
                }),
                new OptionsSection("Skin", UserInterface.OptionsSkin, new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Selection", new List<OptionsItem>()
                    {
                        new OptionsItemCustomSkin(containerRect, "Custom Skin", ConfigManager.Skin),
                        new OptionsItemCustomSkin(containerRect, "Co-op Player 2 Skin", ConfigManager.TournamentPlayer2Skin),
                        new OptionsItemDefaultSkin(containerRect, "Default Skin", ConfigManager.DefaultSkin)
                    }),
                    new OptionsSubcategory("Navigation", new List<OptionsItem>()
                    {
                        new OptionsItemOpenSkinFolder(containerRect, "Open Skin Folder")
                    }),
                    new OptionsSubcategory("Sharing", new List<OptionsItem>()
                    {
                        new OptionsItemExportSkin(containerRect, "Export Skin"),
                        new OptionsItemUploadSkinToWorkshop(containerRect, "Upload Skin To Steam Workshop")
                    }),
                    new OptionsSubcategory("Configuration", new List<OptionsItem>()
                    {
                        new OptionsSlider(containerRect, "Note & Receptor Size Scale", ConfigManager.GameplayNoteScale, i => $"{i / 100f:0.00}x")
                            {Tags = new List<string>() {"mini"}},
                        new OptionsItemCheckbox(containerRect, "Tint Hitlighting Based On Judgement Color", ConfigManager.TintHitLightingBasedOnJudgementColor)
                    })
                }),
                new OptionsSection("Input", UserInterface.OptionsInput, new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Gameplay", new List<OptionsItem>()
                    {
                        new OptionsItemKeybindMultiple(containerRect, "4K Gameplay Layout", new List<Bindable<GenericKey>>()
                        {
                            ConfigManager.KeyMania4K1,
                            ConfigManager.KeyMania4K2,
                            ConfigManager.KeyMania4K3,
                            ConfigManager.KeyMania4K4
                        })
                        {
                            Tags = new List<string> {"keybind", "keyboard", "keys"}
                        },
                        new OptionsItemKeybindMultiple(containerRect, "7K Gameplay Layout", new List<Bindable<GenericKey>>()
                        {
                            ConfigManager.KeyMania7K1,
                            ConfigManager.KeyMania7K2,
                            ConfigManager.KeyMania7K3,
                            ConfigManager.KeyMania7K4,
                            ConfigManager.KeyMania7K5,
                            ConfigManager.KeyMania7K6,
                            ConfigManager.KeyMania7K7,
                        })
                        {
                            Tags = new List<string> {"keybind", "keyboard", "keys"}
                        },
                        new OptionsItemKeybindMultiple(containerRect, "7K + 1 Gameplay Layout", new List<Bindable<GenericKey>>()
                        {
                            ConfigManager.KeyLayout7KScratch1,
                            ConfigManager.KeyLayout7KScratch2,
                            ConfigManager.KeyLayout7KScratch3,
                            ConfigManager.KeyLayout7KScratch4,
                            ConfigManager.KeyLayout7KScratch5,
                            ConfigManager.KeyLayout7KScratch6,
                            ConfigManager.KeyLayout7KScratch7,
                        })
                        {
                            Tags = new List<string> {"keybind", "keyboard", "keys"}
                        },
                        new OptionsItemKeybindMultiple(containerRect, "7K + 1 Scratch Lane Keys", new List<Bindable<GenericKey>>()
                        {
                            ConfigManager.KeyLayout7KScratch8,
                            ConfigManager.KeyLayout7KScratch9,
                        })
                        {
                            Tags = new List<string> {"keybind", "keyboard", "keys"}
                        },
                    }) ,
                    new OptionsSubcategory("Co-op Gameplay", new List<OptionsItem>()
                    {
                        new OptionsItemKeybindMultiple(containerRect, "4K Co-op Player 2 Layout", new List<Bindable<GenericKey>>()
                        {
                            ConfigManager.KeyCoop2P4K1,
                            ConfigManager.KeyCoop2P4K2,
                            ConfigManager.KeyCoop2P4K3,
                            ConfigManager.KeyCoop2P4K4
                        })
                        {
                            Tags = new List<string> {"keybind", "keyboard", "keys"}
                        },
                        new OptionsItemKeybindMultiple(containerRect, "7K Co-op Player 2 Layout", new List<Bindable<GenericKey>>()
                        {
                            ConfigManager.KeyCoop2P7K1,
                            ConfigManager.KeyCoop2P7K2,
                            ConfigManager.KeyCoop2P7K3,
                            ConfigManager.KeyCoop2P7K4,
                            ConfigManager.KeyCoop2P7K5,
                            ConfigManager.KeyCoop2P7K6,
                            ConfigManager.KeyCoop2P7K7,
                        })
                        {
                            Tags = new List<string> {"keybind", "keyboard", "keys"}
                        }
                    }),
                    new OptionsSubcategory("Gameplay Controls", new List<OptionsItem>()
                    {
                        new OptionsItemKeybindGeneric(containerRect, "Pause", ConfigManager.KeyPause),
                        new OptionsItemKeybind(containerRect, "Quick Restart", ConfigManager.KeyRestartMap),
                        new OptionsItemKeybind(containerRect, "Quick Exit", ConfigManager.KeyQuickExit),
                        new OptionsItemKeybindGeneric(containerRect, "Skip Song Intro", ConfigManager.KeySkipIntro),
                        new OptionsItemKeybind(containerRect, "Decrease Scroll Speed", ConfigManager.KeyDecreaseScrollSpeed),
                        new OptionsItemKeybind(containerRect, "Increase Scroll Speed", ConfigManager.KeyIncreaseScrollSpeed),
                        new OptionsItemKeybind(containerRect, "Decrease Map Offset", ConfigManager.KeyDecreaseMapOffset),
                        new OptionsItemKeybind(containerRect, "Increase Map Offset", ConfigManager.KeyIncreaseMapOffset),
                    }),
                    new OptionsSubcategory("Gameplay User Interface", new List<OptionsItem>()
                    {
                        new OptionsItemKeybind(containerRect, "Toggle Scoreboard Visibility", ConfigManager.KeyScoreboardVisible),
                    }),
                    new OptionsSubcategory("User Interface", new List<OptionsItem>()
                    {
                        new OptionsItemKeybind(containerRect, "Toggle Chat Overlay", ConfigManager.KeyToggleOverlay)
                    }),
                    new OptionsSubcategory("Song Selection", new List<OptionsItem>()
                    {
                        new OptionsItemKeybind(containerRect, "Decrease Gameplay Rate", ConfigManager.KeyDecreaseGameplayAudioRate),
                        new OptionsItemKeybind(containerRect, "Increase Gameplay Rate", ConfigManager.KeyIncreaseGameplayAudioRate),
                        new OptionsItemKeybind(containerRect, "Toggle Mirror Mod", ConfigManager.KeyToggleMirror),
                    }),
                    new OptionsSubcategory("Editor", new List<OptionsItem>()
                    {
                        new OptionsItemKeybind(containerRect, "Pause/Play Track", ConfigManager.KeyEditorPausePlay),
                        new OptionsItemKeybind(containerRect, "Decrease Playback Rate", ConfigManager.KeyEditorDecreaseAudioRate),
                        new OptionsItemKeybind(containerRect, "Increase Playback Rate", ConfigManager.KeyEditorIncreaseAudioRate),
                    }),
                    new OptionsSubcategory("Misc", new List<OptionsItem>()
                    {
                        new OptionsItemKeybind(containerRect, "Take Screenshot", ConfigManager.KeyScreenshot),
                    })
                }),
                new OptionsSection("Miscellaneous", UserInterface.OptionsMisc, new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Navigation & Maintenance", new List<OptionsItem>()
                    {
                        new OptionsItemOpenGameFolder(containerRect, "Open Game Folder"),
                        new OptionsItemUpdateRankedStatuses(containerRect, "Update Map Ranked Statuses"),
                        new OptionsItemUpdateOnlineOffsets(containerRect, "Update Map Online Offsets")
                    }),
                    new OptionsSubcategory("Installed Games", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Load Songs From Other Installed Games", ConfigManager.AutoLoadOsuBeatmaps)
                        {
                            Tags = new List<string> {"osu!", "other games", "db", "etterna", "sm", "stepmania"}
                        },
                        new OptionsItemDetectOtherGames(containerRect, "Detect Songs From Other Installed Games")
                        {
                            Tags = new List<string> {"osu!", "other games", "db", "etterna", "sm", "stepmania"}
                        },
                    }),
                    new OptionsSubcategory("Notifications", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Notifications From Bottom-To-Top", ConfigManager.DisplayNotificationsBottomToTop),
                        new OptionsItemCheckbox(containerRect, "Display Online Friend Notifications", ConfigManager.DisplayFriendOnlineNotifications),
                    }),
                    new OptionsSubcategory("Song Select", new List<OptionsItem>()
                    {
                        new OptionsItemPrioritizedGameMode(containerRect, "Prioritized Game Mode"),
                        new OptionsSlider(containerRect, "Prioritized 4K Difficulty", ConfigManager.PrioritizedMapDifficulty4K, i => $"{i / 10f:0.0}"),
                        new OptionsSlider(containerRect, "Prioritized 7K Difficulty", ConfigManager.PrioritizedMapDifficulty7K, i => $"{i / 10f:0.0}"),
                        new OptionsItemSuggestDifficulty(containerRect, "Suggest Difficulty from Overall Rating")
                    }),
                    // new OptionsSubcategory("Beta", new List<OptionsItem>()
                    // {
                    //     new OptionsItemCheckbox(containerRect, "Skip Beta Splash Screen", ConfigManager.SkipSplashScreen),
                    // }),
                }),
                new OptionsSection("Advanced", FontAwesome.Get(FontAwesomeIcon.fa_open_folder), new List<OptionsSubcategory>
                {
                    new OptionsSubcategory("Video", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Lower FPS On Inactive Window", ConfigManager.LowerFpsOnWindowInactive),
                        new OptionsItemCheckbox(containerRect, "Enable High Process Priority", ConfigManager.EnableHighProcessPriority),
                        new OptionsItemCheckbox(containerRect, "Prefer Wayland", ConfigManager.PreferWayland)
                        {
                            Tags = new List<string> {"linux"}
                        },
                        new OptionsSlider(containerRect, "Editor ImGui Scale", ConfigManager.EditorImGuiScalePercentage)
                    }),
                    new OptionsSubcategory("Audio", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Use Smooth Audio/Frame Timing During Gameplay", ConfigManager.SmoothAudioTimingGameplay),
                        new OptionsItemCheckbox(containerRect, "Use Smooth Audio Start Timing During Gameplay", ConfigManager.SmoothAudioStart)
                    }),
                    new OptionsSubcategory("Gameplay", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display Gameplay Overlay (Shift + F6)", ConfigManager.DisplayGameplayOverlay),
                        new OptionsItemCheckbox(containerRect, "Display Notifications During Gameplay", ConfigManager.DisplayNotificationsInGameplay),
                        new OptionsItemCheckbox(containerRect, "Show Spectators", ConfigManager.ShowSpectators),
                        new OptionsItemCheckbox(containerRect, "Display Ranked Accuracy With Custom Judgements", ConfigManager.DisplayRankedAccuracy),
                        new OptionsSlider(containerRect, "Hit Error Fade Time", ConfigManager.HitErrorFadeTime, i => $"{i / 1000f:0.0} sec"),
                        new OptionsItemCheckbox(containerRect, "Enable Combo Alerts", ConfigManager.DisplayComboAlerts),
                        new OptionsItemCheckbox(containerRect, "[Donator] Enable Real-time Top 5 Online Scoreboard", ConfigManager.EnableRealtimeOnlineScoreboard),
                        new OptionsItemCheckbox(containerRect, "Display Unbeatable Scores", ConfigManager.DisplayUnbeatableScoresDuringGameplay),
                        new OptionsItemCheckbox(containerRect, "Keep Playing Upon Failing", ConfigManager.KeepPlayingUponFailing)
                    }),
                    new OptionsSubcategory("Skin", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Display 1v1 Tournament Overlay", ConfigManager.Display1v1TournamentOverlay),
                        new OptionsItemCheckbox(containerRect, "Display 1v1 Playfield Scores", ConfigManager.TournamentDisplay1v1PlayfieldScores),
                        new OptionsItemCheckbox(containerRect, "Reload Skin On Change", ConfigManager.ReloadSkinOnChange)
                    }),
                    new OptionsSubcategory("Input", new List<OptionsItem>()
                    {
                        new OptionsItemKeybind(containerRect, "Toggle Playtest Autoplay", ConfigManager.KeyTogglePlaytestAutoplay),
                        new OptionsItemCheckbox(containerRect, "Invert Editor Scrolling", ConfigManager.InvertEditorScrolling),
                        new OptionsItemCheckbox(containerRect, "Invert Scrolling", ConfigManager.InvertScrolling)
                    }),
                    new OptionsSubcategory("Miscellaneous", new List<OptionsItem>()
                    {
                        new OptionsItemCheckbox(containerRect, "Automatically Log Into The Server", ConfigManager.AutoLoginToServer),
                        new OptionsItemCheckbox(containerRect, "Display Song Request Notifications", ConfigManager.DisplaySongRequestNotifications),
                        new OptionsItemCheckbox(containerRect, "Display Warning For Pausing", ConfigManager.DisplayPauseWarning),
                        new OptionsItemCheckbox(containerRect, "Display Warning For Failing", ConfigManager.DisplayFailWarning),
                        new OptionsItemCheckbox(containerRect, "Display Menu Audio Visualizer", ConfigManager.DisplayMenuAudioVisualizer),
                        new OptionsItemCheckbox(containerRect, "Display Failed Local Scores", ConfigManager.DisplayFailedLocalScores),
                        new OptionsItemCheckbox(containerRect, "Delete Original File After Import", ConfigManager.DeleteOriginalFileAfterImport),
                        new OptionsItemCheckbox(containerRect, "Discord Rich Presence", ConfigManager.DiscordRichPresence)
                    }),
                }),
            };

            SelectedSection = new Bindable<OptionsSection>(Sections.First()) { Value = Sections.First() };
        }

        /// <summary>
        /// </summary>
        private void CreateHeader() => Header = new OptionsHeader(SelectedSection, Width, Sidebar.Width, CurrentSearchQuery,
            IsOptionFocused)
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

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSearchChanged(object sender, BindableValueChangedEventArgs<string> e)
        {
            ScheduleUpdate(() =>
            {
                // User searched nothing, so clear the search and select the first section again
                if (SelectedSection.Value.Name == string.Empty)
                {
                    ClearSearchAndReiRenitializeSections(SelectedSection.Value);

                    if (string.IsNullOrEmpty(e.Value))
                    {
                        SelectedSection.Value = Sections.First();
                        return;
                    }
                }

                var items = new List<OptionsItem>();

                foreach (var section in Sections)
                {
                    foreach (var category in section.Subcategories)
                        items.AddRange(category.Items.FindAll(x => x.Name.Text.ToLower().Contains(e.Value.ToLower())
                                                                   || x.Tags.Any(y => y.ToLower().Contains(e.Value.ToLower()))));
                }

                // Create a temporary section
                var categoryName = $"{items.Count} Search Result";

                if (items.Count > 1 || items.Count == 0)
                    categoryName += "s";

                var newSection = new OptionsSection(string.Empty, FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass),
                    new List<OptionsSubcategory> { new OptionsSubcategory(categoryName, items) });

                ContentContainers.Add(newSection, new OptionsContentContainer(newSection, Content.Size));
                SelectedSection.Value = newSection;
            });
        }

        /// <summary>
        ///     Handles when removing all the text from the search field, and reinitializing the containers
        ///     so they regain their initial state
        /// </summary>
        /// <param name="section"></param>
        private void ClearSearchAndReiRenitializeSections(OptionsSection section)
        {
            var searchedSection = section;

            if (!ContentContainers.ContainsKey(searchedSection))
                return;

            var container = ContentContainers[searchedSection];

            ContentContainers.Remove(searchedSection);

            foreach (var contentContainer in ContentContainers)
                contentContainer.Value.ReInitialize();

            container.Destroy();
            Sections.Remove(searchedSection);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSectionChanged(object sender, BindableValueChangedEventArgs<OptionsSection> e)
        {
            ScheduleUpdate(() =>
            {
                if (e.OldValue.Name == string.Empty)
                    ClearSearchAndReiRenitializeSections(e.OldValue);

                SetActiveContentContainer();

                // Update previous and newest section to make sure button hover status is up-to-date
                UpdateSection(e.OldValue);
                UpdateSection(e.Value);
            });
        }

        private void UpdateSection(OptionsSection section) => section?.Subcategories.ForEach(x =>
            x.Items.ForEach(y => y.Update(new GameTime())));

        /// <summary>
        ///     Looks through each section and checks if any of the keybinds are currently focused.
        ///     This sets the bindable, so that the search textbox knows when to become always active or not
        /// </summary>
        private void SetOptionFocusedState()
        {
            var isFocused = false;

            foreach (var section in Sections)
            {
                foreach (var category in section.Subcategories)
                {
                    foreach (var item in category.Items)
                    {
                        if (item.Focused)
                        {
                            isFocused = true;
                        }
                    }
                }
            }

            IsOptionFocused.Value = isFocused;
        }
    }
}
