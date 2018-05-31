using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components.Judgements
{
    /// <summary>
    ///    Bar Grapgh that displays the current judgement progress.
    /// </summary>
    internal class JudgementBarGraph : Sprite
    {
        /// <summary>
        ///     Reference to the current game's score processor.
        /// </summary>
        private ScoreProcessor Scoring { get; }

        /// <summary>
        ///     The active judgement bars in the graph.
        /// </summary>
        private SortedDictionary<Judgement, Sprite> Bars { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="size"></param>
        internal JudgementBarGraph(ScoreProcessor processor, Vector2 size)
        {
            Scoring = processor;
            Size = new UDim2D(size.X, size.Y);
            Tint = Color.Black;
            Alpha = 0.5f;
            
            CreateBars();            
        }

        /// <summary>
        ///     
        /// </summary>
        private void CreateBars()
        {
            Bars = new SortedDictionary<Judgement, Sprite>();
            
            for (var i = 0; i < Scoring.JudgementWindow.Count; i++)
            {
                var judgement = (Judgement) i;

                Bars[judgement] = new Sprite()
                {
                    Parent = this,
                    Alignment = Alignment.TopLeft,
                    Position = new UDim2D(0, i * 50),
                    Size = new UDim2D(SizeX, SizeY / Scoring.JudgementWindow.Count),
                    Tint = GameBase.LoadedSkin.GetJudgeColor(judgement)
                };
            }
        }
    }
}