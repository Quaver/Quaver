using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
	public class SpatialHash<T>
	{
		private MultiValueDictionary<long, T> Dictionary { get; set; }

		private long CellSize { get; set; }

		public SpatialHash(long cellSize)
		{
			CellSize = cellSize;
			Dictionary = new MultiValueDictionary<long, T>();
		}

		public void Add(long position, T value)
		{
			var key = GetKey(position);
			Dictionary.Add(key, value);
		}

		public void Add(long startPos, long endPos, T value)
		{
            var start = GetKey(startPos);
            var end = GetKey(endPos);

            for (long i = start; i <= end; i++)
            {
                Dictionary.Add(i, value);
            }
		}

		public List<T> GetValues(long position)
		{
			var key = GetKey(position);
			return Dictionary.GetValues(key);
		}

		public long GetKey(long position) => position / CellSize;

	}

    public class MultiValueDictionary<TKey, TValue>
    {
        private Dictionary<TKey, List<TValue>> Dictionary { get; set; }

        public MultiValueDictionary()
        {
            Dictionary = new Dictionary<TKey, List<TValue>>();
        }

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

        public List<TValue> GetValues(TKey key)
        {
            List<TValue> values;
            if (Dictionary.TryGetValue(key, out values))
                return values;

            return new List<TValue>();
        }
    }

}
