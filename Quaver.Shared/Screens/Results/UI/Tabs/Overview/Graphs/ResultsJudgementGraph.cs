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

                Bars.Add(new ResultsJudgementGraphJudgementBar(judgement, Processor,
                    new ScalableVector2(Width, 50)));
            }

            Bars.Add(new ResultsJudgementGraphMineHitBar(Processor,
                new ScalableVector2(Width, 50)));

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

    public class ResultsJudgementGraphNumberBar : Sprite
    {
        private int Count { get; }

        private float Fraction { get; }

        private Texture2D Texture { get; }

        private Color Color { get; }

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
        private bool FinalizedWindowsAfterAnimation { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="count"></param>
        /// <param name="fraction"></param>
        /// <param name="texture"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        protected ResultsJudgementGraphNumberBar(int count, float fraction, Texture2D texture, ScalableVector2 size, Color color)
        {
            Count = count;
            Fraction = fraction;
            Texture = texture;
            Color = color;
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
            {
                var interpolatedCount = (int)(CountAnimation?.PerformInterpolation(gameTime) ?? 0);
                TextCount.Text = $"{interpolatedCount:n0}";
            }

            if (CountAnimation != null && CountAnimation.Done && !FinalizedWindowsAfterAnimation)
            {
                UpdateTextCount();
                FinalizedWindowsAfterAnimation = true;
            }

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
                Tint = Color,
            };

            UpdateTextCount();
        }

        /// <summary>
        /// </summary>
        private void UpdateTextCount() => TextCount.Text = $"{Count:n0}";

        /// <summary>
        /// </summary>
        private void CreateFlagMarker() => FlagMarker = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(4, Height),
            Tint = Color
        };

        /// <summary>
        /// </summary>
        private void CreateBar()
        {
            Bar = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(0, Height),
                Tint = Color,
                Image = UserInterface.OptionsSidebarButtonBackground,
                X = FlagMarker.Width,
            };
        }

        /// <summary>
        /// </summary>
        private void CreateJudgementSprite()
        {
            var height = Height;

            JudgementSprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = FlagMarker.X + FlagMarker.Width + 4,
                Image = Texture,
                Size = new ScalableVector2((float) Texture.Width / Texture.Height * height, height)
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
            Tint = Color,
        };

        /// <summary>
        /// </summary>
        private void AnimateBarPercentage()
        {
            Bar.ClearAnimations();
            Bar.Width = 0;

            const int animTime = 1000;

            Bar.ChangeWidthTo((int) (Width * Fraction), Easing.OutQuint, animTime);

            Percentage.Text = $"({Fraction * 100:0.00}%)";

            CountAnimation = new Animation(AnimationProperty.X, Easing.OutQuint, 0, Count, animTime);
        }
    }

    public class ResultsJudgementGraphJudgementBar : ResultsJudgementGraphNumberBar
    {
        private const string JudgementTextureDirectory = $"Quaver.Resources/Textures/UI/Results";

        /// <summary>
        /// </summary>
        /// <param name="judgement"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public ResultsJudgementGraphJudgementBar(Judgement judgement, Bindable<ScoreProcessor> processor,
            ScalableVector2 size)
            : base(GetCount(judgement, processor), GetFraction(judgement, processor), GetJudgementTexture(judgement), size, GetColor(judgement))
        {
            
        }

        private static int GetCount(Judgement judgement, Bindable<ScoreProcessor> processor) =>
            judgement switch
            {
                Judgement.Miss => processor.Value.CurrentJudgements[judgement] -
                                  processor.Value.CountMineHit,
                _ => processor.Value.CurrentJudgements[judgement],
            };

        private static float GetFraction(Judgement judgement, Bindable<ScoreProcessor> processor) =>
            (float)GetCount(judgement, processor) / processor.Value.TotalJudgementCount;

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Color GetColor(Judgement j)
        {
            switch (j)
            {
                case Judgement.Marv:
                    return new Color(255,255,255);
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
        private static Texture2D GetJudgementTexture(Judgement judgement)
        {
            switch (judgement)
            {
                case Judgement.Marv:
                    return TextureManager.Load($"{JudgementTextureDirectory}/judge-marv.png");
                case Judgement.Perf:
                    return TextureManager.Load($"{JudgementTextureDirectory}/judge-perf.png");
                case Judgement.Great:
                    return TextureManager.Load($"{JudgementTextureDirectory}/judge-great.png");
                case Judgement.Good:
                    return TextureManager.Load($"{JudgementTextureDirectory}/judge-good.png");
                case Judgement.Okay:
                    return TextureManager.Load($"{JudgementTextureDirectory}/judge-okay.png");
                case Judgement.Miss:
                    return TextureManager.Load($"{JudgementTextureDirectory}/judge-miss.png");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public class ResultsJudgementGraphMineHitBar : ResultsJudgementGraphNumberBar
    {
        public static Color Color => new(179, 179, 179);
        private const string JudgementTextureDirectory = $"Quaver.Resources/Textures/UI/Results";

        /// <summary>
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public ResultsJudgementGraphMineHitBar(Bindable<ScoreProcessor> processor,
            ScalableVector2 size)
            : base(processor.Value.CountMineHit,
                (float)processor.Value.CountMineHit / processor.Value.TotalJudgementCount,
                TextureManager.Load($"{JudgementTextureDirectory}/judge-mines.png"),
                size,
                Color)
        {
        }
    }
}