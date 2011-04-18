using System;
using System.Collections.Generic;
using Raven.Light.Persistence;
using System.Linq;

namespace Raven.Light.Impl
{
	/// <summary>
	/// This explicitly doesn't implement IQueryable or IEnumerable
	/// We don't want to actually load the items into the Unit of Work unless they have been actually
	/// been returned to the user as a result of the query.
	/// Because of that, we wrap the entire Linq API.
	/// </summary>
	public class RavenLightQueryable<T>
	{
		private readonly IEnumerable<DocumentBeforeEnteringSession<T>> documents;

		public RavenLightQueryable(IEnumerable<DocumentBeforeEnteringSession<T>> documents)
		{
			this.documents = documents;
		}

		public IEnumerable<IGrouping<TKey, T>> GroupBy<TKey>(Func<T, TKey> keySelector)
		{
			return documents.Select(x=>x.GetEntityButDontAttachToSession()).GroupBy(keySelector);
		}

		public RavenLightQueryable<T> OrderBy<TKey>(Func<T,TKey> keySelector)
		{
			return new RavenLightQueryable<T>(documents.OrderBy(doc => keySelector(doc.GetEntityButDontAttachToSession())));
		}

		public RavenLightQueryable<T> OrderByDescending<TKey>(Func<T, TKey> keySelector)
		{
			return new RavenLightQueryable<T>(documents.OrderByDescending(doc => keySelector(doc.GetEntityButDontAttachToSession())));
		}


		public RavenLightQueryable<T> Where(Func<T, bool> predicate)
		{
			return new RavenLightQueryable<T>(documents.Where(Filter(predicate)));
		}

		private static Func<DocumentBeforeEnteringSession<T>, bool> Filter(Func<T, bool> predicate)
		{
			return tuple => predicate(tuple.GetEntityButDontAttachToSession());
		}

		public List<T> ToList()
		{
			return documents.Select(x => x.AddToSessionAndReturnWiredInstance()).ToList();
		}

		public T[] ToArray()
		{
			return documents.Select(x => x.AddToSessionAndReturnWiredInstance()).ToArray();
		}

		public T First()
		{
			var tuple = documents.First();
			return tuple.AddToSessionAndReturnWiredInstance();
		}

		public T First(Func<T, bool> predicate)
		{
			var tuple = documents.First(Filter(predicate));
			return tuple.AddToSessionAndReturnWiredInstance();
		}

		public T Single()
		{
			var tuple = documents.Single();
			return tuple.AddToSessionAndReturnWiredInstance();
		}

		public T Single(Func<T, bool> predicate)
		{
			var tuple = documents.Single(Filter(predicate));
			return tuple.AddToSessionAndReturnWiredInstance();
		}
		public T FirstOrDefault()
		{
			var tuple = documents.FirstOrDefault();
			return tuple.AddToSessionAndReturnWiredInstance();
		}

		public T FirstOrDefault(Func<T, bool> predicate)
		{
			var tuple = documents.FirstOrDefault(Filter(predicate));
			return tuple.AddToSessionAndReturnWiredInstance();
		}

		public T SingleOrDefault()
		{
			var tuple = documents.SingleOrDefault();
			return tuple.AddToSessionAndReturnWiredInstance();
		}

		public T SingleOrDefault(Func<T, bool> predicate)
		{
			var tuple = documents.SingleOrDefault(Filter(predicate));
			return tuple.AddToSessionAndReturnWiredInstance();
		}
	}
}