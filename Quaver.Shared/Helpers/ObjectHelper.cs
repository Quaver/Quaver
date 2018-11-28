using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Quaver.Shared.Helpers
{
    public static class ObjectHelper
    {
        /// <summary>
        ///     Clones an entire object.
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
    }
}
