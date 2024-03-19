using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
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
            AlternateVertex(_vertices[_currentIndex], 1);

            _currentIndex++;
        }

        while (_currentIndex > 0 && curTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            AlternateVertex(_vertices[_currentIndex], 0);
        }
    }

    private void AlternateVertex(ValueVertex<ISegmentPayload> vertex, float progress = -1)
    {
        if (!_vertices.Contains(vertex)) return;
        var segment = _segments[vertex.Id];
        if (!_activeSegments.TryAdd(vertex.Id, segment))
        {
            _activeSegments.Remove(vertex.Id);
            if (segment.IsDynamic)
            {
                Remove(segment);
            }
        }

        vertex.Payload.Update(vertex.Time, progress);
    }

    public void Update(int curTime)
    {
        try
        {
            UpdateIndex(curTime);
            foreach (var (id, segment) in _activeSegments)
            {
                segment.Payload.Update(curTime, segment.Progress(curTime));
            }
        }
        catch (ScriptRuntimeException e)
        {
            Logger.Error(e.DecoratedMessage, LogType.Runtime);
        }
        catch (Exception e)
        {
            Logger.Error(e, LogType.Runtime);
        }
    }

    private bool InsertVertex(ValueVertex<ISegmentPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ISegmentPayload>.TimeSegmentIdComparer);
        if (insert < 0)
            insert = ~insert;

        if (_currentIndex != _vertices.Count && insert <= _currentIndex)
        {
            AlternateVertex(vertex, 1);
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
            AlternateVertex(vertex, 0);
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
    }
}