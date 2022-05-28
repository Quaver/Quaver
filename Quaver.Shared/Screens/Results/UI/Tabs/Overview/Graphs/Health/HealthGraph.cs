using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.API.Maps.Processors.Scoring.Data;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Health
{
    public class HealthGraph : Sprite
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        ///     Health data points throughout the score
        /// </summary>
        private List<(int, float, bool)> HealthDataHistory { get; set; }

        /// <summary>
        ///     Downscaled container from parent container in order to fit the numbers
        /// </summary>
        private Sprite ContentContainer { get; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public HealthGraph(Map map, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Map = map;
            Processor = processor;
            Size = size;
            Alpha = 0;

            ContentContainer = new Sprite
            {
                Parent = this,
                Alpha = 0f,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(Width - 80, Height * 0.9f)
            };

            CreateHealthHistory();
            DrawDataPointsFromHistory();
            CreateGridlinesAndLabels();
        }

        private void CreateHealthHistory()
        {
            if (!(Processor.Value is ScoreProcessorKeys keysProcessor))
                return;
            var simulatedProcessor =
                new ScoreProcessorKeys(Map.Qua ?? Map.LoadQua(), keysProcessor.Mods, keysProcessor.Windows);

            HealthDataHistory = new List<(int, float, bool)>();

            var previousTime = int.MinValue;
            var playIsFailed = false;

            foreach (var stat in keysProcessor.Stats)
            {
                simulatedProcessor.CalculateScore(stat.Judgement, stat.KeyPressType == KeyPressType.Release);

                var hp = simulatedProcessor.Health;

                // Used to color graph when play is failed but NoFail is active
                if (!playIsFailed && simulatedProcessor.Health <= 0)
                    playIsFailed = true;

                // Prevent multiple data points on a single time
                if (HealthDataHistory.Count > 0 && stat.SongPosition == previousTime)
                    HealthDataHistory.Remove(HealthDataHistory.Last());

                HealthDataHistory.Add((stat.SongPosition, hp, playIsFailed));
                previousTime = stat.SongPosition;
            }
        }

        private void DrawDataPointsFromHistory()
        {
            var start = this.HealthDataHistory.First().Item1;
            var end = Map.SongLength;

            // Needs to be replaced in case max health is changed, since there is no constant for it right now
            var maxHealth = 100f;

            for (var i = 0; i < HealthDataHistory.Count; i++)
            {
                var (time, health, failed) = HealthDataHistory[i];
                var y = health / maxHealth;

                var songProgress = (float) (time - start) / (end - start);
                var nextTime = i == HealthDataHistory.Count - 1 ? time : HealthDataHistory[i + 1].Item1;
                var nextSongProgress = (float) (nextTime - start) / (end - start);
                var width = nextSongProgress - songProgress;

                var point = new Sprite
                {
                    Parent = ContentContainer,
                    Alignment = Alignment.TopLeft,
                    X = songProgress * ContentContainer.Width,
                    Y = (1f - y) * ContentContainer.Height,
                    Size = new ScalableVector2(width * ContentContainer.Width, y * ContentContainer.Height),
                    Visible = true,
                    Alpha = 1f,
                    Tint = GetColor(failed, health)
                };
            }
        }

        /// <summary>
        /// </summary>
        private void CreateGridlinesAndLabels()
        {
            var gridLineCount = 10;

            // <= because we also want to draw the final line
            for (var i = 0; i <= gridLineCount; i++)
            {
                var relativeY = (float) i / gridLineCount;
                var health = Math.Round(100f - i * 10, 2);
                var alpha = 0.5f;
                var textAlpha = 1.0f;
                var thickness = 3;

                // is sub grid line
                if (health % 25 != 0)
                {
                    alpha /= 3;
                    textAlpha /= 2;
                    thickness = 2;
                }

                var line = new Sprite
                {
                    Parent = ContentContainer,
                    Alpha = alpha,
                    Tint = ColorHelper.HexToColor("#808080"),
                    Alignment = Alignment.TopCenter,
                    Y = relativeY * ContentContainer.Height,
                    Size = new ScalableVector2(ContentContainer.Width, thickness),
                };

                var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{health}%", 20,
                    false)
                {
                    Parent = line,
                    Alignment = Alignment.MidLeft,
                    Tint = ColorHelper.HexToColor("#808080"),
                    Alpha = textAlpha
                };

                text.X -= text.Width + 10;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static Color GetColor(bool failed, float health)
        {
            var white = new Color(251, 255, 182);
            var yellow = new Color(255, 231, 107);
            var orange = new Color(255, 156, 107);
            var red = new Color(249, 100, 93);

            if (!failed)
            {
                if (health >= 60)
                    return white;
                else if (health >= 40)
                    return yellow;
                else if (health >= 1)
                    return orange;
                else
                    return red;
            }
            else
                return red;
        }
    }
}