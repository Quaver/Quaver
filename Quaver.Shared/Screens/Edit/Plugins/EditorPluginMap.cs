using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble.Audio.Tracks;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public class EditorPluginMap
    {
        [MoonSharpVisible(false)]
        public Qua Map;

        [MoonSharpVisible(false)]
        public IAudioTrack Track;

        /// <summary>
        ///     The game mode of the map
        /// </summary>
        public GameMode Mode { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     If the scroll velocities are in normalized format (BPM does not affect scroll velocity).
        /// </summary>
        public bool Normalized { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The slider velocities present in the map
        /// </summary>
        public List<SliderVelocityInfo> ScrollVelocities { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The HitObjects that are currently in the map
        /// </summary>
        public List<HitObjectInfo> HitObjects { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The timing points that are currently in the map
        /// </summary>
        public List<TimingPointInfo> TimingPoints { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The non-default editor layers that are currently in the map
        /// </summary>
        public List<EditorLayerInfo> EditorLayers { get; [MoonSharpVisible(false)] set; }
        
        /// <summary>
        ///     The bookmarks that are currently in the map
        /// </summary>
        public List<BookmarkInfo> Bookmarks { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     The default editor layer
        /// </summary>
        public EditorLayerInfo DefaultLayer { get; [MoonSharpVisible(false)] set; }

        /// <summary>
        ///     Total mp3 length
        /// </summary>
        public double TrackLength { get; [MoonSharpVisible(false)] set; }

        [MoonSharpVisible(false)]
        public void SetFrameState()
        {
            Mode = Map.Mode;
            TimingPoints = Map.TimingPoints;
            ScrollVelocities = Map.SliderVelocities; // Original name was SliderVelocities but that name doesn't really make sense
            HitObjects = Map.HitObjects;
            EditorLayers = Map.EditorLayers;
            Bookmarks = Map.Bookmarks;
            TrackLength = Track.Length;
            Normalized = Map.BPMDoesNotAffectScrollVelocity;
        }

        public override string ToString() => Map.ToString();

        /// <summary>
        ///    In Quaver, the key count is defined by the game mode.
        ///    This translates mode to key count.
        /// </summary>
        /// <returns></returns>
        public int GetKeyCount(bool includeScratch = true) => Map.GetKeyCount(includeScratch);

        /// <summary>
        ///     Finds the most common BPM in the current map
        /// </summary>
        /// <returns></returns>
        public float GetCommonBpm() => Map.GetCommonBpm();

        /// <summary>
        ///     Gets the timing point at a particular time in the current map.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public TimingPointInfo GetTimingPointAt(double time) => Map.GetTimingPointAt(time);

        /// <summary>
        ///     Gets the scroll velocity at a particular time in the current map
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public SliderVelocityInfo GetScrollVelocityAt(double time) => Map.GetScrollVelocityAt(time);

        /// <summary>
        ///     Gets the bookmark at a particular time in the current map
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public BookmarkInfo GetBookmarkAt(int time) => Map.GetBookmarkAt(time);

        /// <summary>
        ///    Finds the length of a timing point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public double GetTimingPointLength(TimingPointInfo point) => Map.GetTimingPointLength(point);

        /// <summary>
        ///     Gets the nearest snap time at a time to a given direction.
        /// </summary>
        /// <param name="forwards"></param>
        /// <param name="snap"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public double GetNearestSnapTimeFromTime(bool forwards, int snap, float time) => AudioEngine.GetNearestSnapTimeFromTime(Map, forwards ? Direction.Forward : Direction.Backward, snap, time);
    }
}
