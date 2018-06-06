using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Gameplay.UI.Components.Judgements.Counter
{
    /// <inheritdoc />
    /// <summary>
    ///     Displays all the current judgements + KPS
    /// </summary>
    internal class JudgementCounter : Container
    {
        /// <summary>
        ///     Reference to the ruleset.
        /// </summary>
        private GameplayScreen Screen { get; }

        /// <summary>
        ///     The list of judgement displays.
        /// </summary>
        private Dictionary<Judgement, JudgementCounterItem> JudgementDisplays { get; }

        /// <summary>
        ///     The size of each display item.
        /// </summary>
        internal static Vector2 DisplayItemSize { get; } = new Vector2(45, 45);

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        internal JudgementCounter(GameplayScreen screen)
        {
            Screen = screen;
            
            // Create the judgement displays.
            JudgementDisplays = new Dictionary<Judgement, JudgementCounterItem>();
            for (var i = 0; i < Screen.Ruleset.ScoreProcessor.CurrentJudgements.Count; i++)
            {
                var key = (Judgement) i;
                var color = GameBase.LoadedSkin.GetJudgeColor(key);      
                
                // Default it to an inactive color.
                JudgementDisplays[key] = new JudgementCounterItem(this, key, new Color(color.R / 2, color.G / 2, color.B / 2), new Vector2(DisplayItemSize.Y, DisplayItemSize.Y))
                {
                    Alignment = Alignment.MidRight,
                    Parent = this,
                    Image = GameBase.LoadedSkin.JudgementOverlay,
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
        /// <param name="counterItem"></param>
        /// <param name="dt"></param>
        private static void UpdateTextAndSize(JudgementCounterItem counterItem, double dt)
        {
            // Tween size and pos back to normal
            counterItem.SizeX = GraphicsHelper.Tween(DisplayItemSize.Y, counterItem.SizeX, Math.Min(dt / 180, 1)); 
            counterItem.SizeY = GraphicsHelper.Tween(DisplayItemSize.Y, counterItem.SizeY, Math.Min(dt / 180, 1));
            counterItem.PosX = GraphicsHelper.Tween(0, counterItem.PosX, Math.Min(dt / 180, 1));
                   
            counterItem.SpriteText.Text = (counterItem.JudgementCount == 0) ? JudgementHelper.JudgementToShortName(counterItem.Judgement) : counterItem.JudgementCount.ToString();
        }
    }
}