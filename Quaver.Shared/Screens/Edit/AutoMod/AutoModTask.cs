using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Screens.Edit.AutoMod
{
    public abstract class AutoModTask
    {
        private Map Map { get; }

        public AutoModTask(Map map) => Map = map;

        public abstract void Run();
    }
}