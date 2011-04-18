using System;

namespace Raven.Light.Impl
{
	public class DocumentBeforeEnteringSession<T>
	{
		public Func<T> GetEntityButDontAttachToSession { get; set; }

		public Func<T> AddToSessionAndReturnWiredInstance { get; set; }
	}
}