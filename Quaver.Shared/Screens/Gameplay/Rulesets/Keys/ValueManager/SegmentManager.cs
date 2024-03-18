using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.ValueManager;

public class SegmentManager<TPayload> : IValueChangeManager where TPayload : ISegmentPayload
{
    private readonly HashSet<Segment<TPayload>> _segments;
    private readonly List<ValueVertex<TPayload>> _vertices = new();
    private int _currentIndex;
    private readonly HashSet<TPayload> _activeVertices = new();

    public SegmentManager(HashSet<Segment<TPayload>> segments)
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

        _vertices.Sort(ValueVertex<TPayload>.TimeComparer);
        _currentIndex = 0;
    }

    private void UpdateIndex(float curTime)
    {
        if (_vertices.Count == 0) return;
        if (curTime < _vertices[0].Time) return;
        if (_currentIndex > _vertices.Count) _currentIndex = _vertices.Count;

        while (_currentIndex < _vertices.Count && curTime > _vertices[_currentIndex].Time)
        {
            AlternateVertex(_vertices[_currentIndex].Payload);

            _currentIndex++;
        }

        while (_currentIndex > 0 && curTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            AlternateVertex(_vertices[_currentIndex].Payload);
        }
    }

    private void AlternateVertex(TPayload payload)
    {
        if (!_activeVertices.Add(payload))
        {
            _activeVertices.Remove(payload);
        }
    }

    public void Update(float curTime)
    {
        UpdateIndex(curTime);
        foreach (var payload in _activeVertices)
        {
            payload.Update(curTime);
        }
    }

    private void InsertVertex(ValueVertex<TPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<TPayload>.TimeComparer);
        if (insert < 0)
            insert = ~insert;
        else if (_vertices.Contains(vertex))
            return;

        if (insert < _currentIndex)
        {
            _currentIndex++;
            AlternateVertex(vertex.Payload);
        }
        _vertices.Insert(insert, vertex);
        
    }

    private void RemoveVertex(ValueVertex<TPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex);
        if (index < 0) return;
        if (index < _currentIndex)
        {
            _currentIndex--;
        }
        else
        {
            AlternateVertex(vertex.Payload);
        }
        _vertices.RemoveAt(index);
    }

    public void Add(Segment<TPayload> segment)
    {
        if (!_segments.Add(segment)) return;
        var (v1, v2) = segment.CreateVertexPair();
        InsertVertex(v1);
        InsertVertex(v2);
    }

    public void Remove(Segment<TPayload> segment)
    {
        if (!_segments.Remove(segment)) return;
        var (v1, v2) = segment.CreateVertexPair();
        RemoveVertex(v1);
        RemoveVertex(v2);
    }
}