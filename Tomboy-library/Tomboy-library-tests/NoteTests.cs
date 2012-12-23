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

