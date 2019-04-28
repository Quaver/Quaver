using System;
using System.Collections.Generic;
using System.Text;

namespace Quaver.Shared.Localization.Languages
{
    public abstract class LocalizedStrings
    {
        /// <summary>
        ///     Localized string. <see cref="string.Format(string, object[])"/> is applied to the string for parameters
        /// </summary>
        public abstract Dictionary<LocalizedString, string> Templates { get; }
    }
}
