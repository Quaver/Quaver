using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows
{
    public class JudgementWindowSlider : Container
    {
        /// <summary>
        ///     The judgement this slider represents
        /// </summary>
        private Judgement Judgement { get; }

        /// <summary>
        /// </summary>
        private Sprite JudgementSprite { get; set; }

        /// <summary>
        /// </summary>
        private Slider Slider { get; set; }

        /// <summary>
        /// </summary>
        public BindableInt Bindable { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus MillisecondValue { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="judgement"></param>
        public JudgementWindowSlider(Judgement judgement)
        {
            Judgement = judgement;

            Size = new ScalableVector2(938, 30);

            Bindable = new BindableInt((int) JudgementWindowsDatabaseCache.Selected.Value.GetValueFromJudgement(judgement), 1, 500);

            Bindable.ValueChanged += (sender, args) =>
            {
                SetValue(args.Value);
                SetMillisecondValue();
            };

            CreateJudgementSprite();
            CreateSlider();
            CreateMillisecondValue();
            SetToValue();

            JudgementWindowsDatabaseCache.Selected.ValueChanged += OnJudgementWindowsChanged;
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            JudgementWindowsDatabaseCache.Selected.ValueChanged -= OnJudgementWindowsChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementSprite()
        {
            var img = SkinManager.Skin.Judgements[Judgement].First();

            JudgementSprite = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Image = img,
                Size = new ScalableVector2(120, 0),
            };

            // ReSharper disable once PossibleLossOfFraction
            JudgementSprite.Height = img.Width / img.Height * JudgementSprite.Width;
        }

        /// <summary>
        /// </summary>
        private void CreateSlider()
        {
            Slider = new Slider(Bindable, new Vector2(725, 4), FontAwesome.Get(FontAwesomeIcon.fa_circle))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = JudgementSprite.X + JudgementSprite.Width + 18
            };

            Slider.ActiveColor.Tint = Colors.MainBlue;
        }

        /// <summary>
        /// </summary>
        private void CreateMillisecondValue()
        {
            MillisecondValue = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0 ms", 24)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Slider.X + Slider.Width + 22
            };
        }

        /// <summary>
        /// </summary>
        private void SetMillisecondValue() => MillisecondValue.Text = $"{Bindable.Value} ms";

        /// <summary>
        ///     Called to set the sliders back to the real value when the windows are changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJudgementWindowsChanged(object sender, BindableValueChangedEventArgs<JudgementWindows> e)
            => SetToValue();

        /// <summary>
        /// </summary>
        private void SetToValue()
        {
            Bindable.Value = (int) JudgementWindowsDatabaseCache.Selected.Value.GetValueFromJudgement(Judgement);
            SetMillisecondValue();

            var windows = JudgementWindowsDatabaseCache.Selected.Value;

            var gray = ColorHelper.HexToColor("#5B5B5B");

            if (windows.IsDefault)
            {
                Slider.IsClickable = false;
                Slider.ActiveColor.Tint = gray;
                Slider.ProgressBall.Tint = Color.White;
                Slider.Tint = gray;
                Slider.ProgressBall.Image = UserInterface.SliderProgressBall;
            }
            else
            {
                Slider.ActiveColor.Tint = Colors.MainBlue;
                Slider.ProgressBall.Tint = Colors.MainBlue;
                Slider.Tint = ColorHelper.HexToColor("#5B5B5B");
                Slider.ProgressBall.Image = FontAwesome.Get(FontAwesomeIcon.fa_circle);
                Slider.IsClickable = true;
            }
        }

        /// <summary>
        ///     Sets the bindable's value
        /// </summary>
        /// <param name="val"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetValue(int val)
        {
            var selected = JudgementWindowsDatabaseCache.Selected.Value;

            switch (Judgement)
            {
                case Judgement.Marv:
                    selected.Marvelous = val;
                    break;
                case Judgement.Perf:
                    selected.Perfect = val;
                    break;
                case Judgement.Great:
                    selected.Great = val;
                    break;
                case Judgement.Good:
                    selected.Good = val;
                    break;
                case Judgement.Okay:
                    selected.Okay = val;
                    break;
                case Judgement.Miss:
                    selected.Miss = val;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}