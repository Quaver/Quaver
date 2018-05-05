using System.Collections.Generic;

namespace Quaver.States.Gameplay
{
    internal class HitObjectStore
    {
        internal List<HitObject> Pool { get; set; }
        internal List<HitObject> Dead { get; set; }
    }
}