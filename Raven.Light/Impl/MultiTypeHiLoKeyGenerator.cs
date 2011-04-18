//-----------------------------------------------------------------------
// <copyright file="MultiTypeHiLoKeyGenerator.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Raven.Light.Conventions;

namespace Raven.Light.Impl
{
	/// <summary>
	/// Generate a hilo key for each given type
	/// </summary>
    public class MultiTypeHiLoKeyGenerator
    {
		private readonly EmbeddedDocumentStore documentStore;
        private readonly int capacity;
        private readonly object generatorLock = new object();
        private IDictionary<string, HiLoKeyGenerator> keyGeneratorsByTag = new Dictionary<string, HiLoKeyGenerator>();

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiTypeHiLoKeyGenerator"/> class.
		/// </summary>
		/// <param name="documentStore">The document store.</param>
		/// <param name="capacity">The capacity.</param>
        public MultiTypeHiLoKeyGenerator(EmbeddedDocumentStore documentStore, int capacity)
        {
            this.documentStore = documentStore;
            this.capacity = capacity;
        }

		/// <summary>
		/// Generates the document key.
		/// </summary>
		/// <param name="conventions">The conventions.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
        public string GenerateDocumentKey(DocumentConvention conventions, object entity)
        {
		    var typeTagName = conventions.GetTypeTagName(entity.GetType());
            if (string.IsNullOrEmpty(typeTagName)) //ignore empty tags
                return null; 
		    var tag = typeTagName.ToLowerInvariant();
            HiLoKeyGenerator value;
            if (keyGeneratorsByTag.TryGetValue(tag, out value))
				return value.GenerateDocumentKey(conventions, entity);

            lock(generatorLock)
            {
                if (keyGeneratorsByTag.TryGetValue(tag, out value))
                    return value.GenerateDocumentKey(conventions, entity);

                value = new HiLoKeyGenerator(documentStore, tag, capacity);
                // doing it this way for thread safety
                keyGeneratorsByTag = new Dictionary<string, HiLoKeyGenerator>(keyGeneratorsByTag)
                {
                    {tag, value}
                };
            }

			return value.GenerateDocumentKey(conventions, entity);
        }
    }
}
