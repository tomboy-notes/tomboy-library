using System;
using System.Text;

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
			return note;
		}
		
		private static void Note ()
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
		}
	}
}

