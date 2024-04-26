using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.StateMachine;

/// <summary>
///     A size-based DSU implementation allowing a near constant time complexity for queries
/// </summary>
/// <remarks>
///     <see cref="T"/> is required to implement the <see cref="IWithParent{T}"/> interface for the retrieval of its parent.
/// </remarks>
/// <typeparam name="T">the data that can be joined to form unions</typeparam>
public class DisjointSetUnion<T> where T : IWithParent<T>
{
    private readonly Dictionary<T, T> _parent = new();
    private readonly Dictionary<T, int> _size = new();

    public bool IsUnion(T a, T b) => GetRepresentative(a).Equals(GetRepresentative(b));

    public T GetRepresentative(T value)
    {
        _parent.TryAdd(value, value);
        if (value.Equals(_parent[value]))
            return value;
        _parent[value] = GetRepresentative(_parent[value]);
        return _parent[value];
    }

    /// <summary>
    ///     Joins two values together. This will make <c>IsUnion(a, b)</c> return true
    /// <example>
    ///     <code language="cs">
    /// <![CDATA[
    ///         var dsu = new DisjointSetUnion<int>();
    ///         dsu.Union(1, 2);
    ///         dsu.IsUnion(0, 1); // false
    ///         dsu.IsUnion(2, 1); // true
    ///         dsu.IsUnion(1, 2); // true
    /// ]]>
    ///     </code>
    /// </example>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
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