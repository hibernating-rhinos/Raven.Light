//-----------------------------------------------------------------------
// <copyright file="HiLoKeyGenerator.cs" company="Hibernating Rhinos LTD">
//     Copyright (c) Hibernating Rhinos LTD. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Threading;
using Raven.Json.Linq;
using Raven.Light.Conventions;
using Raven.Light.Exceptions;
using Raven.Light.Json;
using Raven.Light.Persistence;

namespace Raven.Light.Impl
{
	/// <summary>
	/// Generate hilo numbers against a RavenDB document
	/// </summary>
	public class HiLoKeyGenerator
	{
		private const string RavenKeyGeneratorsHilo = "Raven/Hilo/";
		private readonly EmbeddedDocumentStore documentStore;
		private readonly string tag;
		private readonly long capacity;
		private readonly object generatorLock = new object();
		private long currentHi;
		private long currentLo;

		/// <summary>
		/// Initializes a new instance of the <see cref="HiLoKeyGenerator"/> class.
		/// </summary>
		/// <param name="documentStore">The document store.</param>
		/// <param name="tag">The tag.</param>
		/// <param name="capacity">The capacity.</param>
		public HiLoKeyGenerator(EmbeddedDocumentStore documentStore, string tag, long capacity)
		{
			currentHi = 0;
			this.documentStore = documentStore;
			this.tag = tag;
			this.capacity = capacity;
			currentLo = capacity + 1;
		}

		/// <summary>
		/// Generates the document key.
		/// </summary>
		/// <param name="convention">The convention.</param>
		/// <param name="entity">The entity.</param>
		/// <returns></returns>
		public string GenerateDocumentKey(DocumentConvention convention, object entity)
		{
			return string.Format("{0}{1}{2}",
								 tag,
								 convention.IdentityPartsSeparator,
								 NextId());
		}

		private long NextId()
		{
			long incrementedCurrentLow = Interlocked.Increment(ref currentLo);
			if (incrementedCurrentLow > capacity)
			{
				lock (generatorLock)
				{
					if (currentLo > capacity)
					{
						currentHi = GetNextHi();
						currentLo = 1;
						incrementedCurrentLow = 1;
					}
					else
					{
						incrementedCurrentLow = Interlocked.Increment(ref currentLo);
					}
				}
			}
			return (currentHi - 1) * capacity + (incrementedCurrentLow);
		}

		private long GetNextHi()
		{
			while (true)
			{
				try
				{
					JsonDocument document = null;
					documentStore.Storage.Batch(actions =>
					{
						document = actions.Read(RavenKeyGeneratorsHilo + tag);
					});
					if (document == null)
					{
						documentStore.Storage.Batch(actions => actions.Add(RavenKeyGeneratorsHilo + tag, "HiLo",
							// sending empty guid means - ensure the that the document does NOT exists
																		   Guid.Empty, new RavenJObject(),
																		   RavenJObject.FromObject(new HiLoKey { ServerHi = 2 })));
						return 1;
					}
					var hiLoKey = document.Document.JsonDeserialization<HiLoKey>();
					var newHi = hiLoKey.ServerHi;
					hiLoKey.ServerHi += 1;
					documentStore.Storage.Batch(actions => actions.Add(RavenKeyGeneratorsHilo + tag, "HiLo",
																	   document.Etag,
																	   new RavenJObject(),
																	   RavenJObject.FromObject(hiLoKey)));
					return newHi;
				}
				catch (ConcurrencyException)
				{
					// expected, we need to retry
				}
			}
		}

		#region Nested type: HiLoKey

		private class HiLoKey
		{
			public long ServerHi { get; set; }

		}

		#endregion
	}
}
