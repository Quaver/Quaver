using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Judgements
{
    /// <inheritdoc />
    /// <summary>
    ///     Displays all the current judgements + KPS
    /// </summary>
    internal class JudgementStatusDisplay : QuaverContainer
    {
        /// <summary>
        ///     Reference to the ruleset.
        /// </summary>
        internal GameplayScreen Screen { get; }

        /// <summary>
        ///     The list of judgement displays.
        /// </summary>
        private Dictionary<Judgement, JudgementDisplay> JudgementDisplays { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        internal JudgementStatusDisplay(GameplayScreen screen)
        {
            Screen = screen;
           
            JudgementDisplays = new Dictionary<Judgement, JudgementDisplay>();
            for (var i = 0; i < Screen.GameModeComponent.ScoreProcessor.CurrentJudgements.Count; i++)
            {
                var key = (Judgement) i;
                var color = GameBase.LoadedSkin.GetJudgeColor(key);      
                
                // Default it to an inactive color.
                JudgementDisplays[key] = new JudgementDisplay(this, key, new Color(color.R / 2, color.G / 2, color.B / 2))
                {
                    Alignment = Alignment.MidRight,
                    Parent = this,
                    Size = new UDim2D(120, 40)
                };

                // Normalize the position of the first one so that all the rest will be completely in the middle.
                if (i == 0)
                {
                    PosY = Screen.GameModeComponent.ScoreProcessor.CurrentJudgements.Count * -JudgementDisplays[key].SizeY / 2f;
                    continue;
                }

                JudgementDisplays[key].PosY = JudgementDisplays[(Judgement) (i - 1)].PosY + JudgementDisplays[key].SizeY + 10;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Update the judgement counts of each one.
            foreach (var item in JudgementDisplays)
                JudgementDisplays[item.Key].JudgementCount = Screen.GameModeComponent.ScoreProcessor.CurrentJudgements[item.Key];
            
            // Perform the collapse animation for now if the audio is > -500
            // TODO: Make a "OnBreak" property in the screen.
            if (Screen.AudioTiming.CurrentTime >= -500)
                PerformCollapseAnimation(dt);
          
            base.Update(dt);
        }

        /// <summary>
        ///     Collapses the judgement displays.
        /// </summary>
        /// <param name="dt"></param>
        internal void PerformCollapseAnimation(double dt)
        {
            for (var i = 0; i < JudgementDisplays.Count; i++)
            { 
                var judgement = (Judgement) i;
              
                // Start off the first collapse.
                if (i == 0)
                {
                    JudgementDisplays[judgement].SizeX = GraphicsHelper.Tween(40, JudgementDisplays[judgement].SizeX, Math.Min(dt / 240, 1));            
                    ChangeTextWhenCollapsing(JudgementDisplays[judgement]);
                    continue;
                }
                
                // Get the previous judgement display.
                var previous = JudgementDisplays[(Judgement) (i - 1)];

                // Don't preform the animation unless the previous one has gotten to a certain size.
                if (previous.SizeX > 100)
                    continue;
                
                // Start changing the size
                JudgementDisplays[judgement].SizeX = GraphicsHelper.Tween(40, JudgementDisplays[judgement].SizeX, Math.Min(dt / 240, 1));                               
                ChangeTextWhenCollapsing(JudgementDisplays[judgement]);
            }
        }

        /// <summary>
        ///     Makes sure that the text is changed to a singular number when collapsing.
        /// </summary>
        /// <param name="display"></param>
        private static void ChangeTextWhenCollapsing(JudgementDisplay display)
        {
            if (display.SizeX <= 70)
                display.SpriteText.Text = display.JudgementCount.ToString();
        }
    }
}