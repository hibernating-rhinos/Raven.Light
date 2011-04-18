using System.Runtime.CompilerServices;

namespace System.Threading
{
	public class ThreadLocal<T>
	{
		private readonly Func<T> valueCreator;

		public class Holder
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
				if (_state == null || _state.TryGetValue(this, out value) == false)
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
				var holder = _state.GetOrCreateValue(this);
				holder.Val = value;
			}
		}
	}
}