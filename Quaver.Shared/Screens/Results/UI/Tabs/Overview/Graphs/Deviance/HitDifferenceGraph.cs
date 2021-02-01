using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Result.UI;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Deviance
{
    public class HitDifferenceGraph : Sprite
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Bindable<ScoreProcessor> Processor { get; }

        /// <summary>
        /// </summary>
        private ResultHitDifferenceGraph Graph { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        public HitDifferenceGraph(Map map, Bindable<ScoreProcessor> processor, ScalableVector2 size)
        {
            Map = map;
            Processor = processor;
            Size = size;

            Alpha = 0;
            CreateGraph();
            CreateAxesValues();
        }

        /// <summary>
        /// </summary>
        private void CreateGraph() => Graph = new ResultHitDifferenceGraph(new ScalableVector2(Width - 50, Height),
            Processor.Value, Map)
        {
            Parent = this,
            Alignment = Alignment.TopRight,
            SpriteBatchOptions = new SpriteBatchOptions
            {
                BlendState = BlendState.Opaque
            }
        };

        /// <summary>
        /// </summary>
        private void CreateAxesValues()
        {
            foreach (var line in Graph.LineData)
            {
                if (line.Judgement != Judgement.Miss && line.Judgement != Judgement.Great)
                    continue;

                var window = -line.Deviance / ModHelper.GetRateFromMods(Processor.Value.Mods);

                var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{(int) window}", 20,
                    false)
                {
                    Parent = line.Line,
                    Alignment = Alignment.MidLeft,
                    Tint = ColorHelper.HexToColor("#808080")
                };

                text.X -= text.Width + 10;
            }

            var zero = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"0", 20,
                false)
            {
                Parent = Graph.MiddleLine,
                Alignment = Alignment.MidLeft,
                Tint = ColorHelper.HexToColor("#808080")
            };

            zero.X -= zero.Width + 10;
        }
    }
}