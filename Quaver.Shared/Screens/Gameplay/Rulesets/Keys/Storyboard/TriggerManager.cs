using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard;

public class TriggerManager<TPayload> : IValueChangeManager where TPayload : ITriggerPayload
{
    private readonly List<ValueVertex<TPayload>> _vertices;
    private int _currentIndex;

    public TriggerManager(List<ValueVertex<TPayload>> vertices)
    {
        _vertices = vertices;
        _vertices.Sort(ValueVertex<TPayload>.TimeComparer);
    }

    private void UpdateIndex(float curTime)
    {
        if (_vertices.Count == 0) return;
        if (curTime < _vertices[0].Time) return;
        if (_currentIndex > _vertices.Count) _currentIndex = _vertices.Count;

        while (_currentIndex < _vertices.Count && curTime > _vertices[_currentIndex].Time)
        {
            _vertices[_currentIndex].Payload.Trigger();

            _currentIndex++;
        }

        while (_currentIndex > 0 && curTime < _vertices[_currentIndex - 1].Time)
        {
            _currentIndex--;
            _vertices[_currentIndex].Payload.Undo();
        }
    }
    
    public void Update(float curTime)
    {
        UpdateIndex(curTime);
    }

    public void AddVertex(ValueVertex<TPayload> vertex)
    {
        var insert = _vertices.BinarySearch(vertex, ValueVertex<TPayload>.TimeComparer);
        if (insert < 0)
            insert = ~insert;
        else if (_vertices.Contains(vertex))
            return;

        if (insert < _currentIndex)
        {
            _currentIndex++;
            vertex.Payload.Trigger();
        }
        _vertices.Insert(insert, vertex);
        
    }

    public void RemoveVertex(ValueVertex<TPayload> vertex)
    {
        var index = _vertices.BinarySearch(vertex);
        if (index < 0) return;
        if (index < _currentIndex)
        {
            _currentIndex--;
        }
        else
        {
            vertex.Payload.Undo();
        }
        _vertices.RemoveAt(index);
    }
}