using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Parsers;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Main;

namespace Quaver.States.Edit.UI.Modes.Keys.Playfield
{
    internal class EditorSnapLinesKeys : Container
    {
        /// <summary>
        ///     Reference to the editor screen itself.
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        ///     The 
        /// </summary>
        private EditorPlayfieldKeys Playfield { get; }

        /// <summary>
        ///     The buffer of snap lines.
        /// </summary>
        private List<Sprite> LineBuffer { get; }
 
        /// <summary>
        ///     The current index we're in on the object pool.
        /// </summary>
        private int PoolIndex { get; set; }

        /// <summary>
        ///     The amount of lines that'll be pooled.
        /// </summary>
        private int PoolSize { get; } = byte.MaxValue;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        internal EditorSnapLinesKeys(EditorScreen screen)
        {
            Screen = screen;
            Playfield = (EditorPlayfieldKeys) Screen.EditorGameMode.Playfield;
            Parent = Playfield.BackgroundContainer;
            
            LineBuffer = new List<Sprite>();
            
            for (var i = 0; i < PoolSize; i++)
            {
                var line = new Sprite
                {
                    Parent = this,
                    Size = new UDim2D(Playfield.Width, 1),
                    Alignment = Alignment.TopLeft
                };
                              
                LineBuffer.Add(line);
            }
        }

        /// <inheritdoc />
        /// <summary>
        ///     Moves the position of the lines.
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            var tp = Screen.Map.GetTimingPointAt(GameBase.AudioEngine.Time);
            var timePerSnap = tp.MillisecondsPerBeat / Screen.CurrentBeatSnap.Value;
            
            
            for (var i = PoolIndex; i < PoolSize + PoolIndex; i++)
            {
                var targetTime = tp.StartTime + timePerSnap * i;

                var line = LineBuffer[i];
                line.PosY = EditorScrollContainerKeys.GetPosFromOffset(Playfield.HitPositionY, Playfield.ScrollSpeed, targetTime, line.SizeY);

                // Pooling starts when a line has gone off-screen, this is when the audio is going forward,
                // so we need to pool forward.
                if (LineBuffer[PoolIndex].PosY > GameBase.WindowRectangle.Height)
                {
                    // Add a new line.
                    LineBuffer.Add(line);

                    // Make the line in the old position null.
                    LineBuffer[PoolIndex] = null;

                    // Increase the index of the object pooling.
                    PoolIndex++;
                }
            }
            
            base.Update(dt);
        }
    }
}