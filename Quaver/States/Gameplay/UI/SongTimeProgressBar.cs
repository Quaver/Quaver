using System;
using System.Drawing;
using Quaver.Graphics.Base;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Text;
using Quaver.Graphics.UniversalDim;
using Quaver.Graphics.UserInterface;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Gameplay.UI
{
    internal class SongTimeProgressBar : ProgressBar
    {
        /// <summary>
        ///     The text that displays the current audio time
        /// </summary>
        private QuaverSpriteText CurrentTime { get; }

        /// <summary>
        ///     The text that displays the time left.
        /// </summary>
        private QuaverSpriteText TimeLeft { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - 
        /// </summary>
        /// <param name="maxValue"></param>
        /// <param name="initialValue"></param>
        /// <param name="size"></param>
        /// <param name="parent"></param>
        /// <param name="alignment"></param>
        internal SongTimeProgressBar(float maxValue, float initialValue, UDim2D size, Drawable parent, Alignment alignment) 
            : base(maxValue, initialValue, size, parent, alignment)
        {
            CurrentTime = new QuaverSpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(30, 30),
                Font = QuaverFonts.AssistantRegular16,
                TextColor = Color.FloralWhite,
                Text = "00:00",
                PosY = -35,
                PosX = 23
            };
            
            TimeLeft = new QuaverSpriteText()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new UDim2D(30, 30),
                Font = QuaverFonts.AssistantRegular16,
                TextColor = Color.FloralWhite,
                Text = "00:00",
                PosY = -35,
                PosX = -27
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            // Set the time of the current time
            if (CurrentValue > 0)
            {
                var currTime = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(CurrentValue);
                CurrentTime.Text = currTime.ToString("mm:ss");
            }
            
            // Set the time of the time left.
            if (MaxValue - CurrentValue >= 0)
            {
                var timeLeft = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(MaxValue - CurrentValue);
                TimeLeft.Text = $"-{timeLeft:mm:ss}";
            }
            
            base.Update(dt);
        }
    }
}