using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Wobble.Audio.Tracks;
using Wobble.Graphics;

#pragma warning disable CA1822

namespace Quaver.Shared.Screens.Edit.Plugins
{
    [MoonSharpUserData]
    public class EditorPluginMap
    {
        [MoonSharpVisible(false)]
        public Qua Map => EditorPluginUtils.EditScreen.WorkingMap;

        [MoonSharpVisible(false)]
        public IAudioTrack Track => EditorPluginUtils.EditScreen.Track;

        /// <summary>
        ///     The game mode of the map
        /// </summary>
        public GameMode Mode => Map.Mode;

        /// <summary>
        ///     If the scroll velocities are in normalized format (BPM does not affect scroll velocity).
        /// </summary>
        public bool Normalized => Map.BPMDoesNotAffectScrollVelocity;

        /// <summary>
        ///     The slider velocities present in the map
        /// </summary>
        public List<SliderVelocityInfo> ScrollVelocities => EditorPluginUtils.EditScreen.SelectedScrollGroup.ScrollVelocities;

        /// <summary>
        ///     The timing groups present in the map
        /// </summary>
        public IReadOnlyDictionary<string, TimingGroup> TimingGroups =>
            new ReadOnlyDictionary<string, TimingGroup>(Map.TimingGroups);

        /// <summary>
        ///     The default scroll group for hitobjects not bound to any groups
        /// </summary>
        public ScrollGroup DefaultScrollGroup => Map.DefaultScrollGroup;

        /// <summary>
        ///     The scroll group that applies its SVs to all other scroll groups
        /// </summary>
        public ScrollGroup GlobalScrollGroup => Map.GlobalScrollGroup;

        /// <summary>
        ///     The HitObjects that are currently in the map
        /// </summary>
        public List<HitObjectInfo> HitObjects => Map.HitObjects;

        /// <summary>
        ///     The timing points that are currently in the map
        /// </summary>
        public List<TimingPointInfo> TimingPoints => Map.TimingPoints;

        /// <summary>
        ///     The non-default editor layers that are currently in the map
        /// </summary>
        public List<EditorLayerInfo> EditorLayers => Map.EditorLayers;

        /// <summary>
        ///     The bookmarks that are currently in the map
        /// </summary>
        public List<BookmarkInfo> Bookmarks => Map.Bookmarks;

        /// <summary>
        ///     The default editor layer
        /// </summary>
        public EditorLayerInfo DefaultLayer => EditorPluginUtils.EditScreen.DefaultLayer;

        /// <summary>
        ///     Total mp3 length
        /// </summary>
        public double TrackLength => Track.Length;

        /// <inheritdoc cref="Qua.LegacyLNRendering"/>
        public bool LegacyLNRendering => Map.LegacyLNRendering;

        /// <inheritdoc cref="Qua.InitialScrollVelocity"/>
        public float InitialScrollVelocity => EditorPluginUtils.EditScreen.SelectedScrollGroup.InitialScrollVelocity;

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
        /// <param name="timingGroupId"></param>
        /// <returns></returns>
        public SliderVelocityInfo GetScrollVelocityAt(double time, string timingGroupId = null) =>
            Map.GetScrollVelocityAt(time, timingGroupId ?? EditorPluginUtils.EditScreen.SelectedScrollGroupId);

        /// <summary>
        ///     Gets the timing group with an id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TimingGroup GetTimingGroup(string id) => Map.TimingGroups.GetValueOrDefault(id);

        /// <summary>
        ///     Gets the list of IDs of all timing groups in the map
        /// </summary>
        /// <returns></returns>
        public List<string> GetTimingGroupIds() => Map.TimingGroups.Keys.ToList();

        /// <summary>
        ///     O(n)
        ///     Returns the list of hit objects that are in the specified group
        /// </summary>
        /// <param name="timingGroupId"></param>
        /// <returns></returns>
        public List<HitObjectInfo> GetTimingGroupObjects(string timingGroupId) =>
            Map.GetTimingGroupObjects(timingGroupId).ToList();

        /// <summary>
        ///     O(n log m) Given a list of timing group IDs, return a dictionary of (ID, [HitObject])
        /// </summary>
        /// <param name="timingGroupIds"></param>
        /// <returns></returns>
        public Dictionary<string, List<HitObjectInfo>> GetTimingGroupObjects(HashSet<string> timingGroupIds) =>
            Map.GetTimingGroupObjects(timingGroupIds);

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
