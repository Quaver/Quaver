using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class TriggerManager : IValueChangeManager
{
    private readonly Dictionary<int, ValueVertex<ITriggerPayload>> _vertexDictionary = new();
    private readonly List<ValueVertex<ITriggerPayload>> _vertices;
    private int _currentIndex;
    private int _currentId;

    public int GenerateNextId()
    {
        return _currentId++;
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
        if (_currentIndex > _vertices.Count) _currentIndex = _vertices.Count;

        while (_currentIndex < _vertices.Count && curTime > _vertices[_currentIndex].Time)
        {
            _vertices[_currentIndex].Payload.Trigger(curTime);
            if (_vertices[_currentIndex].IsDynamic)
            {
                _vertexDictionary.Remove(_vertices[_currentIndex].Id);
                _vertices.RemoveAt(_currentIndex);
            }
            else
            {
                _currentIndex++;
            }
        }

        while (_currentIndex > 0 && curTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            _vertices[_currentIndex].Payload.Undo(curTime);
        }
    }

    public void Update(int curTime)
    {
        UpdateIndex(curTime);
    }

    public bool AddVertex(ValueVertex<ITriggerPayload> vertex)
    {
        if (_vertexDictionary.ContainsKey(vertex.Id)) return false;
        var insert = _vertices.BinarySearch(vertex, ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
        if (insert < 0)
            insert = ~insert;

        if (_currentIndex != _vertices.Count && insert <= _currentIndex)
        {
            vertex.Payload.Trigger(vertex.Time);
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
        return _vertexDictionary.ContainsKey(id) && RemoveVertex(_vertexDictionary[id]);
    }

    public bool RemoveVertex(ValueVertex<ITriggerPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex, ValueVertex<ITriggerPayload>.TimeSegmentIdComparer);
        if (index < 0) return false;
        if (index <= _currentIndex)
        {
            _currentIndex--;
        }
        else
        {
            vertex.Payload.Undo(vertex.Time);
        }

        _vertices.RemoveAt(index);
        return _vertexDictionary.Remove(vertex.Id);
    }
}