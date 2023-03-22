using System.Collections.Generic;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects
{
	public class SpatialHash
	{
		private MultiValueDictionary<long, GameplayHitObjectInfo> Dictionary { get; set; }

		private long CellSize { get; set; }

		public SpatialHash(long cellSize)
		{
			CellSize = cellSize;
			Dictionary = new MultiValueDictionary<long, GameplayHitObjectInfo>();
		}

		public void AddValue(GameplayHitObjectInfo info)
		{
			if (!info.IsLongNote)
			{
				var key = GetKey(info.InitialTrackPosition);
				Dictionary.Add(key, info);
			}
			else
			{
				var start = GetKey(info.EarliestTrackPosition);
				var end = GetKey(info.LatestTrackPosition);

				for (long i = start; i <= end; i++)
				{
					Dictionary.Add(i, info);
				}
			}
		}

		public List<GameplayHitObjectInfo> GetValues(long position)
		{
			var key = GetKey(position);
			return Dictionary.GetValues(key);
		}

		public long GetKey(long position) => position / CellSize;

		private class MultiValueDictionary<TKey, TValue>
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
}
