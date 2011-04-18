using Raven.Json.Linq;
using Raven.Light.Conventions;
using Raven.Light.Exceptions;
using Raven.Light.Impl;

namespace Raven.Light
{
	public interface IEmbeddedSessionAdvanced
	{
		/// <summary>
		/// Gets the conventions used by this session
		/// </summary>
		/// <value>The conventions.</value>
		/// <remarks>
		/// This instance is shared among all sessions, changes to the <see cref="DocumentConvention"/> should be done
		/// via the <see cref="EmbeddedDocumentStore"/> instance, not on a single session.
		/// </remarks>
		DocumentConvention Conventions { get; }

		/// <summary>
		/// Gets or sets a value indicating whether the session should use optimistic concurrency.
		/// When set to <c>true</c>, a check is made so that a change made behind the session back would fail
		/// and raise <see cref="ConcurrencyException"/>.
		/// </summary>
		/// <value></value>
		bool UseOptimisticConcurrency { get; set; }

		/// <summary>
		/// Gets a value indicating whether any of the entities tracked by the session has changes.
		/// </summary>
		/// <value></value>
		bool HasChanges { get; }

		/// <summary>
		/// Gets the metadata for the specified entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		RavenJObject GetMetadataFor<T>(T instance);

		/// <summary>
		/// Returns whatever a document with the specified id is loaded in the 
		/// current session
		/// </summary>
		bool IsLoaded(string id);

		/// <summary>
		/// Gets the document id.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <returns></returns>
		string GetDocumentId(object instance);

		/// <summary>
		/// Determines whether the specified entity has changed.
		/// </summary>
		/// <param name="entity">The entity.</param>
		/// <returns>
		/// 	<c>true</c> if the specified entity has changed; otherwise, <c>false</c>.
		/// </returns>
		bool HasChanged(object entity);

		/// <summary>
		/// Evicts the specified entity from the session.
		/// Remove the entity from the delete queue and stops tracking changes for this entity.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entity">The entity.</param>
		void Evict<T>(T entity);

		/// <summary>
		/// Clears this instance.
		/// Remove all entities from the delete queue and stops tracking changes for all entities.
		/// </summary>
		void Clear();
	}
}