using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

public class TriggerManager : IValueChangeManager
{
    private readonly Dictionary<int, ValueVertex<ITriggerPayload>> _vertexDictionary = new();
    private readonly List<ValueVertex<ITriggerPayload>> _vertices;
    private int _currentIndex;
    private int _nextId;

    public int GenerateNextId()
    {
        return _nextId++;
    }

    public TriggerManager(List<ValueVertex<ITriggerPayload>> vertices)
    {
        _vertices = vertices;
        _vertices.Sort(ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
    }

    private void UpdateIndex(int curTime)
    {
        if (_vertices.Count == 0) return;
        if (curTime < _vertices[0].Time)
        {
            _currentIndex = 0;
            return;
        }


        while (_currentIndex < _vertices.Count && curTime > _vertices[_currentIndex].Time)
        {
            var vertex = _vertices[_currentIndex];
            vertex.Payload.Trigger(vertex);
            if (vertex.IsDynamic)
            {
                _vertexDictionary.Remove(vertex.Id);
                _vertices.RemoveAt(_currentIndex);
            }
            else if (vertex == _vertices[_currentIndex]) // The vertex could update its own trigger
            {
                _currentIndex++;
            }
        }
        
        if (_currentIndex > _vertices.Count) _currentIndex = _vertices.Count;

        while (_currentIndex > 0 && curTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            _vertices[_currentIndex].Payload.Undo(_vertices[_currentIndex]);
        }
    }

    public void Update(int curTime)
    {
        UpdateIndex(curTime);
    }

    public bool UpdateVertex(ValueVertex<ITriggerPayload> vertex)
    {
        RemoveId(vertex.Id);
        return AddVertex(vertex);
    }

    public bool AddVertex(ValueVertex<ITriggerPayload> vertex)
    {
        if (vertex.Id >= _nextId) return false; // No
        if (_vertexDictionary.ContainsKey(vertex.Id)) return false;
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
        if (insert < 0)
            insert = ~insert;

        if (_currentIndex < _vertices.Count && insert <= _currentIndex)
        {
            vertex.Payload.Trigger(vertex);
            if (vertex.IsDynamic)
            {
                return true;
            }

            _currentIndex++;
        }

        _vertices.Insert(insert, vertex);
        return _vertexDictionary.TryAdd(vertex.Id, vertex);
    }

    public bool RemoveId(int id)
    {
        if (id >= _nextId) return false; // No
        return _vertexDictionary.ContainsKey(id) && RemoveVertex(_vertexDictionary[id]);
    }

    public bool RemoveVertex(ValueVertex<ITriggerPayload> vertex)
    {
        if (vertex.Id >= _nextId) return false; // No
        var index = _vertices.BinarySearch(vertex, ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
        if (index < 0) return false;
        if (_currentIndex > 0 && index <= _currentIndex)
        {
            _currentIndex--;
        }
        else
        {
            vertex.Payload.Undo(vertex);
        }

        _vertices.RemoveAt(index);
        return _vertexDictionary.Remove(vertex.Id);
    }
}