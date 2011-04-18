using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Light.Commands;
using Raven.Light.Listeners;
using Raven.Light.Persistence;

namespace Raven.Light.Impl
{
	public class EmbeddedDocumentSession : AbstractEmbeddedDocumentSession, IEmbeddedSession
	{
		public EmbeddedDocumentSession(EmbeddedDocumentStore documentStore, IDocumentStoreListener[] storeListeners, IDocumentDeleteListener[] deleteListeners) : base(documentStore, storeListeners, deleteListeners)
		{
		}

		protected override JsonDocument GetJsonDocument(string documentKey)
		{
			JsonDocument jsonDocument = null;
			documentStore.Storage.Batch(actions =>
			{
				jsonDocument = actions.Read(documentKey);
			});
			return jsonDocument;
		}

		public IEmbeddedSessionAdvanced Advanced
		{
			get { return this; }
		}

		public T Load<T>(string key)
		{
			var jsonDocument = GetJsonDocument(key);
			if (jsonDocument == null)
				return default(T);

			return TrackEntity<T>(jsonDocument);
		}

		public RavenLightQueryable<T> Query<T>() where T : class
		{
			IEnumerable<JsonDocument> jsonDocuments = null;
			documentStore.Storage.Batch(actions =>
			{
				jsonDocuments = actions.ScanByTag(Conventions.GetTypeTagName(typeof (T)));
			});
			return new RavenLightQueryable<T>(jsonDocuments.Select(document =>
			{
				T cache = null;
				return new DocumentBeforeEnteringSession<T>
				{
					GetEntityButDontAttachToSession = () => (cache ?? (cache = ConvertToEntity<T>(document.Key, document.Document, document.Metadata))),
					AddToSessionAndReturnWiredInstance = () => TrackEntity<T>(document)
				};
			}));
		}

		public void SaveChanges()
		{
			var prepareForSaveChanges = PrepareForSaveChanges();
			var batchResults = new List<BatchResult>();
			documentStore.Storage.Batch(
				actions => batchResults.AddRange(
					prepareForSaveChanges.Commands.Select(command => command.Execute(actions)))
					);
			UpdateBatchResults(batchResults, prepareForSaveChanges.Entities);
		}
	}
}