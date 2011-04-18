using System;
using System.Threading;
using Raven.Munin;

namespace Raven.Light.Persistence
{
	public class TableStorage : Database
	{
		private ThreadLocal<StorageActions> current= new ThreadLocal<StorageActions>();

		public TableStorage(IPersistentSource persistentSource)
			: base(persistentSource)
		{
			Documents = Add(new Table(token => token.Value<string>("key"), "Documents")
			{
				{"ByTag", jToken => jToken.Value<string>("tag")}
			});
		}

		public Table Documents { get; set; }

		public void Batch(Action<StorageActions> action)
		{
			if (current.Value != null)
				action(current.Value);

			try
			{
				current.Value = new StorageActions(this);
				using(BeginTransaction())
				{
					action(current.Value);

					Commit();
				}
			}
			finally
			{
				current.Value = null;
			}
			
		}
	}
}