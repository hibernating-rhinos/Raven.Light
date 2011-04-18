using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
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
			using(var store = new EmbeddedDocumentStore())
			{
			}
		}
	}
}