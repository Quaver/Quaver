using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Logging;
using Quaver.Net.Handlers;

namespace Quaver.Online.Events
{
    internal static class Chicken
    {
        /// <summary>
        ///     Called when we receive chicken from the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnChicken(object sender, ChickenEventArgs e)
        {
            Logger.LogImportant(e.Message, LogType.Network);
        }
    }
}
