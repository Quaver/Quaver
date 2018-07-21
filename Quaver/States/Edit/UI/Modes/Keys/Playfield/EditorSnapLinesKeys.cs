using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Parsers;
using Quaver.Bindables;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Logging;
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

        /// <summary>
        ///     The last captured audio time in the previous frame.
        /// </summary>
        private double LastAudioTime { get; set; }

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
                    Alignment = Alignment.TopLeft,
                    PosY = -1
                };
      
                LineBuffer.Add(line);
            }

            Screen.CurrentBeatSnap.OnValueChanged += OnSnapChanged;
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

            // User seeked backwards in the audio, so we need to pool backwards.
            if (GameBase.AudioEngine.Time < LastAudioTime)
            {
                // Get the amount of time that was skipped back
                var timeSeekedBack = GameBase.AudioEngine.Time - LastAudioTime;
                
                // Get the amount of beats that should be pushed back.
                var beats = (int)Math.Abs(timeSeekedBack / timePerSnap) + 1;

                for (var i = 0; i < beats; i++)
                {
                    var lastIndex = PoolSize + PoolIndex - 1;

                    // Take the line at the last index and put it at the first
                    var line = LineBuffer[lastIndex];

                    LineBuffer.RemoveAt(lastIndex);

                    // Decrease the pool index
                    PoolIndex--;

                    // Put the line at the new pool index.
                    LineBuffer[PoolIndex] = line;
                }
            }

            // Set the positions of each line.
            for (var i = PoolIndex; i < PoolSize + PoolIndex; i++)
            {
                var targetTime = tp.StartTime + timePerSnap * i;

                var line = LineBuffer[i];

                line.SizeY = i % Screen.CurrentBeatSnap.Value == 0 ? 5 : 1;
                var newPos = EditorScrollContainerKeys.GetPosFromOffset(Playfield.HitPositionY, Playfield.ScrollSpeed, targetTime, line.SizeY);

                // Set the new position if it is indeed on-screen.
                line.PosY = (newPos > 0) ? newPos : -1;
  
                // Pooling starts when a line has gone off-screen, this is when the audio is going forward,
                // so we need to pool forward.
                if (!(LineBuffer[PoolIndex].PosY > GameBase.WindowRectangle.Height))
                    continue;

                // Add a new line.
                LineBuffer.Add(line);

                // Make the line in the old position null.
                LineBuffer[PoolIndex] = null;

                // Increase the index of the object pooling.
                PoolIndex++;
            }

            LastAudioTime = GameBase.AudioEngine.Time;
            base.Update(dt);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            Screen.CurrentBeatSnap.OnValueChanged -= OnSnapChanged;
            
            base.Destroy();
        }

        /// <summary>
        ///     When the snap changes backwards, we want to remove any extra lines.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSnapChanged(object sender, BindedValueEventArgs<int> e)
        {
            if (e.Value > e.OldValue)
                return;
                        
            Console.WriteLine("Lines need to be pooled back.");
        }
    }   
}