// ReSharper disable CheckNamespace
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Raven.Json.Linq;
using Raven.Munin;

namespace System.Collections.Concurrent
// ReSharper restore CheckNamespace
{
	public class ConcurrentDictionary<TKey, TValue> 
	{
		private readonly Dictionary<TKey, TValue> dictionary;

		public ConcurrentDictionary(IComparerAndEquality<RavenJToken> comparer)
		{
			dictionary = new Dictionary<TKey, TValue>((IEqualityComparer<TKey>)comparer);
		}

		public ConcurrentDictionary()
		{
			dictionary = new Dictionary<TKey, TValue>();
		}

		public bool TryAdd(TKey key, TValue value)
		{
			lock (dictionary)
			{
				if (dictionary.ContainsKey(key))
					return false;
				dictionary.Add(key, value);
				return true;
			}
		}

		public int Sum(Func<KeyValuePair<TKey, TValue>, int> func)
		{
			lock(dictionary)
			{
				return dictionary.Sum(func);
			}
		}

		public KeyValuePair<TKey, TValue>[] Where(Func<KeyValuePair<TKey, TValue>, bool> func)
		{
			lock (dictionary)
			{
				return dictionary.Where(func).ToArray();
			}
		}

		public bool TryRemove(TKey key, out TValue value)
		{
			lock (dictionary)
			{
				if (dictionary.TryGetValue(key, out value) == false)
					return false;
				dictionary.Remove(key);
				return true;
			}
		}

		public TKey[] KeysAsArray()
		{
			lock (dictionary)
			{
				return dictionary.Keys.ToArray();
			}	
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			lock (dictionary)
			{
				return dictionary.TryGetValue(key, out value);
			}
		}

		public TValue GetOrAdd(TKey key, TValue value)
		{
			lock (dictionary)
			{
				TValue existing;
				if (dictionary.TryGetValue(key, out existing))
					return existing;
				dictionary.Add(key, value);
				return value;
			}
		}
	}
}