using System;
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class StorageTests
	{
		[Test()]
		public void TestCase ()
		{
			
			Console.WriteLine (DiskStorage.Read ("/Users/jjennings/Library/Application Support/Tomboy/d8903a05-40ba-4a77-a408-bbb50d76b837.note", "tomboy://d8903a05-40ba-4a77-a408-bbb50d76b837").Title);
		}
	}
}

