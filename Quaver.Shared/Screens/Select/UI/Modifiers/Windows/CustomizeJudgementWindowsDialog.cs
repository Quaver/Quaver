using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;

namespace Quaver.Shared.Screens.Select.UI.Modifiers.Windows
{
    public class CustomizeJudgementWindowsDialog : DialogScreen
    {
        private SpriteTextBitmap Header { get; set; }

        private Sprite CustomizeContainer { get; set; }

        private Sprite SubHeaderBackground { get; set; }

        private Sprite FooterBackground { get; set; }

        private JudgementWindowContainer WindowContainer { get; set; }

        private Dictionary<Judgement, BindableInt> BindableSliderValues { get; }

        private Dictionary<Judgement, Slider> Sliders { get; } = new Dictionary<Judgement, Slider>();

        private Button EditButton { get; set; }

        private Button DeleteButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="backgroundAlpha"></param>
        public CustomizeJudgementWindowsDialog(float backgroundAlpha) : base(backgroundAlpha)
        {
            BindableSliderValues = new Dictionary<Judgement, BindableInt>()
            {
                {Judgement.Marv, new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.Marvelous, 1, 500)},
                {Judgement.Perf, new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.Perfect, 1, 500)},
                {Judgement.Great, new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.Great, 1, 500)},
                {Judgement.Good, new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.Good, 1, 500)},
                {Judgement.Okay, new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.Okay, 1, 500)},
                {Judgement.Miss, new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.Miss, 1, 500)},
            };

            CreateContent();
            StyleSliders();
            JudgementWindowsDatabaseCache.Selected.ValueChanged += OnSelectedWindowChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContainer();
            CreateHeader();
            CreateSubHeaders();
            CreateFooter();
            CreateWindowContainer();
            CreateSliders();
        }

        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            JudgementWindowsDatabaseCache.Selected.ValueChanged -= OnSelectedWindowChanged;

            BindableSliderValues.Values.ToList().ForEach(x => x.Dispose());
            base.Destroy();
        }


        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Last() != this)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            CustomizeContainer = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(930, 500),
                Tint = ColorHelper.HexToColor("#2f2f2f")
            };

            CustomizeContainer.AddBorder(Colors.MainAccent, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new SpriteTextBitmap(FontsBitmap.GothamRegular, "Customize Judgement Windows")
            {
                Parent = CustomizeContainer,
                FontSize = 18,
                Y = -24
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSubHeaders()
        {
            SubHeaderBackground = new Sprite
            {
                Parent = CustomizeContainer,
                Y = 2,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(CustomizeContainer.Width - 4, 47),
                Tint = ColorHelper.HexToColor("#101010")
            };

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextBitmap(FontsBitmap.GothamBold, "Presets")
            {
                Parent = SubHeaderBackground,
                Alignment = Alignment.MidLeft,
                X = 18,
                FontSize = 20
            };

            var addButton = new BorderedTextButton("Add", Color.Lime, (sender, args) =>
            {
                var windows = new JudgementWindows()
                {
                    Name = "Preset"
                };

                var id = JudgementWindowsDatabaseCache.Insert(windows);

                windows.Name = $"Preset {windows.Id}";
                JudgementWindowsDatabaseCache.Update(windows);

                WindowContainer.AddObject(windows);
                JudgementWindowsDatabaseCache.Selected.Value = windows;
            })
            {
                Parent = SubHeaderBackground,
                Alignment = Alignment.MidRight,
                X = -15,
                Text =
                {
                    FontSize = 14,
                    Font = Fonts.Exo2SemiBold
                }
            };

            addButton.Size = new ScalableVector2(addButton.Width * 0.85f, addButton.Height * 0.85f);

            EditButton = new BorderedTextButton("Edit Name", Color.Yellow, (sender, args) =>
            {
                DialogManager.Show(new JudgementWindowRenameDialog(JudgementWindowsDatabaseCache.Selected.Value, WindowContainer));
            })
            {
                Parent = SubHeaderBackground,
                Alignment = Alignment.MidRight,
                X = addButton.X - addButton.Width - 12,
                Text =
                {
                    FontSize = 14,
                    Font = Fonts.Exo2SemiBold
                }
            };

            EditButton.Size = new ScalableVector2(EditButton.Width * 0.85f, EditButton.Height * 0.85f);

            DeleteButton = new BorderedTextButton("Delete", Color.Crimson, (sender, args) =>
            {
                var index = JudgementWindowsDatabaseCache.Presets.IndexOf(JudgementWindowsDatabaseCache.Selected.Value);

                JudgementWindowsDatabaseCache.Delete(JudgementWindowsDatabaseCache.Selected.Value);
                WindowContainer.Remove(JudgementWindowsDatabaseCache.Selected.Value);

                JudgementWindowsDatabaseCache.Selected.Value = JudgementWindowsDatabaseCache.Presets[index - 1];
            })
            {
                Parent = SubHeaderBackground,
                Alignment = Alignment.MidRight,
                X = EditButton.X - EditButton.Width - 12,
                Text =
                {
                    FontSize = 14,
                    Font = Fonts.Exo2SemiBold
                }
            };

            DeleteButton.Size = new ScalableVector2(DeleteButton.Width * 0.85f, DeleteButton.Height * 0.85f);
        }

        /// <summary>
        /// </summary>
        private void CreateFooter()
        {
            FooterBackground = new Sprite
            {
                Parent = CustomizeContainer,
                Y = -2,
                Alignment = Alignment.BotCenter,
                Size = new ScalableVector2(CustomizeContainer.Width - 4, 49),
                Tint = ColorHelper.HexToColor("#101010")
            };

            var closeButton = new BorderedTextButton("Close", Color.Crimson, (sender, args) => Close())
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidRight,
                X = -15,
                Text =
                {
                    FontSize = 14,
                    Font = Fonts.Exo2SemiBold
                }
            };

            closeButton.Size = new ScalableVector2(closeButton.Width * 0.85f, closeButton.Height * 0.85f);

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextBitmap(FontsBitmap.GothamBold, "NOTE: Scores with custom windows must be passing on the Standard windows in order to be ranked.")
            {
                Parent = FooterBackground,
                Alignment = Alignment.MidLeft,
                X = 15,
                FontSize = 15
            };
        }

        private void CreateWindowContainer()
        {
            WindowContainer = new JudgementWindowContainer(JudgementWindowsDatabaseCache.Presets)
            {
                Parent = CustomizeContainer,
                Y = SubHeaderBackground.Y + SubHeaderBackground.Height,
                X = 2
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite()
            {
                Parent = WindowContainer,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(1, WindowContainer.Height - 1)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSliders()
        {
            for (var i = 0; i < Enum.GetValues(typeof(Judgement)).Length; i++)
            {
                var judgement = (Judgement) i;

                if (judgement == Judgement.Ghost)
                    continue;

                var name = new SpriteTextBitmap(FontsBitmap.GothamRegular, ((Judgement) i).ToString())
                {
                    Parent = CustomizeContainer,
                    Y = SubHeaderBackground.Height + 40 + i * 60,
                    X = WindowContainer.X + WindowContainer.Width + 18,
                    Tint = SkinManager.Skin.Keys[GameMode.Keys4].JudgeColors[judgement],
                    FontSize = 18
                };

                var slider = new Slider(BindableSliderValues[judgement], new Vector2(550, 3), FontAwesome.Get(FontAwesomeIcon.fa_circle))
                {
                    Parent = CustomizeContainer,
                    X = name.X + 70,
                    Y = name.Y + name.Height / 2f
                };

                Sliders.Add(judgement, slider);
                var value = new SpriteTextBitmap(FontsBitmap.GothamRegular, BindableSliderValues[judgement].Value.ToString(), false)
                {
                    Parent = CustomizeContainer,
                    Alignment = Alignment.TopRight,
                    X = -18,
                    Y = name.Y,
                    FontSize = 18
                };

                slider.BindedValue.ValueChanged += (sender, args) =>
                {
                    value.Text = args.Value.ToString();

                    switch (judgement)
                    {
                        case Judgement.Marv:
                            JudgementWindowsDatabaseCache.Selected.Value.Marvelous = args.Value;
                            break;
                        case Judgement.Perf:
                            JudgementWindowsDatabaseCache.Selected.Value.Perfect = args.Value;
                            break;
                        case Judgement.Great:
                            JudgementWindowsDatabaseCache.Selected.Value.Great = args.Value;
                            break;
                        case Judgement.Good:
                            JudgementWindowsDatabaseCache.Selected.Value.Good = args.Value;
                            break;
                        case Judgement.Okay:
                            JudgementWindowsDatabaseCache.Selected.Value.Okay = args.Value;
                            break;
                        case Judgement.Miss:
                            JudgementWindowsDatabaseCache.Selected.Value.Miss = args.Value;
                            break;
                        default:
                            break;
                    }
                };
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedWindowChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e)
        {
            BindableSliderValues[Judgement.Marv].Value = (int) e.Value.Marvelous;
            BindableSliderValues[Judgement.Perf].Value = (int) e.Value.Perfect;
            BindableSliderValues[Judgement.Great].Value = (int) e.Value.Great;
            BindableSliderValues[Judgement.Good].Value = (int) e.Value.Good;
            BindableSliderValues[Judgement.Okay].Value = (int) e.Value.Okay;
            BindableSliderValues[Judgement.Miss].Value = (int) e.Value.Miss;

            StyleSliders();
        }

        private void StyleSliders()
        {
            foreach (var slider in Sliders.Values)
            {
                if (JudgementWindowsDatabaseCache.Selected.Value.IsDefault)
                {
                    slider.Tint = ColorHelper.HexToColor("#4b4b4b");
                    slider.ProgressBall.Tint = ColorHelper.HexToColor("#4b4b4b");
                    slider.IsClickable = false;

                    EditButton.Visible = false;
                    EditButton.IsClickable = false;
                    DeleteButton.Visible = false;
                    DeleteButton.IsClickable = false;
                }
                else
                {
                    slider.Tint = Color.White;
                    slider.ProgressBall.Tint = Color.White;
                    slider.IsClickable = true;

                    EditButton.Visible = true;
                    EditButton.IsClickable = true;
                    DeleteButton.Visible = true;
                    DeleteButton.IsClickable = true;
                }
            }
        }

        private void Close()
        {
            ThreadScheduler.Run(JudgementWindowsDatabaseCache.UpdateAll);
            DialogManager.Dismiss(this);
        }
    }
}