using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Light.Impl;

namespace SilverlightTest1
{
	[TestClass]
	public class TheVeryBasics
	{
		[TestMethod]
		public void TheTestFrameworkWorks()
		{
			// only in Silverlight would I have to do this sort of stuff
			Assert.IsTrue(true);
		}

		[TestMethod]
		public void CanStartTheStore()
		{
			using (new EmbeddedDocumentStore())
			{
			}
		}

		[TestMethod]
		public void CanOpenASession()
		{
			using (var store = new EmbeddedDocumentStore())
			{
				using(var session = store.OpenSession())
				{
					
				}
			}
		}

		[TestMethod]
		public void CanStoreAnEntity()
		{
			using (var store = new EmbeddedDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					session.Store(new User
					{
						Name = "ayende"
					});
					session.SaveChanges();
				}
			}
		}
	}

	public class User
	{
		public string Name { get; set; }
	}
}