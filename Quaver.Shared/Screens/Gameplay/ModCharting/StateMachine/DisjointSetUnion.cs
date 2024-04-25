using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

public class DisjointSetUnion<T> where T : IWithParent<T>
{
    private readonly Dictionary<T, T> _parent = new();
    private readonly Dictionary<T, int> _size = new();

    public T GetRepresentative(T value)
    {
        _parent.TryAdd(value, value);
        if (value.Equals(_parent[value]))
            return value;
        _parent[value] = GetRepresentative(_parent[value]);
        return _parent[value];
    }

    public void Union(T a, T b)
    {
        var aParent = GetRepresentative(a);
        var bParent = GetRepresentative(b);
        if (aParent.Equals(bParent)) return;
        _size.TryAdd(a, 1);
        _size.TryAdd(b, 1);
        if (_size[aParent] >= _size[bParent])
        {
            _parent[bParent] = aParent;
            _size[aParent] += _size[bParent];
        }
        else
        {
            _parent[aParent] = bParent;
            _size[bParent] += _size[aParent];
        }
    }
}