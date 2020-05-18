using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using ManagedBass.Fx;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.Components
{
    [MoonSharpUserData]
    public class EditorBpmDetector
    {
        /// <summary
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        ///     BPM and their occurrences
        /// </summary>
        public Dictionary<int, int> Bpms { get; } = new Dictionary<int, int>();

        /// <summary>
        ///     The BPM with the highest value of confidence
        /// </summary>
        public int HighestConfidenceBpm { get; private set; }

        /// <summary>
        ///     The percentage of the BPM with the highest confidence
        /// </summary>
        public int HighestConfidenceBpmPercentage { get; private set; }

        /// <summary>
        ///     The amount of detected BPMs there were
        /// </summary>
        public int TotalBpmDetectionIntervals => Bpms.Values.ToList().Sum();

        /// <summary>
        ///     The suggested offset for the user to use
        /// </summary>
        public int SuggestedOffset { get; private set; }

        /// <summary>
        ///     If the detection has completed
        /// </summary>
        public bool Done { get; private set; }

        /// <summary>
        ///     An action to be performed once completed
        /// </summary>
        private Action<Dictionary<int, int>, int> OnComplete { get; }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="onComplete"></param>
        [MoonSharpVisible(false)]
        public EditorBpmDetector(IAudioTrack track, Action<Dictionary<int, int>, int> onComplete = null)
        {
            Track = track;
            OnComplete = onComplete;
            Start();
        }

        private void Start()
        {
            if (Track is AudioTrackVirtual)
                return;

            ThreadScheduler.Run(() =>
            {
                var track = (AudioTrack) Track;
                var stream = 0;

                try
                {
                    switch (track.Type)
                    {
                        case AudioTrackLoadType.FilePath:
                            stream = Bass.CreateStream(track.OriginalFilePath, 0, 0, BassFlags.Decode);
                            break;
                        case AudioTrackLoadType.ByteArray:
                            stream = Bass.CreateStream(track.OriginalByteArray, 0, track.OriginalByteArray.Length,
                                BassFlags.Decode);
                            break;
                        case AudioTrackLoadType.Uri:
                            stream = Bass.CreateStream(track.OriginalUri.ToString(), 0, 0, BassFlags.Decode);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    DetectBpm(stream);
                    DetectOffset(stream);

                    OnComplete?.Invoke(Bpms, SuggestedOffset);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "Failed to detect BPM and offset!");
                }
                finally
                {
                    Done = true;
                    Bass.StreamFree(stream);
                }
            });
        }

        /// <summary>
        ///     Detects the BPM of the song
        /// </summary>
        /// <param name="stream"></param>
        private void DetectBpm(int stream)
        {
            var length = Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetLength(stream));

            const int intervals = 10;
            var timePerInterval = length / intervals;

            // BPM Detection
            for (var i = 0; i < intervals; i++)
            {
                var bpm = BassFx.BPMDecodeGet(stream, 0, (i + 1) * timePerInterval, 0,
                    BassFlags.FxBpmBackground | BassFlags.FXBpmMult2,
                    (channel, percent, user) => {});

                var bpmRounded = (int) Math.Round(bpm, MidpointRounding.AwayFromZero);

                if (Bpms.ContainsKey(bpmRounded))
                    Bpms[bpmRounded]++;
                else
                    Bpms[bpmRounded] = 1;
            }

            var totalIntervals = Bpms.Values.ToList().Sum();

            Logger.Important($"Detected {totalIntervals} total BPM intervals for AudioTrack", LogType.Runtime);

            foreach (var item in Bpms)
            {
                Logger.Important($"BPM: {item.Key} | Occurrences: {item.Value} " +
                                 $"| Confidence: {item.Value / (float) totalIntervals * 100f}%", LogType.Runtime);
            }

            HighestConfidenceBpm = Bpms.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            HighestConfidenceBpmPercentage = (int) Math.Round(Bpms[HighestConfidenceBpm] / (float) totalIntervals * 100f, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        ///     Detects the offset of the song
        /// </summary>
        /// <param name="stream"></param>
        private void DetectOffset(int stream)
        {
            var length = Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetLength(stream));

            var offset = 0d;

            BassFx.BPMBeatDecodeGet(stream, 0, length, BassFlags.FxBpmBackground,
                (channel, position, user) =>
                {
                    if (offset == 0)
                        offset = position;
                });

            SuggestedOffset = (int) Math.Round(offset * 100, MidpointRounding.AwayFromZero);
            Logger.Important($"Suggested offset: {SuggestedOffset}", LogType.Runtime);
        }
    }
}