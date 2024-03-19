using System.Collections.Generic;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class SegmentManager : IValueChangeManager
{
    private readonly Dictionary<int, Segment> _segments;
    private readonly List<ValueVertex<ISegmentPayload>> _vertices = new();
    private int _currentIndex;
    private readonly Dictionary<int, Segment> _activeSegments = new();
    private int _nextId;

    public int GenerateNextId()
    {
        return _nextId++;
    }

    public SegmentManager(Dictionary<int, Segment> segments)
    {
        _segments = segments;
        GenerateVertices();
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
        _currentIndex = 0;
    }

    private void UpdateIndex(int curTime)
    {
        if (_vertices.Count == 0) return;
        if (curTime < _vertices[0].Time)
        {
            _currentIndex = 0;
            _activeSegments.Clear();
            return;
        }
        if (_currentIndex > _vertices.Count) _currentIndex = _vertices.Count;

        while (_currentIndex < _vertices.Count && curTime > _vertices[_currentIndex].Time)
        {
            AlternateVertex(_vertices[_currentIndex]);

            _currentIndex++;
        }

        while (_currentIndex > 0 && curTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            AlternateVertex(_vertices[_currentIndex]);
        }
    }

    private void AlternateVertex(ValueVertex<ISegmentPayload> vertex)
    {
        if (!_vertices.Contains(vertex)) return;
        var segment = _segments[vertex.Id];
        if (!_activeSegments.TryAdd(vertex.Id, segment))
        {
            Logger.Debug($"Alternate: Remove active {vertex.Id}", LogType.Runtime);
            _activeSegments.Remove(vertex.Id);
            if (segment.IsDynamic)
            {
                Remove(segment);
                Logger.Debug($"Removed dynamic segment {segment.Id}", LogType.Runtime);
            }
        }
        else
        {
            Logger.Debug($"Alternate: Set active {vertex.Id}", LogType.Runtime);
        }
        vertex.Payload.Update(vertex.Time, -1);
    }

    public void Update(int curTime)
    {
        UpdateIndex(curTime);
        foreach (var (id, segment) in _activeSegments)
        {
            segment.Payload.Update(curTime, segment.Progress(curTime));
        }
    }

    private bool InsertVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        if (insert < 0)
            insert = ~insert;
        // // TODO not O(n)
        // else if (_vertices.Contains(vertex))
        //     return false;

        if (_currentIndex != _vertices.Count && insert <= _currentIndex)
        {
            AlternateVertex(vertex);
            _currentIndex++;
        }
        _vertices.Insert(insert, vertex);
        return true;
    }

    private bool RemoveVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        if (index < 0) return false;
        // var index = _vertices.FindIndex(v => v.Time == vertex.Time && v.Segment.Id == vertex.Segment.Id);
        // if (index == -1) return;
        if (index <= _currentIndex)
        {
            _currentIndex--;
        }
        else
        {
            AlternateVertex(vertex);
        }
        _vertices.RemoveAt(index);
        return true;
    }

    public bool Add(Segment segment)
    {
        if (!_segments.TryAdd(segment.Id, segment)) return false;
        return InsertVertex(segment.StartVertex) && InsertVertex(segment.EndVertex);
    }

    public bool UpdateSegment(Segment segment)
    {
        if (_segments.TryGetValue(segment.Id, out var foundSegment))
        {
            Remove(foundSegment);
        }

        return Add(segment);
    }

    public bool Remove(Segment segment)
    {
        if (!_segments.ContainsKey(segment.Id)) return false;
        var result = RemoveVertex(segment.StartVertex) && RemoveVertex(segment.EndVertex);
        _segments.Remove(segment.Id);
        _activeSegments.Remove(segment.Id);
        return result;
        // _vertices.RemoveAll(v => v.Segment.Id == segment.Id);
    }
}