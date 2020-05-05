using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs
{
    public class ResultsJudgementGraph : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private List<Drawable> Bars { get; } = new List<Drawable>();

        /// <summary>
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public ResultsJudgementGraph(Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Processor = processor;
            Size = size;
            Alpha = 0f;

            CreateBars();
        }

        /// <summary>
        /// </summary>
        private void CreateBars()
        {
            Bars.ForEach(x => x.Destroy());
            Bars.Clear();

            foreach (Judgement judgement in Enum.GetValues(typeof(Judgement)))
            {
                if (judgement >= Judgement.Ghost)
                    continue;

                Bars.Add(new ResultsJudgementGraphBar(judgement, Processor, new ScalableVector2(Width, 50)));
            }

            var heightSum = Bars.First().Height * Bars.Count;
            var heightPer = (Height - heightSum) / (Bars.Count + 1);

            for (var i = 0; i < Bars.Count; i++)
            {
                var item = Bars[i];

                item.Parent = this;
                item.Y = heightPer;

                if (i != 0)
                {
                    var last = Bars[i - 1];
                    item.Y = last.Y + last.Height + heightPer;
                }
            }
        }
    }

    public class ResultsJudgementGraphBar : Sprite
    {
        /// <summary>
        /// </summary>
        private Judgement Judgement { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus TextCount { get; set; }

        /// <summary>
        /// </summary>
        private Sprite FlagMarker { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Bar { get; set; }

        /// <summary>
        /// </summary>
        private Sprite JudgementSprite { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Percentage { get; set; }

        /// <summary>
        /// </summary>
        private Animation CountAnimation { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="judgement"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public ResultsJudgementGraphBar(Judgement judgement, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Judgement = judgement;
            Processor = processor;
            Size = size;
            Alpha = 0f;

            CreateTextCount();
            CreateFlagMarker();
            CreateBar();
            CreateJudgementSprite();
            CreatePercentage();

            AnimateBarPercentage();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (CountAnimation != null && !CountAnimation.Done)
                TextCount.Text = $"{(int) CountAnimation?.PerformInterpolation(gameTime):n0}";

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateTextCount()
        {
            TextCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBold), "", 32)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Tint = GetColor(Judgement)
            };

            UpdateTextCount();
        }

        /// <summary>
        /// </summary>
        private void UpdateTextCount() => TextCount.Text = $"{Processor.Value.CurrentJudgements[Judgement]:n0}";

        /// <summary>
        /// </summary>
        private void CreateFlagMarker() => FlagMarker = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(4, Height),
            Tint = GetColor(Judgement)
        };

        /// <summary>
        /// </summary>
        private void CreateBar()
        {
            Bar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(0, Height),
                Tint = GetColor(Judgement),
                Image = UserInterface.OptionsSidebarButtonBackground,
                X = FlagMarker.Width,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementSprite()
        {
            var height = Height;
            var tex = GetJudgementTexture();

            JudgementSprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = FlagMarker.X + FlagMarker.Width + 4,
                Image = tex,
                Size = new ScalableVector2((float) tex.Width / tex.Height * height, height)
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePercentage() => Percentage = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
            "", 21)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = JudgementSprite.X + JudgementSprite.Width + 10,
            Tint = GetColor(Judgement)
        };

        /// <summary>
        /// </summary>
        private void AnimateBarPercentage()
        {
            Bar.ClearAnimations();
            Bar.Width = 0;

            var percent = (float) Processor.Value.CurrentJudgements[Judgement] / Processor.Value.TotalJudgementCount;

            const int animTime = 1500;

            Bar.ChangeWidthTo((int) (Width * percent), Easing.OutQuint, animTime);

            Percentage.Text = Processor.Value.CurrentJudgements[Judgement] == 0 ? "(0.00%)" : $"({percent * 100:0.00}%)";

            CountAnimation = new Animation(AnimationProperty.X, Easing.OutQuint, 0,
                Processor.Value.CurrentJudgements[Judgement], animTime);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Color GetColor(Judgement j)
        {
            switch (j)
            {
                case Judgement.Marv:
                    return new Color(251,255,182);
                case Judgement.Perf:
                    return new Color(255,231,107);
                case Judgement.Great:
                    return new Color(86,254,110);
                case Judgement.Good:
                    return new Color(0,209,255);
                case Judgement.Okay:
                    return new Color(217,107,206);
                case Judgement.Miss:
                    return new Color(249,100,93);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private Texture2D GetJudgementTexture()
        {
            var dir = $"Quaver.Resources/Textures/UI/Results";

            switch (Judgement)
            {
                case Judgement.Marv:
                    return TextureManager.Load($"{dir}/judge-marv.png");
                case Judgement.Perf:
                    return TextureManager.Load($"{dir}/judge-perf.png");
                case Judgement.Great:
                    return TextureManager.Load($"{dir}/judge-great.png");
                case Judgement.Good:
                    return TextureManager.Load($"{dir}/judge-good.png");
                case Judgement.Okay:
                    return TextureManager.Load($"{dir}/judge-okay.png");
                case Judgement.Miss:
                    return TextureManager.Load($"{dir}/judge-miss.png");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}