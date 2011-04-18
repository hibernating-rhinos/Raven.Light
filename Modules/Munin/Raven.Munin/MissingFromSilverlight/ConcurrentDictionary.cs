// ReSharper disable CheckNamespace
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Collections.Concurrent
// ReSharper restore CheckNamespace
{
	public class ConcurrentDictionary<TKey, TValue> 
	{
		private readonly Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

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
	}

	public class ConcurrentQueue<T>
	{
		private readonly Queue<T> queue = new Queue<T>();

		public int Count
		{
			get
			{
				lock (queue)
				{
					return queue.Count;
				}
			}
		}

		public bool TryDequeue(out T result)
		{
			lock (queue)
			{
				result = default(T);
				if (queue.Count == 0)
					return false;
				result = queue.Dequeue();
				return true;
			}
		}

		public void Enqueue(T value)
		{
			lock (queue)
			{
				queue.Enqueue(value);	
			}
		}
	}
}