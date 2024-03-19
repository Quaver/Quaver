using System.Collections.Generic;

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

        _vertices.Sort(ValueVertex<ISegmentPayload>.TimeComparer);
        _currentIndex = 0;
    }

    private void UpdateIndex(float curTime)
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
        if (!_activeSegments.TryAdd(vertex.Segment.Id, vertex.Segment))
        {
            _activeSegments.Remove(vertex.Segment.Id);
            if (vertex.Segment.IsDynamic)
            {
                _vertices.RemoveAll(v => v.Segment.Id == vertex.Segment.Id);
                _segments.Remove(vertex.Segment.Id);
            }
        }
        vertex.Payload.Update(vertex.Time);
    }

    public void Update(float curTime)
    {
        UpdateIndex(curTime);
        foreach (var (id, segment) in _activeSegments)
        {
            segment.Payload.Update(curTime);
        }
    }

    private void InsertVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeComparer);
        if (insert < 0)
            insert = ~insert;
        // TODO not O(n)
        else if (_vertices.Contains(vertex))
            return;

        if (insert < _currentIndex)
        {
            AlternateVertex(vertex);
            _currentIndex++;
        }
        _vertices.Insert(insert, vertex);
        
    }

    private void RemoveVertex(ValueVertex<ISegmentPayload> vertex)
    {
        // var index = _vertices.BinarySearch(vertex);
        // if (index < 0) return;
        var index = _vertices.FindIndex(v => v.Time == vertex.Time && v.Segment.Id == vertex.Segment.Id);
        if (index == -1) return;
        if (index < _currentIndex)
        {
            _currentIndex--;
        }
        else
        {
            AlternateVertex(vertex);
        }
        _vertices.RemoveAt(index);
    }

    public void Add(Segment segment)
    {
        if (!_segments.TryAdd(segment.Id, segment)) return;
        InsertVertex(segment.StartVertex);
        InsertVertex(segment.EndVertex);
    }

    public void UpdateSegment(Segment segment)
    {
        if (_segments.TryGetValue(segment.Id, out var foundSegment))
        {
            Remove(foundSegment);
        }

        Add(segment);
    }

    public void Remove(Segment segment)
    {
        if (!_segments.ContainsKey(segment.Id)) return;
        _segments.Remove(segment.Id);
        RemoveVertex(segment.StartVertex);
        RemoveVertex(segment.EndVertex);
        // _vertices.RemoveAll(v => v.Segment.Id == segment.Id);
    }
}