using System.Collections.Generic;
using Quaver.Shared.Screens.Gameplay.UI;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Proxy;

public class GameplayNumberDisplayProxy : SpriteProxy
{
    private readonly GameplayNumberDisplay _gameplayNumberDisplay;
    public GameplayNumberDisplayProxy(GameplayNumberDisplay drawable) : base(drawable)
    {
        _gameplayNumberDisplay = drawable;
    }

    public double CurrentValue => _gameplayNumberDisplay.CurrentValue;
    public double TargetValue => _gameplayNumberDisplay.TargetValue;
    public List<Sprite> Digits => _gameplayNumberDisplay.Digits;
    public string Value => _gameplayNumberDisplay.Value;

    public void UpdateValue(double num) => _gameplayNumberDisplay.UpdateValue(num);
}