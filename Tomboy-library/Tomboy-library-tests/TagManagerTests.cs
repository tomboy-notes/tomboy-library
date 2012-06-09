// 
//  TagManagerTests.cs
//  
//  Author:
//       Jared L Jennings <jaredljennings@gmail.com>
// 
//  Copyright (c) 2012 Jared L Jennings
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
using System.Collections.Generic;
using NUnit.Framework;
using Tomboy;
using Tomboy.Tags;

namespace Tomboylibrarytests
{
	[TestFixture()]
	public class TagManagerTests
	{
		private TagManager tagMgr;
		const string TAG_NAME_GOOGLE = "Google";

		[SetUp]
		public void Init ()
		{
			tagMgr = new TagManager ();
			tagMgr.GetOrCreateTag (TAG_NAME_GOOGLE);
		}

		[Test()]
		public void Contains_tag_Google ()
		{
			Assert.IsNotNull (tagMgr.GetTag (TAG_NAME_GOOGLE));
		}

		[Test()]
		public void NOT_Contains_tag ()
		{
			Assert.IsNull (tagMgr.GetTag ("A crazy tag Name"));
		}

		[Test()]
		public void Note_Mapping_Contains_Note ()
		{
			Note note = TesterNote.GetTesterNote ();
			Tag tag = new Tag ("School");
			note.Tags.Add ("School", tag);
			tagMgr.AddTagMap (note);
			Assert.IsTrue (tagMgr.GetTag ("School").Name.Equals ( "School"));
			Assert.IsTrue (tagMgr.GetNotesByTag ("School").Contains (note));

		}

		[Test ()]
		public void Get_Tags ()
		{
			Note note = TesterNote.GetTesterNote ();
			Tag tag = new Tag ("School");
			note.Tags.Add ("School", tag);
			tagMgr.AddTagMap (note);
			Assert.IsTrue (tagMgr.GetTags ().ContainsKey ("school"));
		}

		[Test ()][ExpectedException ("System.ArgumentException")]
		public void Remove_Tag ()
		{
			Note note = TesterNote.GetTesterNote ();
			Tag tag = new Tag ("School");
			note.Tags.Add ("School", tag);
			tagMgr.AddTagMap (note);
			tagMgr.RemoveTag (tag);
			Assert.IsFalse (tagMgr.GetTags ().ContainsKey ("school"));
			Assert.IsFalse (tagMgr.GetNotesByTag ("School").Contains (note)); // The tag should no longer exist
		}
	}
}