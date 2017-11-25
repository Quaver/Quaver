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
        ///     Deserializes a replay file 
        /// </summary>
        /// <returns></returns>
        internal static Replay ReadReplay(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException();

            // Create the replay object
            var replay = new Replay();

            // Read the replay data
            using (var fs = new FileStream(path, FileMode.Open))
            using (var br = new BinaryReader(fs))
            using (var outStream = new MemoryStream())
            {
                replay.QuaverVersion = br.ReadString();
                replay.BeatmapMd5 = br.ReadString();
                replay.ReplayMd5 = br.ReadString();
                replay.Name = br.ReadString();
                replay.Date = Convert.ToDateTime(br.ReadString());
                replay.Mods = (ModIdentifier)br.ReadInt32();
                replay.ScrollSpeed = br.ReadInt32();
                replay.Score = br.ReadInt32();
                replay.Accuracy = br.ReadSingle();
                replay.MaxCombo = br.ReadInt32();
                replay.MarvPressCount = br.ReadInt32();
                replay.MarvReleaseCount = br.ReadInt32();
                replay.PerfPressCount = br.ReadInt32();
                replay.PerfReleaseCount = br.ReadInt32();
                replay.GreatPressCount = br.ReadInt32();
                replay.GreatReleaseCount = br.ReadInt32();
                replay.GoodPressCount = br.ReadInt32();
                replay.GoodReleaseCount = br.ReadInt32();
                replay.OkayPressCount = br.ReadInt32();
                replay.OkayReleaseCount = br.ReadInt32();
                replay.Misses = br.ReadInt32();

                // Create the new list of replay frames.
                replay.ReplayFrames = new List<ReplayFrame>();

                // Decompress & Deserialize replay frames
                SevenZip.Helper.Decompress(br.BaseStream, outStream);

                // Split the frames up by commas
                var frames = Encoding.ASCII.GetString(outStream.ToArray()).Split(',');

                // Add all the replay frames to the object
                foreach (var frame in frames)
                {
                    try
                    {
                        // Split up the frame string by SongTime|KeyPressState
                        var frameSplit = frame.Split('|');

                        // Add the replay frame to the list!
                        replay.ReplayFrames.Add(new ReplayFrame
                        {
                            SongTime = int.Parse(frameSplit[0]),
                            KeyPressState = (KeyPressState)Enum.Parse(typeof(KeyPressState), frameSplit[1])
                        });
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                }
            }

            return replay;
        }

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

        /// <summary>
        ///     Generates a perfect, 0ms hit replay to be used during auto play
        /// </summary>
        /// <param name="hitObjects"></param>
        internal static Replay GeneratePerfectReplay(List<HitObject> hitObjects)
        {
            // Create the base replay object
            var replay = new Replay()
            {
                Name = "Quaver",
                Date = DateTime.UtcNow,
                Score = 100000,
                Accuracy = 100,
                ReplayFrames = new List<ReplayFrame>()
            };

            // Create a dictionary in groups of the hit object's start times.
            var startTimeGroup = hitObjects.GroupBy(x => x.StartTime).ToDictionary(x => x.Key, x => x.ToList());

            // Create a dictionary in groups but this time of the object's end times. (LNs)
            var endTimeGroup = hitObjects.GroupBy(x => x.EndTime).ToDictionary(x => x.Key, x => x.ToList());

            // For every object we want to create a perfect frame hit. 
            // To achieve this, we'll need to do two things:
            //  - For every regular note, we'll need to add two frames
            //      1. The initial key down frame
            //      2. An additional frame 1ms later with key up.
            //  - For every LN:
            //      1. A frame on the object start time with key down.
            //      2. A frame on the object end time with key up.
            foreach (var objectGroup in startTimeGroup)
            {
                var frame = new ReplayFrame
                {
                    SongTime = (int)(objectGroup.Key / GameBase.GameClock)
                };

                // Get the key press state of the current object group
                var kps = new KeyPressState();
                objectGroup.Value.ForEach(x => kps = kps | ConvertKeyLaneToKeyPressState(x.KeyLane));

                // Add the KeyPressState to the frame
                frame.KeyPressState = kps;

                // Add the key down replay frame to the list
                replay.ReplayFrames.Add(frame);

                // Now that we've added the first one, we need to add a second key up frame 1ms later
                var keyUpFrame = new ReplayFrame
                {
                    SongTime = frame.SongTime + 1,
                    KeyPressState = 0
                };

                replay.ReplayFrames.Add(keyUpFrame);
            }

            // LN Key Up Frames
            foreach (var objectGroup in endTimeGroup)
            {
                // Disregard end times of 0, as those are just normal hit objects.
                if (objectGroup.Key == 0)
                    continue;

                // Add a new key up frame.
                var frame = new ReplayFrame
                {
                    SongTime = (int) (objectGroup.Key / GameBase.GameClock),
                    KeyPressState = 0
                };

                replay.ReplayFrames.Add(frame);
            }

            // Order the frames by their start time
            replay.ReplayFrames = replay.ReplayFrames.OrderBy(x => x.SongTime).ToList();

            // Last step ladies and gentlemen, and that's to remove the frames where
            // another note is pressed while it's on the release frame. 
            // so... just say peace out to the extra unneeded frame.
            for (var i = 1; i < replay.ReplayFrames.Count; i++)
                if (replay.ReplayFrames[i].SongTime == replay.ReplayFrames[i - 1].SongTime && replay.ReplayFrames[i].KeyPressState == 0)
                    replay.ReplayFrames.RemoveAt(i);

        
            // Write the automatically generated file to a log file.
            replay.WriteToLogFile(Configuration.DataDirectory + "/" + "autoplay_replay.txt");

            return replay;
        }

        /// <summary>
        ///     Converts a Qua key lane to a KeyPressState
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        private static KeyPressState ConvertKeyLaneToKeyPressState(int lane)
        {
            switch (lane)
            {
                case 1:
                    return KeyPressState.K1;
                case 2:
                    return KeyPressState.K2;
                case 3:
                    return KeyPressState.K3;
                case 4:
                    return KeyPressState.K4;
                case 5:
                    return KeyPressState.K5;
                case 6:
                    return KeyPressState.K6;
                case 7:
                    return KeyPressState.K7;
                default:
                    return new KeyPressState();
            }
        }

        /// <summary>
        ///     Converts all replay frames to a string
        /// </summary>
        public static string ReplayFramesToString(List<ReplayFrame> replayFrames)
        {
            // The format for the replay frames are the following:
            //      SongTime|KeysPressed,
            var frameStr = "";

            replayFrames.ForEach(x => frameStr += $"{x.SongTime}|{(int)x.KeyPressState},");

            return frameStr;
        }
    }
}