using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Helpers
{
    internal static class StreamHelper
    {
        /// <summary>
        ///     Turns a Stream object into a byte array
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        internal static byte[] ConvertStreamToByteArray(Stream input)
        {
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
