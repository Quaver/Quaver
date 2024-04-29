using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Timeline;

public class TriggerManager : IValueChangeManager
{
    private readonly Dictionary<int, ValueVertex<ITriggerPayload>> _vertexDictionary = new();
    private readonly List<ValueVertex<ITriggerPayload>> _vertices;
    private int _currentIndex;
    private int _currentTime;
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

    private void UpdateIndex()
    {
        if (_vertices.Count == 0) return;
        // if (curTime < _vertices[0].Time)
        // {
        //     _currentIndex = 0;
        //     return;
        // }


        while (_currentIndex < _vertices.Count && _currentTime > _vertices[_currentIndex].Time)
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

        while (_currentIndex > 0 && _currentTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            _vertices[_currentIndex].Payload.Undo(_vertices[_currentIndex]);
        }
    }

    public void Update(int curTime)
    {
        _currentTime = curTime;
        UpdateIndex();
    }

    public bool UpdateVertex(ValueVertex<ITriggerPayload> vertex, bool trigger = true)
    {
        RemoveId(vertex.Id, trigger);
        return AddVertex(vertex, trigger);
    }

    public bool AddVertex(ValueVertex<ITriggerPayload> vertex, bool trigger = true)
    {
        if (vertex.Id >= _nextId) return false; // No
        if (_vertexDictionary.ContainsKey(vertex.Id)) return false;
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
        if (insert < 0)
            insert = ~insert;
        else
            return false;

        if (_currentTime > vertex.Time)
        {
            if (trigger) vertex.Payload.Trigger(vertex);
            if (vertex.IsDynamic)
            {
                return true;
            }

            _currentIndex++;
        }

        _vertices.Insert(insert, vertex);
        return _vertexDictionary.TryAdd(vertex.Id, vertex);
    }

    public bool RemoveId(int id, bool trigger = true)
    {
        if (id >= _nextId) return false; // No
        return _vertexDictionary.ContainsKey(id) && RemoveVertex(_vertexDictionary[id], trigger);
    }

    public bool RemoveVertex(ValueVertex<ITriggerPayload> vertex, bool trigger = true)
    {
        if (vertex.Id >= _nextId) return false; // No
        var index = _vertices.BinarySearch(vertex, ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
        if (index < 0) return false;
        if (_currentTime > vertex.Time)
        {
            if (_currentIndex > 0)
                _currentIndex--;
            if (trigger) vertex.Payload.Undo(vertex);
        }

        _vertices.RemoveAt(index);
        return _vertexDictionary.Remove(vertex.Id);
    }
}