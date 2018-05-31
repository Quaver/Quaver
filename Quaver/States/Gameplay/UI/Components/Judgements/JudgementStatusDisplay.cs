using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components.Judgements
{
    /// <inheritdoc />
    /// <summary>
    ///     Displays all the current judgements + KPS
    /// </summary>
    internal class JudgementStatusDisplay : Container
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
        internal static Vector2 DisplayItemSize { get; } = new Vector2(50, 50);

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
                    Image = GameBase.QuaverUserInterface.JudgementOverlay,
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
            {
                JudgementDisplays[item.Key].JudgementCount = Screen.Ruleset.ScoreProcessor.CurrentJudgements[item.Key];
                UpdateTextAndSize(JudgementDisplays[item.Key], dt);
            }

            base.Update(dt);
        }

        /// <summary>
        ///     Makes sure that the text is changed to a singular number when collapsing.
        /// </summary>
        /// <param name="display"></param>
        /// <param name="dt"></param>
        private static void UpdateTextAndSize(JudgementDisplay display, double dt)
        {
            // Tween size and pos back to normal
            display.SizeX = GraphicsHelper.Tween(DisplayItemSize.Y, display.SizeX, Math.Min(dt / 180, 1)); 
            display.SizeY = GraphicsHelper.Tween(DisplayItemSize.Y, display.SizeY, Math.Min(dt / 180, 1));
            display.PosX = GraphicsHelper.Tween(0, display.PosX, Math.Min(dt / 180, 1));
                   
            display.SpriteText.Text = display.JudgementCount == 0 ? JudgementHelper.JudgementToShortName(display.Judgement) : display.JudgementCount.ToString();
        }
    }
}