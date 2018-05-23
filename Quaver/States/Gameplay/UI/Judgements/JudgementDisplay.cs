using System;
using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Helpers;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Gameplay.UI.Judgements
{
    internal class JudgementDisplay : QuaverSprite
    {
        /// <summary>
        ///     The parent judgement display that controls the rest of them.
        /// </summary>
        private JudgementStatusDisplay ParentDisplay { get; }

        /// <summary>
        ///     The actual judgement this represents.
        /// </summary>
        internal Judgement Judgement { get; }

        /// <summary>
        ///     The current judgement count for this
        /// </summary>
        private int _judgementCount;
        internal int JudgementCount
        {
            get => _judgementCount;
            set
            {
                // Ignore if the judgement count is the same as the incoming value.
                if (_judgementCount == value)
                    return;

                _judgementCount = value;

                // Change the color to its active one.
                Tint = GameBase.LoadedSkin.GetJudgeColor(Judgement);
                
                // Make the size of the display look more pressed.
                SizeX = JudgementStatusDisplay.DisplayItemSize.Y - JudgementStatusDisplay.DisplayItemSize.Y / 4;
                SizeY = SizeX;
                PosX = -JudgementStatusDisplay.DisplayItemSize.Y / 16;
            }
        }

        /// <summary>
        ///     The sprite text for this given judgement.
        /// </summary>
        internal QuaverSpriteText SpriteText { get; }

        /// <summary>
        ///     The inactive color for this.
        /// </summary>
        private Color InactiveColor { get; }

        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="parentDisplay"></param>
        /// <param name="j"></param>
        /// <param name="color"></param>
        /// <param name="size"></param>
        internal JudgementDisplay(JudgementStatusDisplay parentDisplay, Judgement j, Color color, Vector2 size)
        {
            Judgement = j;
            ParentDisplay = parentDisplay;
            
            Size = new UDim2D(size.X, size.Y);
           
            SpriteText = new QuaverSpriteText()
            {
                Alignment = Alignment.MidCenter,
                Parent = this,
                Text = $"{JudgementCount}",
                Font = QuaverFonts.Medium12,
                TextColor = Color.Black,
                PosX = 0,
            };

            InactiveColor = color;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Make sure the color is always tweening down back to its inactive one.
            var r = GraphicsHelper.Tween(InactiveColor.R, Tint.R, Math.Min(dt / 360, 1));
            var g = GraphicsHelper.Tween(InactiveColor.G, Tint.G, Math.Min(dt / 360, 1));
            var b = GraphicsHelper.Tween(InactiveColor.B, Tint.B, Math.Min(dt / 360, 1));
            Tint = new Color((int)r, (int)g, (int)b);
                        
            base.Update(dt);
        }
    }
}