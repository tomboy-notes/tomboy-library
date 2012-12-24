//
//  Author:
//       Timo Dörr <timo@latecrew.de>
//
//  Copyright (c) 2012 Timo Dörr
//
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using NUnit.Framework;

namespace Tomboy
{
	[TestFixture()]
	public class NoteTests
	{
		[Test()]
		public void NoteHasGuid ()
		{
			var note = new Note ();
			Assert.That (!string.IsNullOrEmpty (note.Guid));
		}
		[Test()]
		public void NoteHasUri ()
		{
			var note = new Note ();
			var guid = note.Guid;

			Assert.AreEqual ("note://tomboy/" + guid, note.Uri);
		}

		[Test()]
		public void NoteUnequalityOperator ()
		{
			var note1 = new Note ();
			var note2 = new Note ();

			Assert.AreNotEqual (note1, note2);
			Assert.That (note1 != note2);
		}

		[Test()]
		public void NoteUnequalityOperatorWithNull ()
		{
			Note note = new Note ();
			Assert.AreNotEqual (note, null);
			Assert.That (note != null);
		}

		[Test]
		public void NoteEqualityOperator ()
		{
			var note1 = new Note ();
			var note2 = note1;
			Assert.AreEqual (note1, note2);
			Assert.That (note1 == note2);
		}
		[Test]
		public void NoteEqualityOperatorWithNull ()
		{
			Note note = null;
			Assert.That (null == note);
			Assert.That (!(null != note));
		}

		[Test]
		public void NoteEqualityOperatorBothNull ()
		{
			Note note1 = null;
			Note note2 = null;

			Assert.AreEqual (note1, note2);
			Assert.That (note1 == note2);
		}
	}
}

