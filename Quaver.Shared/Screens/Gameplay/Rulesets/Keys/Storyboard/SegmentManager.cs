using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class SegmentManager : IValueChangeManager
{
    private readonly HashSet<Segment> _segments;
    private readonly List<ValueVertex<ISegmentPayload>> _vertices = new();
    private int _currentIndex;
    private readonly HashSet<ISegmentPayload> _activeVertices = new();

    public SegmentManager(HashSet<Segment> segments)
    {
        _segments = segments;
        GenerateVertices();
    }

    public void GenerateVertices()
    {
        _vertices.Clear();
        foreach (var segment in _segments)
        {
            var (v1, v2) = segment.CreateVertexPair();
            _vertices.Add(v1);
            _vertices.Add(v2);
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
            _activeVertices.Clear();
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
        if (!_activeVertices.Add(vertex.Payload))
        {
            _activeVertices.Remove(vertex.Payload);
        }
        vertex.Payload.Update(vertex.Time);
    }

    public void Update(float curTime)
    {
        UpdateIndex(curTime);
        foreach (var payload in _activeVertices)
        {
            payload.Update(curTime);
        }
    }

    private void InsertVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeComparer);
        if (insert < 0)
            insert = ~insert;
        else if (_vertices.Contains(vertex))
            return;

        if (insert < _currentIndex)
        {
            _currentIndex++;
            AlternateVertex(vertex);
        }
        _vertices.Insert(insert, vertex);
        
    }

    private void RemoveVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex);
        if (index < 0) return;
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
        if (!_segments.Add(segment)) return;
        var (v1, v2) = segment.CreateVertexPair();
        InsertVertex(v1);
        InsertVertex(v2);
    }

    public void Remove(Segment segment)
    {
        if (!_segments.Remove(segment)) return;
        var (v1, v2) = segment.CreateVertexPair();
        RemoveVertex(v1);
        RemoveVertex(v2);
    }
}