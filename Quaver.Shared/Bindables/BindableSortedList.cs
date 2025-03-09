using System;
using System.Collections.Generic;
using Quaver.API.Helpers;
using Quaver.API.Maps.Structures;
using Wobble.Bindables;

namespace Quaver.Shared.Bindables;

public class BindableSortedList<T> : BindableList<T> where T : IStartTime {
    public BindableSortedList(List<T> defaultVal, EventHandler<BindableValueChangedEventArgs<List<T>>> action = null) : base(defaultVal, action)
    {
    }

    public BindableSortedList(string name, List<T> defaultVal, EventHandler<BindableValueChangedEventArgs<List<T>>> action = null) : base(name, defaultVal, action)
    {
    }

    public override void Add(T obj)
    {
        Value.InsertSorted(obj);
        ItemAdded?.Invoke(this, new BindableListItemAddedEventArgs<T>(obj));
    }

    public override void AddRange(List<T> list)
    {
        Value.InsertSorted(list);
        MultipleItemsAdded?.Invoke(this, new BindableListMultipleItemsAddedEventArgs<T>(list));
    }
}