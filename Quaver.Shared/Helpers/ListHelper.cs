using System.Collections.Generic;

namespace Quaver.Shared.Helpers
{
    public static class ListHelper
    {
        public static void Swap<T>(IList<T> list, int indexA, int indexB)
        {
            var tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}
