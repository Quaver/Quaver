using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
    /// <summary>
    ///     1-Dimensional implementation of a spatial hash map for HitObjectManagerKeys
    ///     Allows quickly finding objects in the same cell as some given position.
    /// </summary>
    /// <typeparam name="T"></typeparam>
	public class SpatialHashMap<T>
	{
        /// <summary>
        ///     Multi-value dictionary used for storing multiple objects in the same cell.
        /// </summary>
		public MultiValueDictionary<long, T> Dictionary { get; private set; }

        /// <summary>
        ///     Determines the size of each cell.
        /// </summary>
		public long CellSize { get; private set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="cellSize">Determines the size of each region.</param>
		public SpatialHashMap(long cellSize)
		{
			CellSize = cellSize;
			Dictionary = new MultiValueDictionary<long, T>();
		}

        /// <summary>
        ///     Add an object using the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="value"></param>
		public void Add(long position, T value)
		{
			var key = GetKey(position);
			Dictionary.Add(key, value);
		}

        /// <summary>
        ///     Add an object using the given interval of position.
        ///     The object is added to all cells between start and end inclusive.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="value"></param>
		public void Add(long startPos, long endPos, T value)
		{
            var start = GetKey(startPos);
            var end = GetKey(endPos);

            for (long i = start; i <= end; i++)
            {
                Dictionary.Add(i, value);
            }
		}

        /// <summary>
        ///     Get all objects that are in the same cell as the given position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
		public List<T> GetValues(long position)
		{
			var key = GetKey(position);
			return Dictionary.GetValues(key);
		}

        /// <summary>
        ///     Convert a given position to its corresponding dictionary key.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
		private long GetKey(long position) => position / CellSize;
	}

    /// <summary>
    ///     A dictionary that supports multiple values per key.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MultiValueDictionary<TKey, TValue>
    {
        /// <summary>
        ///     Multiple values per key are supported through the use of a list as a value in the backing dictionary.
        /// </summary>
        public Dictionary<TKey, List<TValue>> Dictionary { get; private set; }

        /// <summary>
        ///     Constructor
        /// </summary>
        public MultiValueDictionary()
        {
            Dictionary = new Dictionary<TKey, List<TValue>>();
        }

        /// <summary>
        ///     Add a value to the dictionary with the associated key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            List<TValue> values;

            if (!Dictionary.TryGetValue(key, out values))
            {
                values = new List<TValue>();
                values.Add(value);
                Dictionary.Add(key, values);
                return;
            }

            values.Add(value);
        }

        /// <summary>
        ///     Get all values associated with a key.
        ///     An empty list is returned if no values are associated.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<TValue> GetValues(TKey key)
        {
            List<TValue> values;
            if (Dictionary.TryGetValue(key, out values))
                return values;

            return new List<TValue>();
        }
    }
}
