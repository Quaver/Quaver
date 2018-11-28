using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Skinning;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class KeysPerSecond : NumberDisplay
    {
        /// <summary>
        ///     The list of all the click times.
        /// </summary>
        private List<double> Clicks { get; } = new List<double>();

        /// <summary>
        ///     The keys per second.
        /// </summary>
        private int Kps => Clicks.Count;

        /// <summary>
        ///     The amount of time 
        /// </summary>
        private double Time { get; set; }


        public KeysPerSecond(NumberDisplayType type, string startingValue, Vector2 imageScale)
            : base(type, startingValue, imageScale)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Time += gameTime.ElapsedGameTime.TotalMilliseconds;

            CalculateKeysPerSecond();
            Value = Kps.ToString();
            X = -TotalWidth + SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].KpsDisplayPosX;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Calculates the keys per second every frame.
        /// </summary>
        private void CalculateKeysPerSecond()
        {
            for (var i = 0; i < Clicks.Count; i++)
            {
                if (Clicks[i] <= Time - 1000)
                    Clicks.RemoveAt(i);
            }
        }

        /// <summary>
        ///     Adds a click to the KPS times.
        /// </summary>
        public void AddClick() => Clicks.Add(Time);
    }
}
