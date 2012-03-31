using System;
using System.Collections.Generic;
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
		
		[Test()]
		public void Test_GetNotes ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath ("/home/jjennings/.local/share/tomboy");
			List<Note> notes = storage.GetNotes ();
			Assert.IsNotNull (notes);
			Assert.IsTrue (notes.Count > 0);			
		}
	}
}

