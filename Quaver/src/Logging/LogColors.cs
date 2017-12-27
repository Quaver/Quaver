using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Logging
{
    public struct LogColors
    {
        public static readonly Color GameError = Color.Red;
        public static readonly Color GameWarning = Color.Yellow;
        public static readonly Color GameSuccess = Color.Green;
        public static readonly Color GameInfo = Color.LightGreen;
        public static readonly Color GameImportant = Color.LightCyan;
    }
}
