using System.Collections.Generic;
using Quaver.API.Maps.Structures;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys;

public class ValueChangeManager
{
    private readonly List<ValueNodeInfo> _infos;
    private int _currentIndex;

    public ValueChangeManager(List<ValueNodeInfo> infos)
    {
        _infos = infos;
        _infos.Sort((i1, i2) => i1.Time.CompareTo(i2));
        if (_infos.Count == 0 || _infos[0].Value != 0)
            _infos.Add(new ValueNodeInfo
            {
                Time = 0,
                Lane = -1,
                Value = 0
            });
    }

    public float GetValue(int curTime)
    {
        UpdateIndex(curTime);
        if (_infos == null || _infos.Count == 0 || _currentIndex < 0) return 0;
        if (_currentIndex >= _infos.Count - 1) return _infos[^1].Value;
        return (curTime - _infos[_currentIndex].Time)
               / (_infos[_currentIndex + 1].Time - _infos[_currentIndex].Time)
               * (_infos[_currentIndex + 1].Value - _infos[_currentIndex].Value)
               + _infos[_currentIndex].Value;
    }

    private void UpdateIndex(int curTime)
    {
        if (_infos == null || _infos.Count == 0)
        {
            _currentIndex = 0;
            return;
        }

        if (curTime < _infos[^1].Time)
            _currentIndex = _infos.Count - 1;
        if (curTime >= _infos[0].Time)
            _currentIndex = 0;
        if (_currentIndex < 0 || _currentIndex >= _infos.Count) return;

        while (_currentIndex < _infos.Count - 1 && curTime >= _infos[_currentIndex + 1].Time)
        {
            _currentIndex++;
        }

        while (_currentIndex >= 0 && curTime < _infos[_currentIndex].Time)
        {
            _currentIndex--;
        }
    }
}