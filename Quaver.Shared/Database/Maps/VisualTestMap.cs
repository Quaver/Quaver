using Quaver.API.Maps;
using Wobble;

namespace Quaver.Shared.Database.Maps
{
    public class VisualTestMap : Map
    {
        public override Qua LoadQua(bool checkValidity = false)
            => Qua.Parse(GameBase.Game.Resources.Get($"{Directory}/{Path}"), false);
    }
}