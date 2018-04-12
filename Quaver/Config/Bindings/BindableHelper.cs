using System;
using System.Collections.Generic;

namespace Quaver.Config.Bindings
{
    internal static class BindableHelper
    {
        /// <summary>
        ///     Reads a Binded bool.
        /// </summary>
        /// <returns></returns>
        internal static BindedValue<bool> ReadBool(BindedValueString s, bool defaultVal, string newVal, EventHandler<BindedValueEventArgs<bool>> action,
            bool addToStore, List<BindedValue<bool>> store)
        {
            var bindedBool = new BindedValue<bool>(s, action);

            // Attempt to parse the bool and default it if it can't.
            bool val;
            bindedBool.Value = bool.TryParse(newVal, out val) ? val : defaultVal;

            if (addToStore)
                store.Add(bindedBool);
            
            return bindedBool;
        }
    }
}