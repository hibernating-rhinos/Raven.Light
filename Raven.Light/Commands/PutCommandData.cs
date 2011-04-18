//-----------------------------------------------------------------------
// <copyright file="PutCommandData.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Raven.Json.Linq;
using Raven.Light.Conventions;
using Raven.Light.Persistence;

namespace Raven.Light.Commands
{
	/// <summary>
	/// A single batch operation for a document PUT
	/// </summary>
    public class PutCommandData : ICommandData
    {
		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
        public virtual string Key { get; set; }

		/// <summary>
		/// Gets or sets the etag.
		/// </summary>
		/// <value>The etag.</value>
        public virtual Guid? Etag { get; set; }

		public BatchResult Execute(StorageActions storage)
		{
			var guid = storage.Add(Key, Metadata.Value<string>(Constants.RavenEntityName) ?? "",Etag, Metadata, Document);
			return new BatchResult
			{
				Etag = guid,
				Key = Key,
			};
		}

		/// <summary>
		/// Gets or sets the document.
		/// </summary>
		/// <value>The document.</value>
        public virtual RavenJObject Document { get; set; }

		/// <summary>
		/// Gets or sets the document metadata
		/// </summary>
		public virtual RavenJObject Metadata { get; set; }
    }
}
