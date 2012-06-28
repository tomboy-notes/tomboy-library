// 
//  ReaderTests.cs
//  
//  Author:
//       Robert Nordan <rpvn@robpvn.net>
//  
//  Copyright (c) 2012 Robert Nordan
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 2.1 of the License, or
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
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class ReaderTests
	{
		//TODO: Make this independent of Diskstorage
		[Test()]
		[ExpectedException (typeof (TomboyException),
		 ExpectedMessage = "Note XML is corrupted!")] //TODO: This message subject to change!
		public void Read_NonsenseNoteFile_ThrowsException ()
		{
			string StartHereNotePath = "../../test_notes/corrupt_notes/nonsense.note";
			DiskStorage.Read (StartHereNotePath, "tomboy://nonsense");
		}

		[Test()]
		public void Read_ProperNoteFile_CorrectTitle ()
		{
			string StartHereNotePath = "../../test_notes/proper_notes/90d8eb70-989d-4b26-97bc-ba4b9442e51f.note";
			Note toCheck = DiskStorage.Read (StartHereNotePath, "tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51f");

			Assert.AreEqual ("Start Here", toCheck.Title);
		}
	}
}

