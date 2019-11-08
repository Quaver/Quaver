/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Settings.Elements;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Window;

namespace Quaver.Shared.Screens.Settings
{
    public class SettingsDialog : DialogScreen
    {
        /// <summary>
        ///     The entire container for the settings menu
        /// </summary>
        public Sprite ContentContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite HeaderContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite FooterContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite DividerLine { get; private set; }

        /// <summary>
        ///     The button to save changes.
        /// </summary>
        private BorderedTextButton ApplyButton { get; set; }

        /// <summary>
        ///     The button to cancel existing changes
        /// </summary>
        private BorderedTextButton CloseButton { get; set; }

        /// <summary>
        ///     The list of available settings sections.
        /// </summary>
        public List<SettingsSection> Sections { get; private set; }

        /// <summary>
        ///     The currently selected options section.
        /// </summary>
        public SettingsSection SelectedSection { get; private set; }

        /// <summary>
        ///     A newly queued default skin, if the user chooses to change it.
        /// </summary>
        public DefaultSkins NewQueuedDefaultSkin { get; set; }

        /// <summary>
        ///     If the user has changed their resolution and it needs to change when they press OK.
        /// </summary>
        public Point NewQueuedScreenResolution { get; set; }

        /// <summary>
        ///     If true, the dialog won't close if the user presses escape.
        ///     This is used for when keybinds are changed (and the user wants to change it to escape).
        /// </summary>
        public bool PreventExitOnEscapeKeybindPress { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SettingsDialog() : base(0)
        {
            // Important. Make sure sure the default values that are sent in config if the values
            // are non nullable.
            NewQueuedDefaultSkin = ConfigManager.DefaultSkin.Value;
            NewQueuedScreenResolution = new Point(ConfigManager.WindowWidth.Value, ConfigManager.WindowHeight.Value);

            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, Alpha, 0.65f, 300));
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContentContainer();
            CreateHeader();
            CreateFooter();
            CreateDividerLine();
            CreateSections();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SkinManager.HandleSkinReloading();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (SkinManager.TimeSkinReloadRequested != 0 || PreventExitOnEscapeKeybindPress)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);

            if (KeyboardManager.IsUniqueKeyPress(Keys.Tab))
            {
                if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                    KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                {
                    var index = Sections.FindIndex(x => x == SelectedSection);
                    SwitchSelected(index == 0 ? Sections.Last() : Sections[index - 1]);
                }
                else
                {
                    var index = Sections.FindIndex(x => x == SelectedSection);
                    SwitchSelected(index == Sections.Count - 1 ? Sections.First() : Sections[index + 1]);
                }
            }
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainer() => ContentContainer = new Sprite
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(1200, 620),
            Tint = ColorHelper.HexToColor($"#414345"),
            Alpha = 1f
        };

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderContainer = new Sprite
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 45),
                Tint = ColorHelper.HexToColor($"#212121")
            };

            var headerFlag = new Sprite()
            {
                Parent = HeaderContainer,
                Size = new ScalableVector2(5, HeaderContainer.Height),
                Tint = Color.LightGray,
                Alpha = 0
            };

            var headerText = new SpriteText(Fonts.Exo2Medium, "Options Menu", 16)
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = headerFlag.X + 15
            };

            var exitButton = new ImageButton(FontAwesome.Get(FontAwesomeIcon.fa_times), (sender, args) => DialogManager.Dismiss())
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(25, 25)
            };

            exitButton.X -= exitButton.Width / 2f + 5;
        }

        /// <summary>
        /// </summary>
        private void CreateFooter()
        {
            FooterContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 50),
                Tint = ColorHelper.HexToColor("#212121"),
                Alignment = Alignment.BotLeft,
                Y = 1
            };

            CreateApplyButton();
            CreateCloseButton();
        }

        /// <summary>
        ///     Creates the button to save changes
        /// </summary>
        private void CreateApplyButton()
        {
            ApplyButton = new BorderedTextButton("Apply", Color.LimeGreen)
            {
                Parent = FooterContainer,
                Alignment = Alignment.MidRight,
                X = -20
            };

            ApplyButton.Clicked += (o, e) =>
            {
                // Determines whether we'll be dismissing the dialog if no changes have been made.
                var dismissDalog = true;

                // Handle skin reloads
                if (SkinManager.NewQueuedSkin != null && SkinManager.NewQueuedSkin != ConfigManager.Skin.Value
                    || NewQueuedDefaultSkin != ConfigManager.DefaultSkin.Value)
                {
                    ConfigManager.Skin.Value = SkinManager.NewQueuedSkin;
                    ConfigManager.DefaultSkin.Value = NewQueuedDefaultSkin;

                    Transitioner.FadeIn();
                    SkinManager.TimeSkinReloadRequested = GameBase.Game.TimeRunning;
                    dismissDalog = false;
                }

                // Handle screen resolution changes.
                if (NewQueuedScreenResolution.X != ConfigManager.WindowWidth.Value &&
                    NewQueuedScreenResolution.Y != ConfigManager.WindowHeight.Value)
                {
                    ConfigManager.WindowWidth.Value = NewQueuedScreenResolution.X;
                    ConfigManager.WindowHeight.Value = NewQueuedScreenResolution.Y;
                    WindowManager.ChangeScreenResolution(NewQueuedScreenResolution);

                    dismissDalog = false;
                }

                // Handle device period and buffer length changes.
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    if (ConfigManager.DevicePeriod.Value != Bass.GetConfig(Configuration.DevicePeriod)
                        || ConfigManager.DeviceBufferLengthMultiplier.Value !=
                        Bass.GetConfig(Configuration.DeviceBufferLength) / Bass.GetConfig(Configuration.DevicePeriod))
                    {
                        DialogManager.Show(new ConfirmCancelDialog(
                            "The game must be restarted to apply the new audio device properties. Exit the game now?",
                            (sender, args) =>
                            {
                                // Make sure the config is saved.
                                Task.Run(ConfigManager.WriteConfigFileAsync).Wait();

                                var game = GameBase.Game as QuaverGame;
                                game?.Exit();
                            }));
                    }
                }

                if (dismissDalog)
                    DialogManager.Dismiss(this);
            };
        }

        /// <summary>
        ///     Creates the button to cancel all changes
        /// </summary>
        private void CreateCloseButton()
        {
            CloseButton = new BorderedTextButton("Close", Color.Crimson)
            {
                Parent = FooterContainer,
                Alignment = Alignment.MidRight,
                X = ApplyButton.X - ApplyButton.Width - 20
            };

            CloseButton.Clicked += (o, e) => DialogManager.Dismiss(this);
        }
        /// <summary>
        /// </summary>
        private void CreateDividerLine() => DividerLine = new Sprite
        {
            Parent = ContentContainer,
            Size = new ScalableVector2(1, ContentContainer.Height - HeaderContainer.Height - FooterContainer.Height - 20),
            X = 230,
            Alignment = Alignment.MidLeft,
            Alpha = 0.75f
        };

        /// <summary>
        /// </summary>
        private void CreateSections()
        {
            Sections = new List<SettingsSection>
            {
                // Video
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_desktop_monitor), "Video", new List<Drawable>
                {
                    new SettingsResolution(this, "Screen Resolution"),
                    new SettingsBool(this, "Fullscreen", ConfigManager.WindowFullScreen),
                    new SettingsFpsLimiter(this),
                    new SettingsBool(this, "Display FPS Counter", ConfigManager.FpsCounter)
                }),
                // Audio
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_volume_up_interface_symbol), "Audio", new List<Drawable>
                {
                    new SettingsSlider(this, "Master Volume", ConfigManager.VolumeGlobal, x => $"{x}%"),
                    new SettingsSlider(this, "Music Volume", ConfigManager.VolumeMusic, x => $"{x}%"),
                    new SettingsSlider(this, "Effect Volume", ConfigManager.VolumeEffect, x => $"{x}%"),
                    new SettingsBool(this, "Enable Hitsounds", ConfigManager.EnableHitsounds),
                    new SettingsBool(this, "Enable Keysounds", ConfigManager.EnableKeysounds),
                    new SettingsBool(this, "Pitch Audio With Rate", ConfigManager.Pitched),
                    new SettingsSlider(this, "Global Audio Offset", ConfigManager.GlobalAudioOffset, x => $"{x} ms"),
                    new SettingsCalibrateOffset(this, "Calibrate Offset"),
                    new SettingsSlider(this, "Audio Device Period", ConfigManager.DevicePeriod, x => $"{x} ms"),
                    new SettingsSliderAudioBufferLength(this, "Audio Device Buffer Length",
                                ConfigManager.DeviceBufferLengthMultiplier,
                                ConfigManager.DevicePeriod,
                                (multiplier, period) => $"{multiplier * period} ms")
                }),
                // Gameplay
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), "Gameplay", new List<Drawable>
                {
                    new SettingsSlider(this, "Background Brightness", ConfigManager.BackgroundBrightness, x => $"{x}%"),
                    new SettingsSlider(this, "Scroll Speed (4 Keys)", ConfigManager.ScrollSpeed4K),
                    new SettingsSlider(this, "Scroll Speed (7 Keys)", ConfigManager.ScrollSpeed7K),
                    new SettingsScrollDirection(this, "Scroll Direction 4K", ConfigManager.ScrollDirection4K),
                    new SettingsScrollDirection(this, "Scroll Direction 7K", ConfigManager.ScrollDirection7K),
                    new SettingsBool(this, "Blur Background In Gameplay", ConfigManager.BlurBackgroundInGameplay),
                    new SettingsBool(this, "Display Timing Lines", ConfigManager.DisplayTimingLines),
                    new SettingsBool(this, "Display Song Time Progress", ConfigManager.DisplaySongTimeProgress),
                    new SettingsBool(this, "Display Song Time Progress Numbers", ConfigManager.DisplaySongTimeProgressNumbers),
                    new SettingsBool(this, "Display Judgement Counter", ConfigManager.DisplayJudgementCounter),
                    new SettingsBool(this, "Enable Combo Alerts", ConfigManager.DisplayComboAlerts),
                    new SettingsBool(this, "Display Scoreboard", ConfigManager.ScoreboardVisible),
                    new SettingsBool(this, "Tap to Pause", ConfigManager.TapToPause),
                    new SettingsBool(this, "Skip Results Screen After Quitting", ConfigManager.SkipResultsScreenAfterQuit),
                    new SettingsSlider(this, "Top Lane Cover Height", ConfigManager.LaneCoverTopHeight, x => $"{x}%"),
                    new SettingsSlider(this, "Bottom Lane Cover Height", ConfigManager.LaneCoverBottomHeight, x => $"{x}%"),
                    new SettingsBool(this, "Top Lane Cover", ConfigManager.LaneCoverTop),
                    new SettingsBool(this, "Bottom Lane Cover", ConfigManager.LaneCoverBottom),
                    new SettingsBool(this, "Display UI Elements Over Lane Covers", ConfigManager.UIElementsOverLaneCover),
                    new SettingsBool(this, "Smooth Accuracy Changes", ConfigManager.SmoothAccuracyChanges),
                    new SettingsBool(this, "Enable Battle Royale Background Flashing", ConfigManager.EnableBattleRoyaleBackgroundFlashing),
                    new SettingsBool(this, "Enable Battle Royale Alerts", ConfigManager.EnableBattleRoyaleAlerts),
                    new SettingsBool(this, "Show Spectators", ConfigManager.ShowSpectators)
                }),
                // Editor
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_beaker), "Editor", new List<Drawable>()
                {
                    new SettingsEditorSnapColors(this),
                    new SettingsBool(this, "Enable Hitsounds", ConfigManager.EditorEnableHitsounds),
                    new SettingsBool(this, "Enable Keysounds", ConfigManager.EditorEnableKeysounds),
                    new SettingsBool(this, "Enable Metronome", ConfigManager.EditorPlayMetronome),
                    new SettingsBool(this, "Play Metronome Half-Beats", ConfigManager.EditorMetronomePlayHalfBeats),
                    new SettingsBool(this, "Show Lane Divider Lines", ConfigManager.EditorShowLaneDividerLines),
                    new SettingsBool(this, "Only Show Measure Lines", ConfigManager.EditorOnlyShowMeasureLines),
                    new SettingsBool(this, "Anchor HitObjects At Midpoint", ConfigManager.EditorHitObjectsMidpointAnchored),
                }),
                // Skinning
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_pencil), "Skin", new List<Drawable>()
                {
                    new SettingsCustomSkin(this, "Custom Skin"),
                    new SettingsDefaultSkin(this, "Default Skin"),
                    new SettingsExportSkin(this, "Export Custom Skin")
                }),
                // Input
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_keyboard), "Input", new List<Drawable>
                {
                    new SettingsKeybindMultiple(this, "Gameplay Layout (4 Keys)", new List<Bindable<Keys>>
                    {
                        ConfigManager.KeyMania4K1,
                        ConfigManager.KeyMania4K2,
                        ConfigManager.KeyMania4K3,
                        ConfigManager.KeyMania4K4
                    }),
                    new SettingsKeybindMultiple(this, "Gameplay Layout (7 Keys)", new List<Bindable<Keys>>
                    {
                        ConfigManager.KeyMania7K1,
                        ConfigManager.KeyMania7K2,
                        ConfigManager.KeyMania7K3,
                        ConfigManager.KeyMania7K4,
                        ConfigManager.KeyMania7K5,
                        ConfigManager.KeyMania7K6,
                        ConfigManager.KeyMania7K7
                    }),
                    new SettingsKeybind(this, "Pause", ConfigManager.KeyPause),
                    new SettingsKeybind(this, "Skip Intro", ConfigManager.KeySkipIntro),
                    new SettingsKeybind(this, "Restart Map", ConfigManager.KeyRestartMap),
                    new SettingsKeybind(this, "Decrease Scroll Speed", ConfigManager.KeyDecreaseScrollSpeed),
                    new SettingsKeybind(this, "Increase Scroll Speed", ConfigManager.KeyIncreaseScrollSpeed),
                    new SettingsKeybind(this, "Decrease Map Offset", ConfigManager.KeyDecreaseMapOffset),
                    new SettingsKeybind(this, "Increase Map Offset", ConfigManager.KeyIncreaseMapOffset),
                    new SettingsKeybind(this, "Toggle Scoreboard Visibility", ConfigManager.KeyScoreboardVisible),
                    new SettingsKeybind(this, "Quick Exit", ConfigManager.KeyQuickExit),
                    new SettingsKeybind(this, "Toggle Chat Overlay", ConfigManager.KeyToggleOverlay),
                    new SettingsKeybind(this, "Interface - Select", ConfigManager.KeyNavigateSelect),
                    new SettingsKeybind(this, "Interface - Back", ConfigManager.KeyNavigateBack),
                    new SettingsKeybind(this, "Interface - Navigate Left", ConfigManager.KeyNavigateLeft),
                    new SettingsKeybind(this, "Interface - Navigate Right", ConfigManager.KeyNavigateRight),
                    new SettingsKeybind(this, "Interface - Navigate Up", ConfigManager.KeyNavigateUp),
                    new SettingsKeybind(this, "Interface - Navigate Down", ConfigManager.KeyNavigateDown),
                    new SettingsKeybind(this, "Editor - Pause/Play Track", ConfigManager.KeyEditorPausePlay),
                    new SettingsKeybind(this, "Editor - Decrease Audio Playback Rate", ConfigManager.KeyEditorDecreaseAudioRate),
                    new SettingsKeybind(this, "Editor - Increase Audio Playback Rate", ConfigManager.KeyEditorIncreaseAudioRate)
                }),
                // Misc
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_question_sign), "Miscellaneous", new List<Drawable>
                {
                    new SettingsBool(this, "Automatically Login To The Server", ConfigManager.AutoLoginToServer),
                    new SettingsBool(this, "Load Maps From Other Games", ConfigManager.AutoLoadOsuBeatmaps),
                    new SettingsBool(this, "Display Menu Audio Visualizer", ConfigManager.DisplayMenuAudioVisualizer),
                    new SettingsBool(this, "Display Failed Local Scores", ConfigManager.DisplayFailedLocalScores)
                })
            };

            SelectedSection = Sections.First();
            AlignSectionButtons();
        }

        /// <summary>
        ///     Sets the position of the section buttons
        /// </summary>
        private void AlignSectionButtons()
        {
            for (var i = 0; i < Sections.Count; i++)
            {
                var button = Sections[i].Button;
                button.Parent = ContentContainer;

                button.X = 10;
                button.Y = HeaderContainer.Height + 15 + button.Height * i + 15 * i;

                if (Sections[i] == SelectedSection)
                {
                    button.DisplayAsSelected();
                    Sections[i].Container.Parent = ContentContainer;
                }
                else
                {
                    button.DisplayAsDeselected();
                    Sections[i].Container.Visible = false;
                }
            }
        }

        /// <summary>
        /// </summary>
        public void SwitchSelected(SettingsSection section)
        {
            SelectedSection.Button.DisplayAsDeselected();
            SelectedSection.Container.Visible = false;
            SelectedSection.Container.Parent = null;

            SelectedSection = section;
            SelectedSection.Button.DisplayAsSelected();
            SelectedSection.Container.Visible = true;
            SelectedSection.Container.Parent = ContentContainer;

            Logger.Debug($"Switched to options section: {section.Name}", LogType.Runtime);
        }
    }
}
