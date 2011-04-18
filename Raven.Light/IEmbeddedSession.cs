using System;
using Raven.Light.Impl;

namespace Raven.Light
{
	public interface IEmbeddedSession : IDisposable
	{
		IEmbeddedSessionAdvanced Advanced { get; }

		void Store(object instance);
		T Load<T>(string key);
		RavenLightQueryable<T> Query<T>() where T : class;
		void Delete(object instance);

		void SaveChanges();
	}
}