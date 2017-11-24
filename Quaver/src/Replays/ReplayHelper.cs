using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Modifiers;
using Quaver.QuaFile;

namespace Quaver.Replays
{
    /// <summary>
    ///     Dedicated class for helping with replays
    /// </summary>
    internal class ReplayHelper
    {
        /// <summary>
        ///     The interval at which to capture replays.
        //      In this case, we're going to be capturing replays at a frame rate of 60 fps.
        //      So we'll want to capture frames every 16.67ms or so.
        /// </summary>
        internal static float ReplayFpsInterval { get; set; } = 1000 / 60;

        /// <summary>
        ///     The previous task.
        /// </summary>
        internal static Task LastTask { get; set; }

        /// <summary>
        ///     This adds the correct frames to replays, called every frame during gameplay
        /// </summary>
        internal static void AddReplayFrames(List<ReplayFrame> ReplayFrames, Qua qua)
        {
            if (LastTask != null && !LastTask.IsCompleted)
            {
                LastTask.Wait();
            }
                
            if (LastTask == null || LastTask.IsCompleted)
                LastTask = Task.Run(() =>
                {
                    // Get the elapsed time
                    var elapsed = GameBase.GameTime.ElapsedMilliseconds;

                    // Create the base of the new frame
                    var frame = new ReplayFrame
                    {
                        GameTime = elapsed,
                        SongTime = (int)SongManager.Position,
                    };

                    // Don't capture frames if the song hasn't started yet
                    if (frame.SongTime == 0)
                        return;

                    // Add the first frame to the list of replay frames.
                    if (ReplayFrames.Count == 0)
                    {
                        // Check keyboard state
                        CheckKeyboardstate(qua, frame);

                        ReplayFrames.Add(frame);
                        return;
                    }

                    // Add the song time since the last frame
                    frame.TimeSinceLastFrame = frame.SongTime - ReplayFrames.Last().SongTime;
                    
                    // Grab the key press state of the last frame
                    var lastKeyPressState = ReplayFrames.Last().KeyPressState;

                    // Get the current keyboard state
                    CheckKeyboardstate(qua, frame);

                    // If the press state of the current frame isn't the same as the last
                    // then this frame must be very important.
                    if (lastKeyPressState != frame.KeyPressState)
                    {
                        ReplayFrames.Add(frame);
                        return;
                    }


                    // Start capturing frames at 60fps
                    if (ReplayFrames.Count != 0 && elapsed - ReplayFrames.Last().GameTime >= ReplayFpsInterval)
                        ReplayFrames.Add(frame);
                });
        }

        /// <summary>
        ///     Checks the keyboard state and adds it to the frame
        /// </summary>
        private static void CheckKeyboardstate(Qua qua, ReplayFrame frame)
        {
            switch (qua.KeyCount)
            {
                case 4:
                    // Key 1
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania1))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K1;

                    // Key 2
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania2))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K2;

                    // Key 3
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania3))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K3;

                    // Key 4
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania4))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K4;
                    break;
                case 7:
                    // Key 1
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k1))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K1;
                    // Key 2
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k2))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K2;
                    // Key 3
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k3))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K3;
                    // Key 4
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k4))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K4;
                    // Key 5
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k5))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K5;
                    // Key 6
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k6))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K6;
                    // Key 7
                    if (GameBase.KeyboardState.IsKeyDown(Configuration.KeyMania7k7))
                        frame.KeyPressState = frame.KeyPressState | KeyPressState.K7;
                    break;
                default:
                    break;
            }
        }
    }
}