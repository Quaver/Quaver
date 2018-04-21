using System.Collections.Generic;

namespace Quaver.Helpers
{
    internal static class ListHelper
    {
        internal static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}