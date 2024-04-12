namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Storyboard.Timeline;

public interface IValueChangeManager
{
    void Update(int curTime);

    // private void UpdateIndex(int curTime)
    // {
    //     if (_infos == null || _infos.Count == 0)
    //     {
    //         _currentIndex = 0;
    //         return;
    //     }
    //
    //     if (curTime < _infos[^1].Time)
    //         _currentIndex = _infos.Count - 1;
    //     if (curTime >= _infos[0].Time)
    //         _currentIndex = 0;
    //     if (_currentIndex < 0 || _currentIndex >= _infos.Count) return;
    //
    //     while (_currentIndex < _infos.Count - 1 && curTime >= _infos[_currentIndex + 1].Time)
    //     {
    //         _currentIndex++;
    //     }
    //
    //     while (_currentIndex >= 0 && curTime < _infos[_currentIndex].Time)
    //     {
    //         _currentIndex--;
    //     }
    // }
}