using System;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class StorageTests
	{
		[Test()]
		public void DiskStorageReadTitle ()
		{
			string StartHereNotePath = "../../tests/test_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note StartHere =  DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (StartHere.Title, "Start Here");
		}
		
		[Test()]
		public void Test_GetNotes ()
		{
			IStorage storage = DiskStorage.Instance;
			storage.SetPath ("../../tests/test_notes");
			List<Note> notes = storage.GetNotes ();
			Assert.IsNotNull (notes);
			Assert.IsTrue (notes.Count > 0);			
		}
		
		[Test()]
		public void DiskStorageWriteNote ()
		{
			
			DiskStorage.Write ("../../tests/test_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51d.note", TesterNote.GetTesterNote ());
			
		}
	}
}

