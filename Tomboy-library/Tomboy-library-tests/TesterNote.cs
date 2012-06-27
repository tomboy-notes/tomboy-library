//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
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
using System.Collections.Generic;
using System.Text;
using Tomboy.Tags;

namespace Tomboy
{
	/// <summary>
	/// Tester note.
	/// </summary>
	public static class TesterNote
	{
		private static Note note = new Note ("tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51d");
		
		public static Note GetTesterNote ()
		{
			if (note == null)
				note = new Note ("tomboy://90d8eb70-989d-4b26-97bc-ba4b9442e51d");
			SetUpNote ();
			return note;
		}

		public static void TearDownNote ()
		{
			note = null;
		}
		
		private static void SetUpNote ()
		{
			string changeDateString = "2012-04-05T22:51:54.2191587+02:00";
			DateTime changeDate = DateTime.Parse(changeDateString, System.Globalization.CultureInfo.InvariantCulture);
			note.ChangeDate = changeDate;
			
			string metaDateString = "2012-04-05T22:51:54.2191587+02:00";
			DateTime metaDate = DateTime.Parse(metaDateString, System.Globalization.CultureInfo.InvariantCulture);
			note.MetadataChangeDate = metaDate;

			DateTime createDate = DateTime.Parse("2012-04-05T21:51:54.2191587+02:00", System.Globalization.CultureInfo.InvariantCulture);
			note.CreateDate = createDate;
			
			StringBuilder sb = new StringBuilder ();
			sb.AppendLine ("<bold>Welcome to Tomboy!</bold>");
			sb.AppendLine ("Use this \"Start Here\" note to begin organizing your ideas and thoughts.");
			note.Text = sb.ToString ();
			note.Title = "Unit Test Note";

			Tag tag = new Tag ("Example Notebook");
			if (!note.Tags.ContainsKey (tag.NormalizedName)) 
				note.Tags.Add (tag.NormalizedName, tag);
		}
	}
}

