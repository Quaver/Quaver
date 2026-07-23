using System.Collections.Generic;

namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Marks screens that participate in the persistent UI lifecycle.
    /// </summary>
    internal interface IPersistentScreen
    {
        IReadOnlyCollection<string> PersistentElementKeys { get; }
    }
}
