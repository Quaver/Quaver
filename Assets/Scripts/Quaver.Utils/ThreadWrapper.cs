using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Quaver.Qua;

namespace Quaver.Utils
{
    public class ThreadWrapper
    {
        /// <summary>
        /// Parses a .qua file in a new thread.
        /// </summary>
        /// <param name="filePath">The file path of the .qua file</param>
        /// <returns>A new instance of QuaFile</returns>
        public static QuaFile ParseQuaInNewThread(string filePath)
        {
            QuaFile parsedMap = new QuaFile();

            Thread newThread = new Thread(() => { parsedMap = QuaParser.Parse(filePath); });
            newThread.Start();
            newThread.Join();

            return parsedMap;
        }
    }
}