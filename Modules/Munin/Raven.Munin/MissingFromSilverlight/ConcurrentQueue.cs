using System.Collections.Generic;

namespace System.Collections.Concurrent
{
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