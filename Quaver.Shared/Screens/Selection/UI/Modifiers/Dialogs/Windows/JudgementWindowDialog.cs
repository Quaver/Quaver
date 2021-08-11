using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Form;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private Sprite Panel { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HeaderBackground { get; set; }

        /// <summary>
        /// </summary>
        private Sprite FooterBackground { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ContentBackground { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus FootNote { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HeaderBanner { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TextPresets { get; set; }

        /// <summary>
        /// </summary>
        private Sprite CloseButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton DeleteButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton ResetButton { get; set; }

        /// <summary>
        /// </summary>
        private IconButton CreateButton { get; set; }

        /// <summary>
        /// </summary>
        private JudgementWindowScrollContainer JudgementWindowScrollContainer { get; set; }

        /// <summary>
        /// </summary>
        private Dictionary<Judgement, JudgementWindowSlider> Sliders { get; set; }

        /// <summary>
        /// </summary>
        private TextboxTabControl TabControl { get; set; }

        /// <summary>
        /// </summary>
        private LabelledTextbox NameTextbox { get; set; }

        /// <summary>
        /// </summary>
        private JudgementWindowComboBreakDropdown ComboBreak { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public JudgementWindowDialog() : base(0)
        {
            FadeTo(0.75f, Easing.Linear, 200);
            CreateContent();

            JudgementWindowsDatabaseCache.Selected.ValueChanged += OnSelectedWindowsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreatePanel();
            CreateTitle();
            CreateHeaderBackground();
            CreateFooterBackground();
            CreateContentBackground();
            CreateFootNote();
            CreateHeaderBanner();
            CreateTextPresets();
            CreateCloseButton();
            CreateCreateButton();
            CreateDeleteButton();
            CreateResetButton();
            CreateJudgementWindowScrollContainer();
            CreateNameTextbox();
            CreateSliders();
            CreateComboBreakDropdown();

            HandleButtonVisibility();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var selected = JudgementWindowsDatabaseCache.Selected.Value;

            if (selected.IsDefault)
            {
                NameTextbox.Textbox.Button.IsClickable = false;
                NameTextbox.Textbox.Focused = false;
            }
            else
            {
                NameTextbox.Textbox.Button.IsClickable = true;
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Last() == this)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                    Close();

                if (MouseManager.IsUniqueClick(MouseButton.Left) && !Panel.IsHovered())
                    Close();
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            JudgementWindowsDatabaseCache.Selected.ValueChanged -= OnSelectedWindowsChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Performs a closing animation on the dialog
        /// </summary>
        public void Close()
        {
            FadeTo(0, Easing.Linear, 200);

            Panel.Visible = false;

            ThreadScheduler.Run(JudgementWindowsDatabaseCache.UpdateAll);
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 200);
        }

        /// <summary>
        ///     Creates the panel container for the dialog
        /// </summary>
        private void CreatePanel()
        {
            Panel = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(1308, 704),
                Image = UserInterface.JudgementWindowPanel
            };
        }

        /// <summary>
        ///     Creates the title of the panel
        /// </summary>
        private void CreateTitle()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Customize Judgement Windows".ToUpper(), 26)
            {
                Parent = Panel,
                Y = -5
            };

            Title.Y -= Title.Height;
        }

        /// <summary>
        ///     Creates the background that contains the header
        /// </summary>
        private void CreateHeaderBackground()
        {
            HeaderBackground = new Sprite
            {
                Parent = Panel,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(Panel.Width - 4, 67),
                Y = 2,
                Tint = ColorHelper.HexToColor("#242424")
            };
        }

        /// <summary>
        ///     Creates the background that contains the footer
        /// </summary>
        private void CreateFooterBackground()
        {
            FooterBackground = new Sprite
            {
                Parent = Panel,
                Alignment = Alignment.BotCenter,
                Size = HeaderBackground.Size,
                Y = -HeaderBackground.Y,
                Tint = HeaderBackground.Tint
            };
        }

        /// <summary>
        ///     Creates the background that contains the content of the dialog
        /// </summary>
        private void CreateContentBackground()
        {
            ContentBackground = new Sprite
            {
                Parent = Panel,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(HeaderBackground.Width, Panel.Height - HeaderBackground.Height - FooterBackground.Height),
                Y = HeaderBackground.Y + HeaderBackground.Height,
                Tint = ColorHelper.HexToColor("#2F2F2F")
            };
        }

        /// <summary>
        ///     Tells the user that their score must be passing on Standard*
        /// </summary>
        private void CreateFootNote()
        {
            FootNote = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "NOTE: Scores with custom judgement windows must be passing on Standard* in order to be ranked.", 24)
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidLeft,
                X = 16
            };
        }

        /// <summary>
        /// </summary>
        private void CreateHeaderBanner()
        {
            HeaderBanner = new Sprite
            {
                Parent = HeaderBackground,
                Size = new ScalableVector2(389, HeaderBackground.Height),
                Image = UserInterface.JudgementWindowHeaderBanner
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTextPresets()
        {
            TextPresets = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "PRESETS", 24)
            {
                Parent = HeaderBanner,
                Alignment = Alignment.MidLeft,
                X = FootNote.X
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCloseButton()
        {
            CloseButton = new IconButton(UserInterface.JudgementWindowCloseButton, (o, e) => Close())
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(221, 40),
                X = -12,
                Y = 2
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDeleteButton()
        {
            DeleteButton = new IconButton(UserInterface.JudgementWindowDeleteButton, (o, e) => DeleteSelectedPreset())
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                Size = CreateButton.Size,
                X = CreateButton.X - CreateButton.Width + CreateButton.X,
                Y = CloseButton.Y
            };
        }

        /// <summary>
        /// </summary>
        private void CreateResetButton()
        {
            ResetButton = new IconButton(UserInterface.JudgementWindowResetButton, (o, e) => ResetSelectedPreset())
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                Size = DeleteButton.Size,
                X = DeleteButton.X - DeleteButton.Width + CreateButton.X,
                Y = CloseButton.Y
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCreateButton()
        {
            CreateButton = new IconButton(UserInterface.JudgementWindowCreateButton, (o, e) => CreateNewPreset())
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(180, 36),
                X = CloseButton.X,
                Y = CloseButton.Y
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementWindowScrollContainer()
        {
            JudgementWindowScrollContainer = new JudgementWindowScrollContainer(JudgementWindowsDatabaseCache.Presets,
                new ScalableVector2(306, ContentBackground.Height))
            {
                Parent = ContentBackground
            };
        }

        /// <summary>
        /// </summary>
        private void CreateNameTextbox()
        {
            NameTextbox = new LabelledTextbox(300, "Preset Name:", 24, 40, 22, 14,
                "Enter the name of the preset", JudgementWindowsDatabaseCache.Selected.Value.Name)
            {
                Parent = ContentBackground,
                Y = 28,
                X = JudgementWindowScrollContainer.Width + 22,
                Textbox =
                {
                    StoppedTypingActionCalltime = 1,
                    AllowSubmission = false,
                    OnSubmit = HandleTextboxSubmission,
                    OnStoppedTyping = HandleTextboxSubmission,
                    Border =
                    {
                        Tint = ColorHelper.HexToColor("#5B5B5B")
                    }
                }
            };

            NameTextbox.Textbox.Y = NameTextbox.Label.Y;
            NameTextbox.Textbox.X = NameTextbox.Label.X + NameTextbox.Label.Width + 12;
            NameTextbox.Textbox.Parent = NameTextbox.Label;
            NameTextbox.Textbox.Alignment = Alignment.MidLeft;
        }

        private void CreateComboBreakDropdown()
        {
            ComboBreak = new JudgementWindowComboBreakDropdown(JudgementWindowsDatabaseCache.Selected)
            {
                Parent = ContentBackground,
                Alignment = Alignment.TopRight,
                Y = NameTextbox.Y - 10,
                X = -22
            };
        }

        /// <summary>
        ///     Creates a new judgement window preset
        /// </summary>
        private void CreateNewPreset() => ThreadScheduler.Run(() =>
        {
            lock (JudgementWindowScrollContainer.AvailableItems)
            lock (JudgementWindowScrollContainer.Pool)
            {
                var windows = new JudgementWindows {Name = "Preset"};

                var presetCount = JudgementWindowsDatabaseCache.Presets.FindAll(x => !x.IsDefault).Count;
                JudgementWindowsDatabaseCache.Insert(windows);

                windows.Name = $"Preset {presetCount + 1}";
                JudgementWindowsDatabaseCache.Update(windows);

                JudgementWindowScrollContainer.AddObject(windows);
                JudgementWindowsDatabaseCache.Selected.Value = windows;
            }
        });

        /// <summary>
        ///     Deletes the selected preset
        /// </summary>
        private void DeleteSelectedPreset()
        {
            ThreadScheduler.Run(() =>
            {
                lock (JudgementWindowScrollContainer.AvailableItems)
                lock (JudgementWindowScrollContainer.Pool)
                {
                    var index = JudgementWindowsDatabaseCache.Presets.IndexOf(JudgementWindowsDatabaseCache.Selected.Value);
                    JudgementWindowsDatabaseCache.Delete(JudgementWindowsDatabaseCache.Selected.Value);
                    JudgementWindowScrollContainer.Remove(JudgementWindowsDatabaseCache.Selected.Value);

                    JudgementWindowsDatabaseCache.Selected.Value = JudgementWindowsDatabaseCache.Presets[index - 1];
                }
            });
        }

        /// <summary>
        /// </summary>
        private void ResetSelectedPreset()
        {
            var defaultWindows = JudgementWindowsDatabaseCache.Standard;

            foreach (Judgement j in Enum.GetValues(typeof(Judgement)))
            {
                if (j == Judgement.Ghost)
                    continue;

                Sliders[j].Bindable.Value = (int) defaultWindows.GetValueFromJudgement(j);
            }
        }

        /// <summary>
        ///     Makes sure that the buttons are correctly displayed for the selected windows
        /// </summary>
        private void HandleButtonVisibility()
        {
            if (JudgementWindowsDatabaseCache.Selected.Value.IsDefault)
            {
                DeleteButton.Visible = false;
                DeleteButton.IsClickable = false;
                ResetButton.Visible = false;
                ResetButton.IsClickable = false;
            }
            else
            {
                DeleteButton.Visible = true;
                DeleteButton.IsClickable = true;
                ResetButton.Visible = true;
                ResetButton.IsClickable = true;
            }
        }

        /// <summary>
        ///     Creates the sliders to modify the windows
        /// </summary>
        private void CreateSliders()
        {
            Sliders = new Dictionary<Judgement, JudgementWindowSlider>();

            TabControl = new TextboxTabControl(new List<Textbox>()
            {
                NameTextbox.Textbox
            }) {Parent = this};

            foreach (Judgement judgement in Enum.GetValues(typeof(Judgement)))
            {
                if (judgement == Judgement.Ghost)
                    continue;

                var slider = new JudgementWindowSlider(judgement)
                {
                    Parent = ContentBackground,
                    X = JudgementWindowScrollContainer.Width + 22,
                    Y = NameTextbox.Textbox.Y + NameTextbox.Textbox.Height + 70 + 80 * (int) judgement
                };

                Sliders.Add(judgement, slider);
                TabControl.AddTextbox(slider.ValueTextbox);
            }
        }

        /// <summary>
        ///     Controls the visibility of the buttons upon switching presets
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedWindowsChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e)
        {
            HandleButtonVisibility();

            ScheduleUpdate(() =>
            {
                NameTextbox.Textbox.RawText = e.Value.Name;
                NameTextbox.Textbox.InputText.Text = e.Value.Name;
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="s"></param>
        private void HandleTextboxSubmission(string s)
        {
            JudgementWindowsDatabaseCache.Selected.Value.Name = s;
            JudgementWindowsDatabaseCache.Selected.Value = JudgementWindowsDatabaseCache.Selected.Value;
        }
    }
}