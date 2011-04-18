using System;
using Raven.Json.Linq;

namespace Raven.Light.Persistence
{
	public class JsonDocument
	{
		public string Key { get; set; }
		public string Tag { get; set; }
		public Guid Etag { get; set; }
		public DateTime LastModified { get; set; }
		public RavenJObject Metadata { get; set; }
		public RavenJObject Document { get; set; }
	}
}