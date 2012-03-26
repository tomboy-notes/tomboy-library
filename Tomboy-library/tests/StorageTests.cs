using System;
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
			Note StartHere = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");
			Assert.AreEqual (StartHere.Title, "Start Here");
		}
	}
}

