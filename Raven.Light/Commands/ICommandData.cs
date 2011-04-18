//-----------------------------------------------------------------------
// <copyright file="ICommandData.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Raven.Light.Persistence;

namespace Raven.Light.Commands
{
	/// <summary>
	/// A single operation inside a batch
	/// </summary>
    public interface ICommandData
    {
		/// <summary>
		/// Gets the key.
		/// </summary>
		/// <value>The key.</value>
		string Key { get; }

		/// <summary>
		/// Gets the etag.
		/// </summary>
		/// <value>The etag.</value>
		Guid? Etag { get; }

		/// <summary>
		/// Execute te command
		/// </summary>
		/// <param name="storage"></param>
		/// <returns></returns>
		BatchResult Execute(StorageActions storage);
    }
}
