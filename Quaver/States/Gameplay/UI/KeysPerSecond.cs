using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Graphics.UserInterface;

namespace Quaver.States.Gameplay.UI
{
    internal class KeysPerSecond : NumberDisplay
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


        internal KeysPerSecond(NumberDisplayType type, string startingValue, Vector2 imageScale) : base(type, startingValue, imageScale)
        {
        }
        
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            Time += dt;
            
            CalculateKeysPerSecond();
            Value = Kps.ToString();
            PosX = -TotalWidth - 10;
                      
            base.Update(dt);
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
        internal void AddClick() => Clicks.Add(Time);
    }
}