//-----------------------------------------------------------------------
// <copyright file="DeleteCommandData.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Raven.Light.Persistence;

namespace Raven.Light.Commands
{
	/// <summary>
	/// A single batch operation for a document DELETE
	/// </summary>
    public class DeleteCommandData : ICommandData
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
			storage.Delete(Key, Etag);
			return new BatchResult
			{
				Key = Key,
			};
		}
    }
}
