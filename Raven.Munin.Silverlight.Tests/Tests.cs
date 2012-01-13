using System.IO.IsolatedStorage;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Light.Impl;

namespace SilverlightTest1
{
	[TestClass]
	public class TheVeryBasics
	{
		[TestCleanup]
		public void ClearIsolatedStorage()
		{
			IsolatedStorageFile.GetUserStoreForApplication().Remove();
		}

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

		[TestMethod]
		public void CanQueryWithStoreRestart()
		{
			string ayendeId, rahineId;
			using (var store = new EmbeddedDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					var ayende = new User
					{
						Name = "ayende"
					};
					session.Store(ayende);
					ayendeId = session.Advanced.GetDocumentId(ayende);
					var rahien = new User
					{
						Name = "rahien"
					};
					session.Store(rahien);
					rahineId = session.Advanced.GetDocumentId(rahien);
					session.SaveChanges();
				}
			}

			using (var store = new EmbeddedDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					var users = from user in session.Query<User>()
								where user.Name == "ayende"
								select user;

					Assert.AreEqual(1, users.ToList().Count);
					Assert.IsFalse(session.Advanced.IsLoaded(rahineId));
					Assert.IsTrue(session.Advanced.IsLoaded(ayendeId));
				}
			}
		}

		[TestMethod]
		public void CanQuery()
		{
			string ayendeId, rahineId;
			using (var store = new EmbeddedDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					var ayende = new User
					{
						Name = "ayende"
					};
					session.Store(ayende);
					ayendeId = session.Advanced.GetDocumentId(ayende);
					var rahien = new User
					{
						Name = "rahien"
					};
					session.Store(rahien);
					rahineId = session.Advanced.GetDocumentId(rahien);
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					var users = from user in session.Query<User>()
					            where user.Name == "ayende"
					            select user;

					Assert.AreEqual(1, users.ToList().Count);
					Assert.IsFalse(session.Advanced.IsLoaded(rahineId));
					Assert.IsTrue(session.Advanced.IsLoaded(ayendeId));
				}
			}
		}


		[TestMethod]
		public void CanQueryWithJoin()
		{
			string ayendeId, rahineId;
			using (var store = new EmbeddedDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					var ayende = new User
					{
						Name = "ayende"
					};
					session.Store(ayende);
					ayendeId = session.Advanced.GetDocumentId(ayende);
					var rahien = new User
					{
						ParentId = ayendeId,
						Name = "rahien"
					};
					session.Store(rahien);
					rahineId = session.Advanced.GetDocumentId(rahien);
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					var users = from user in session.Query<User>()
					            join child in session.Query<User>() on user.Id equals child.ParentId
					            where user.Name == "ayende"
					            select new {ParentName = user.Name, ChildName = child.Name};

					var actual = users.First();
					Assert.AreEqual("ayende", actual.ParentName);
					Assert.AreEqual("rahien", actual.ChildName);
				}
			}
		}

		[TestMethod]
		public void CanStoreAnEntityAndThenLoadIt()
		{
			string documentId;
			using (var store = new EmbeddedDocumentStore())
			{
				using (var session = store.OpenSession())
				{
					var instance = new User
					{
						Name = "ayende"
					};
					session.Store(instance);
					documentId = session.Advanced.GetDocumentId(instance);
					session.SaveChanges();
				}

				using (var session = store.OpenSession())
				{
					var user = session.Load<User>(documentId);
					Assert.AreEqual("ayende", user.Name);
				}
			}
		}
	}
}