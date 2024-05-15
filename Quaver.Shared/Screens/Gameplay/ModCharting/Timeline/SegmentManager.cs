using System.Collections.Generic;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events.Arguments;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

public class SegmentManager : IValueChangeManager
{
    private readonly Dictionary<int, Segment> _segments;
    private readonly List<ValueVertex<ISegmentPayload>> _vertices = new();
    private int _currentIndex = -1;
    private int _currentTime;
    private readonly Dictionary<int, Segment> _activeSegments = new();
    private int _nextId;
    private ModChartEvents _modChartEvents;

    public int GenerateNextId()
    {
        return _nextId++;
    }

    public SegmentManager(Dictionary<int, Segment> segments)
    {
        _segments = segments;
        GenerateVertices();
    }

    public void SetupEvents(ModChartEvents modChartEvents)
    {
        _modChartEvents = modChartEvents;
        modChartEvents[ModChartEventType.TimelineAddSegment].OnInvoke += args =>
        {
            var segment = ((ModChartEventAddSegmentInstance)args).Segment;
            Add(segment);
        };
        modChartEvents[ModChartEventType.TimelineRemoveSegment].OnInvoke += args =>
        {
            var segment = ((ModChartEventRemoveSegmentInstance)args).Segment;
            Remove(segment);
        };
        modChartEvents[ModChartEventType.TimelineUpdateSegment].OnInvoke += args =>
        {
            var segment = ((ModChartEventUpdateSegmentInstance)args).Segment;
            UpdateSegment(segment);
        };
    }

    public void GenerateVertices()
    {
        _vertices.Clear();
        foreach (var (id, segment) in _segments)
        {
            _vertices.Add(segment.StartVertex);
            _vertices.Add(segment.EndVertex);
        }

        _vertices.Sort(ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        _currentIndex = -1;
    }

    private void UpdateIndex()
    {
        if (_vertices.Count == 0) return;
        // We have not yet reached the first vertex
        if (_currentTime < _vertices[0].Time)
        {
            _currentIndex = -1;
            _activeSegments.Clear();
            return;
        }

        // Going forward
        while (_currentIndex < _vertices.Count - 1 && _currentTime > _vertices[_currentIndex + 1].Time)
        {
            _currentIndex++;
            AlternateVertex(_vertices[_currentIndex], 0, 1);
        }

        // Going backward
        while (_currentIndex >= 0 && _currentTime < _vertices[_currentIndex].Time)
        {
            AlternateVertex(_vertices[_currentIndex], 1, 0);
            _currentIndex--;
        }
    }

    /// <summary>
    ///     Alternates the vertex's presence in the active segment list
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="enterProgress"></param>
    /// <param name="leaveProgress"></param>
    private void AlternateVertex(ValueVertex<ISegmentPayload> vertex, float enterProgress = -1,
        float leaveProgress = -1, bool queueRemoveDynamic = true)
    {
        var segment = _segments[vertex.Id];
        var progress = enterProgress;
        if (!_activeSegments.TryAdd(vertex.Id, segment))
        {
            _activeSegments.Remove(vertex.Id);
            progress = leaveProgress;
            if (segment.IsDynamic && queueRemoveDynamic)
            {
                _modChartEvents.Enqueue(ModChartEventType.TimelineRemoveSegment, segment);
                segment.MarkedToRemove = true;
            }
        }

        vertex.Payload.Update(progress, segment);
    }

    public void Update(int curTime)
    {
        _currentTime = curTime;
        UpdateIndex();
        foreach (var (id, segment) in _activeSegments)
        {
            segment.Payload.Update(segment.Progress(_currentTime), segment);
        }
    }

    private bool InsertVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        if (insert < 0)
            insert = ~insert;
        else
            return false;

        _vertices.Insert(insert, vertex);
        // The timeline has already passed the time of vertex
        if (_currentTime > vertex.Time)
        {
            _currentIndex++;
            AlternateVertex(vertex, 0, 1);
        }

        return true;
    }

    private bool RemoveVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        if (index < 0) return false;
        // The timeline has already passed the time of vertex
        if (_currentTime > vertex.Time)
        {
            // The vertex will be removed anyway, dynamic segments don't need to be queued to remove
            AlternateVertex(vertex, 0, 1, false);
            _currentIndex--;
        }

        _vertices.RemoveAt(index);
        return true;
    }

    public bool Add(Segment segment)
    {
        if (segment.Id >= _nextId) return false; // No
        if (!_segments.TryAdd(segment.Id, segment)) return false;
        var success = InsertVertex(segment.StartVertex) && InsertVertex(segment.EndVertex);
        // Logger.Important($"Added {segment}: {success}, {_activeSegments.Count} active at {_currentTime}", LogType.Runtime);
        return success;
    }

    public bool UpdateSegment(Segment segment)
    {
        if (segment.Id >= _nextId) return false; // No
        if (_segments.TryGetValue(segment.Id, out var foundSegment))
        {
            foundSegment.MarkedToRemove = true;
            Remove(foundSegment);
        }

        return Add(segment);
    }

    public bool TryGetSegment(int id, out Segment segment) => _segments.TryGetValue(id, out segment);

    public bool ContainsSegment(int id) => _segments.ContainsKey(id);

    public bool Remove(Segment segment)
    {
        if (segment.Id >= _nextId) return false; // No
        if (!_segments.ContainsKey(segment.Id)) return false;
        if (!segment.MarkedToRemove) return false;
        var result = RemoveVertex(segment.StartVertex) && RemoveVertex(segment.EndVertex);
        if (!result) return false;
        segment.MarkedToRemove = false;
        _segments.Remove(segment.Id);
        _activeSegments.Remove(segment.Id);
        // Logger.Important($"Remove{segment}, {_activeSegments.Count} active at {_currentTime}", LogType.Runtime);
        return true;
    }
}