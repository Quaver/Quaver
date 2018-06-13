using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using osu_database_reader;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Quaver.Main;
using Quaver.States.Gameplay.GameModes.Keys.Input;
using Quaver.States.Gameplay.HitObjects;

namespace Quaver.States.Gameplay.Replays
{
    public class Replay
    {
        /// <summary>
        ///     The game mode this replay is for.
        /// </summary>
        public GameMode Mode { get; }
        
        /// <summary>
        ///     All of the replay frames.
        /// </summary>
        public List<ReplayFrame> Frames { get; }

        /// <summary>
        /// 
        /// </summary>
        public string QuaverVersion => "Quaver Test Build";
        
        /// <summary>
        ///     
        /// </summary>
        public string PlayerName { get; }

        /// <summary>
        ///     The activated mods on this replay.
        /// </summary>
        public ModIdentifier Mods { get; }

        /// <summary>
        ///     The md5 hash of the map.
        /// </summary>
        public string MapMd5 { get; }

        public int Score { get; private set; }

        public float Accuracy { get; private set; }

        public int MaxCombo { get; private set; }

        public int CountMarv { get; private set; }

        public int CountPerf { get; private set; }

        public int CountGreat { get; private set; }

        public int CountGood { get; private set; }

        public int CountOkay { get; private set; }

        public int CountMiss { get; private set; }

        /// <summary>
        ///     The interval in milliseconds at which replays are captured.
        /// </summary>
        public static float CaptureInterval { get; } = 1000 / 60f;

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="name"></param>
        /// <param name="mods"></param>
        /// <param name="md5"></param>
        public Replay(GameMode mode, string name, ModIdentifier mods, string md5)
        {
            PlayerName = name;
            Mode = mode;
            Mods = mods;
            MapMd5 = md5;
            Frames = new List<ReplayFrame>();
        }

        /// <summary>
        ///    Writes the current replay to a binary file.
        /// </summary>
        internal void Write(string path)
        {       
            using (var replayDataStream = new MemoryStream(Encoding.ASCII.GetBytes(FramesToString())))
            using (var bw = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                var str = $"{QuaverVersion}--{MapMd5}//{PlayerName}=w{(int) Mods}xxx={Score}--.{Accuracy}--" +
                          $"{MaxCombo}@#{CountMarv}$!---{CountPerf}" +
                          $"---{CountGreat}@!!{CountGood}.@@@!@!{CountOkay}----{CountMiss}--{replayDataStream}";
                              
                bw.Write(QuaverVersion);
                bw.Write(MapMd5);
                bw.Write(CryptoHelper.StringToMd5(str));
                bw.Write(PlayerName);
                bw.Write(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                bw.Write((int)Mods);
                bw.Write(Score);
                bw.Write(Accuracy);
                bw.Write(MaxCombo);
                bw.Write(CountMarv);
                bw.Write(CountPerf);
                bw.Write(CountGreat);
                bw.Write(CountGood);
                bw.Write(CountOkay);
                bw.Write(CountMiss);
                bw.Write(StreamHelper.ConvertStreamToByteArray(LZMACoder.Compress(replayDataStream)));
            }
        }
        
        /// <summary>
        ///     Adds a frame to the replay.
        /// </summary>
        internal void AddFrame(float time, ReplayKeyPressState keys) => Frames.Add(new ReplayFrame(time, keys));

        /// <summary>
        ///    Populates the replay header properties from a score processor.
        /// </summary>
        internal void FromScoreProcessor(ScoreProcessor processor)
        {
            Score = processor.Score;
            Accuracy = processor.Accuracy;
            MaxCombo = processor.MaxCombo;
            CountMarv = processor.CurrentJudgements[Judgement.Marv];
            CountPerf = processor.CurrentJudgements[Judgement.Perf];
            CountGreat = processor.CurrentJudgements[Judgement.Great];
            CountGood = processor.CurrentJudgements[Judgement.Good];
            CountOkay = processor.CurrentJudgements[Judgement.Okay];
            CountMiss = processor.CurrentJudgements[Judgement.Miss];
        }
        
        /// <summary>
        ///     Generates a perfect replay given the game mode.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Replay GeneratePerfectReplay(Qua map, string md5)
        {        
            var replay = new Replay(map.Mode, "Autoplay", GameBase.CurrentMods, md5);

            switch (map.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    replay = GeneratePerfectReplayKeys(replay, map);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return replay;
        }

        /// <summary>
        ///     Generates a perfect replay for the keys game mode.
        /// </summary>
        /// <param name="replay"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        private static Replay GeneratePerfectReplayKeys(Replay replay, Qua map)
        {
            var nonCombined = new List<ReplayAutoplayFrame>();
            
            foreach (var hitObject in map.HitObjects)
            {
                // Add key press frame
                nonCombined.Add(new ReplayAutoplayFrame(hitObject, ReplayAutoplayFrameType.Press, hitObject.StartTime, KeyLaneToPressState(hitObject.Lane)));
                
                // If LN, add key up state at end time
                if (hitObject.IsLongNote)
                    nonCombined.Add(new ReplayAutoplayFrame(hitObject, ReplayAutoplayFrameType.Release, hitObject.EndTime - 1, KeyLaneToPressState(hitObject.Lane)));
                // If not ln, add key up frame 1ms after object.
                else
                    nonCombined.Add(new ReplayAutoplayFrame(hitObject, ReplayAutoplayFrameType.Release, hitObject.StartTime + 30, KeyLaneToPressState(hitObject.Lane)));
            }
            
            // Order objects by time
            nonCombined = nonCombined.OrderBy(x => x.Time).ToList();

            // Global replay state so we can loop through in track it.
            ReplayKeyPressState state = 0;
            
            // Add beginning frame w/ no press state. (-10000 just to be on the safe side.)
            replay.Frames.Add(new ReplayFrame(-10000, 0));
            
            var startTimeGroup = nonCombined.GroupBy(x => x.Time).ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in startTimeGroup)
            {
                foreach (var frame in item.Value)
                {
                    switch (frame.Type)
                    {
                        case ReplayAutoplayFrameType.Press:
                            state |= KeyLaneToPressState(frame.HitObject.Lane);
                            break;
                        case ReplayAutoplayFrameType.Release:
                            state -= KeyLaneToPressState(frame.HitObject.Lane);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                
                //Console.WriteLine($"Added frame at: {item.Key} with state: {state}");
                replay.Frames.Add(new ReplayFrame(item.Key, state));
            }
      
            return replay;
        }
        
        /// <summary>
        ///     Converts a lane to a key press state.
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        internal static ReplayKeyPressState KeyLaneToPressState(int lane)
        {
            switch (lane)
            {
                case 1:
                    return ReplayKeyPressState.K1;
                case 2:
                    return ReplayKeyPressState.K2;
                case 3:
                    return ReplayKeyPressState.K3;
                case 4:
                    return ReplayKeyPressState.K4;
                case 5:
                    return ReplayKeyPressState.K5;
                case 6:
                    return ReplayKeyPressState.K6;
                case 7:
                    return ReplayKeyPressState.K7;
                default:
                    throw new ArgumentException("Lane specified must be between 1 and 7");
            }
        }

        /// <summary>
        ///     Converts a key press state to a list of lanes that are active.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public static List<int> KeyPressStateToLanes(ReplayKeyPressState keys)
        {
            var lanes = new List<int>();

            if (keys.HasFlag(ReplayKeyPressState.K1))
                lanes.Add(0);            
            if (keys.HasFlag(ReplayKeyPressState.K2))
                lanes.Add(1);
            if (keys.HasFlag(ReplayKeyPressState.K3))
                lanes.Add(2);
            if (keys.HasFlag(ReplayKeyPressState.K4))
                lanes.Add(3);
            if (keys.HasFlag(ReplayKeyPressState.K5))
                lanes.Add(4);
            if (keys.HasFlag(ReplayKeyPressState.K6))
                lanes.Add(5);
            if (keys.HasFlag(ReplayKeyPressState.K7))
                lanes.Add(6);

             return lanes;
        }
        
        /// <summary>
        ///     Converts all replay frames to a string
        /// </summary>
        internal string FramesToString(bool debug = false)
        {
            // The format for the replay frames are the following:
            //     Time|KeysPressed,
            var frameStr = "";
            
            if (debug)
                Frames.ForEach(x => frameStr += $"{x.ToDebugString()}\r\n");
            else
                Frames.ForEach(x => frameStr += $"{x.ToString()},");

            return frameStr;
        }
    }
}