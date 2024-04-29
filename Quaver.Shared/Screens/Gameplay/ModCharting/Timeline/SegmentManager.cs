using System.Collections.Generic;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects;
using Quaver.Shared.Screens.Gameplay.ModCharting.Objects.Events;

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
        var timelineCategoryEvent = modChartEvents[ModChartEventType.Timeline];
        timelineCategoryEvent[ModChartEventType.TimelineAddSegment].OnInvoke += (type, args) =>
        {
            var segment = args[0] as Segment;
            Add(segment);
        };
        timelineCategoryEvent[ModChartEventType.TimelineRemoveSegment].OnInvoke += (type, args) =>
        {
            var segment = args[0] as Segment;
            Remove(segment);
        };
        timelineCategoryEvent[ModChartEventType.TimelineUpdateSegment].OnInvoke += (type, args) =>
        {
            var segment = args[0] as Segment;
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
    private void AlternateVertex(ValueVertex<ISegmentPayload> vertex, float enterProgress = -1, float leaveProgress = -1)
    {
        if (!_vertices.Contains(vertex)) return;
        var segment = _segments[vertex.Id];
        var progress = enterProgress;
        if (!_activeSegments.TryAdd(vertex.Id, segment))
        {
            _activeSegments.Remove(vertex.Id);
            progress = leaveProgress;
            if (segment.IsDynamic)
            {
                _modChartEvents.Enqueue(ModChartEventType.TimelineRemoveSegment, segment);
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

        // The timeline has already passed the time of vertex
        if ( _currentTime > vertex.Time)
        {
            AlternateVertex(vertex, 0, 1);
            _currentIndex++;
        }

        _vertices.Insert(insert, vertex);
        return true;
    }

    private bool RemoveVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        if (index < 0) return false;
        // The timeline has already passed the time of vertex
        if (_currentTime > vertex.Time)
        {
            AlternateVertex(vertex, 0, 1);
            _currentIndex--;
        }

        _vertices.RemoveAt(index);
        return true;
    }

    public bool Add(Segment segment)
    {
        if (segment.Id >= _nextId) return false; // No
        if (!_segments.TryAdd(segment.Id, segment)) return false;
        return InsertVertex(segment.StartVertex) && InsertVertex(segment.EndVertex);
    }

    public bool UpdateSegment(Segment segment)
    {
        if (segment.Id >= _nextId) return false; // No
        if (_segments.TryGetValue(segment.Id, out var foundSegment))
        {
            Remove(foundSegment);
        }

        return Add(segment);
    }

    public bool Remove(Segment segment)
    {
        if (segment.Id >= _nextId) return false; // No
        if (!_segments.ContainsKey(segment.Id)) return false;
        var result = RemoveVertex(segment.StartVertex) && RemoveVertex(segment.EndVertex);
        _segments.Remove(segment.Id);
        _activeSegments.Remove(segment.Id);
        return result;
    }
}