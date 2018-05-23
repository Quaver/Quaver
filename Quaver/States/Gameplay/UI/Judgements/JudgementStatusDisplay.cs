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

        /// <summary>
        ///     The size of each display item.
        /// </summary>
        internal static Vector2 DisplayItemSize { get; } = new Vector2(125, 45);

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        internal JudgementStatusDisplay(GameplayScreen screen)
        {
            Screen = screen;
            
            // Create the judgement displays.
            JudgementDisplays = new Dictionary<Judgement, JudgementDisplay>();
            for (var i = 0; i < Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count; i++)
            {
                var key = (Judgement) i;
                var color = GameBase.LoadedSkin.GetJudgeColor(key);      
                
                // Default it to an inactive color.
                JudgementDisplays[key] = new JudgementDisplay(this, key, new Color(color.R / 2, color.G / 2, color.B / 2), new Vector2(DisplayItemSize.Y, DisplayItemSize.Y))
                {
                    Alignment = Alignment.MidRight,
                    Parent = this,
                    Image = GameBase.QuaverUserInterface.JudgementOverlay
                };

                // Normalize the position of the first one so that all the rest will be completely in the middle.
                if (i == 0)
                {
                    PosY = Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count * -JudgementDisplays[key].SizeY / 2f;
                    continue;
                }

                JudgementDisplays[key].PosY = JudgementDisplays[(Judgement) (i - 1)].PosY + JudgementDisplays[key].SizeY + 5;
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
            if (!Screen.OnBreak && Screen.Timing.CurrentTime >= 0)
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
                if ((previous.SizeX > DisplayItemSize.X - DisplayItemSize.X / JudgementDisplays.Count && !isExpanding) 
                    || (previous.SizeX < DisplayItemSize.X / 2f && isExpanding))
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
        private void ChangeSizeAndTextWhenCollapsing(JudgementDisplay display, double dt, bool isExpanding)
        {
            // If we're expanding, we want to make it bigger and return the text.
            if (isExpanding)
            {
                display.SizeX = GraphicsHelper.Tween(DisplayItemSize.X, display.SizeX, Math.Min(dt / 180, 1));   
                display.SizeY = GraphicsHelper.Tween(DisplayItemSize.Y, display.SizeY, Math.Min(dt / 180, 1));   

                if (display.SizeX >= DisplayItemSize.X / 2f + 5)
                    display.SpriteText.Text = $"{display.Judgement.ToString()}: {display.JudgementCount.ToString()}";
            }
            // If we're collapsing and not expanding, make it a square.
            else
            {
                display.SizeX = GraphicsHelper.Tween(DisplayItemSize.Y, display.SizeX, Math.Min(dt / 180, 1)); 
                display.SizeY = GraphicsHelper.Tween(DisplayItemSize.Y, display.SizeY, Math.Min(dt / 180, 1));
                display.PosX = GraphicsHelper.Tween(0, display.PosX, Math.Min(dt / 180, 1));
                       
                if (display.SizeX <= DisplayItemSize.X / 2f + 5)
                    display.SpriteText.Text = display.JudgementCount.ToString();
            }
        }
    }
}