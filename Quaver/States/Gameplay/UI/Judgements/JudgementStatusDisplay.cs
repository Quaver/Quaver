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
        private GameplayScreen Screen { get; }

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
            for (var i = 0; i < Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count; i++)
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
                    PosY = Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count * -JudgementDisplays[key].SizeY / 2f;
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
                JudgementDisplays[item.Key].JudgementCount = Screen.Ruleset.ScoreProcessor.CurrentJudgements[item.Key];
            
            // Perform the collapse animation when the break is finished.
            // and the song is close to starting.
            if (!Screen.OnBreak && Screen.Timing.CurrentTime >= -500)
                PerformCollapseAnimation(dt);
            else
                PerformCollapseAnimation(dt, true);
          
            base.Update(dt);
        }

        /// <summary>
        ///     Collapses the judgement displays.
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="isExpanding"></param>
        private void PerformCollapseAnimation(double dt, bool isExpanding = false)
        {
            for (var i = 0; i < JudgementDisplays.Count; i++)
            { 
                var judgement = (Judgement) i;
                
                // Start off the first collapse.
                if (i == 0)
                {    
                    ChangeSizeAndTextWhenCollapsing(JudgementDisplays[judgement], dt, isExpanding);
                    continue;
                }
                
                // Get the previous judgement display.
                var previous = JudgementDisplays[(Judgement) (i - 1)];

                // Don't preform the animation unless the previous one has gotten to a certain size.
                // or if we're already hit our target
                if ((previous.SizeX > 100 && !isExpanding) || (previous.SizeX < 60 && isExpanding))
                    continue;
                            
                // Start changing the size                          
                ChangeSizeAndTextWhenCollapsing(JudgementDisplays[judgement], dt, isExpanding);
            }
        }

        /// <summary>
        ///     Makes sure that the text is changed to a singular number when collapsing.
        /// </summary>
        /// <param name="display"></param>
        /// <param name="dt"></param>
        /// <param name="isExpanding"></param>
        private static void ChangeSizeAndTextWhenCollapsing(JudgementDisplay display, double dt, bool isExpanding)
        {
            // If we're expanding, we want to make it bigger and return the text.
            if (isExpanding)
            {
                if (display.SizeX >= 40)
                    display.SizeX = GraphicsHelper.Tween(120, display.SizeX, Math.Min(dt / 240, 1));

                if (display.SizeX <= 70)
                    display.SpriteText.Text = $"{display.Judgement.ToString()}: {display.JudgementCount.ToString()}";
            }
            // If we're collapsing and not expanding.
            else
            {
                if (display.SizeX >= 40)
                    display.SizeX = GraphicsHelper.Tween(40, display.SizeX, Math.Min(dt / 240, 1)); 
                       
                if (display.SizeX <= 70)
                    display.SpriteText.Text = display.JudgementCount.ToString();
            }
        }
    }
}