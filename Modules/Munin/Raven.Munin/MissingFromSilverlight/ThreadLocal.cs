using System.Runtime.CompilerServices;

namespace System.Threading
{
	public class ThreadLocal<T>
	{
		private readonly Func<T> valueCreator;

		private class Holder
 		{
 			public T Val;
 		}

		[ThreadStatic]
		private static ConditionalWeakTable<object, Holder> _state;

		public ThreadLocal():this(() => default(T))
		{
			
		}

		public ThreadLocal(Func<T> valueCreator)
		{
			this.valueCreator = valueCreator;
		}

		public T Value
		{
			get
			{
				Holder value;
				if (_state == null || _state.TryGetValue(this, out value))
				{
					var val = valueCreator();
					Value = val;
					return val;
				}
				return value.Val;
			}
			set
			{
				if (_state == null)
					_state = new ConditionalWeakTable<object, Holder>();
				_state.Add(this, new Holder{Val = value});
			}
		}
	}
}