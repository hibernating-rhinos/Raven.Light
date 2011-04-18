using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Bson;
using Raven.Json.Linq;
using Raven.Light.Exceptions;
using Raven.Munin.Util;

namespace Raven.Light.Persistence
{
	public class StorageActions
	{
		public TableStorage Storage {get; set; }

		public StorageActions(TableStorage storage)
		{
			Storage = storage;
		}

		public void Delete(string key, Guid? etag)
		{
			if (EnsureEtagMatches(key, etag) == false)
				return;

			Storage.Documents.Remove(key);
		}

		private bool EnsureEtagMatches(string key, Guid? etag)
		{
			var readResult = Storage.Documents.Read(key);
			if (readResult == null)
				return false;

			if(etag != null)
			{
				var existingEtag = new Guid(readResult.Key.Value<byte[]>("etag"));
				if(existingEtag != etag.Value)
					throw new ConcurrencyException("Could not delete " + key + " because it is already modified");
			}
			return true;
		}

		public Guid Add(string key, string tag, Guid? etag, RavenJObject metadata, RavenJObject document)
		{
			EnsureEtagMatches(key, etag);

			var stream = new PooledMemoryStream();
			
			metadata.WriteTo(new BsonWriter(stream));
			document.WriteTo(new BsonWriter(stream));

			var newGuid = Guid.NewGuid();
			Storage.Documents.Put(new RavenJObject
			{
				{"key", key},
				{"tag", tag},
				{"etag", newGuid.ToByteArray()}
			}, stream);

			return newGuid;
		}

		public IEnumerable<JsonDocument> ScanByTag(string tag)
		{
			return Storage.Documents["ByTag"]
				.SkipTo(tag)
				.TakeWhile(x => string.Equals(x.Value<string>("tag"), tag, StringComparison.InvariantCultureIgnoreCase))
				.Select(token => Read(token.Value<string>("key")))
				.Where(read => read != null);
		}

		public JsonDocument Read(string key)
		{
			var readResult = Storage.Documents.Read(key);
			if (readResult == null)
				return null;

			using(var stream = readResult.Data())
			{
				return new JsonDocument
				{
					Key = readResult.Key.Value<string>("key"),
					Tag = readResult.Key.Value<string>("tag"),
					Metadata = RavenJObject.Load(new BsonReader(stream)),
					Document = RavenJObject.Load(new BsonReader(stream))
				};
			}
		}
	}
}