//  Author:
//       Robert Nordan
//  
//  Copyright (c) 2012
//  Robert Nordan
//  Jared Jennings
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

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

