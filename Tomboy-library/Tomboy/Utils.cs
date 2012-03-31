using System;
using System.IO;

namespace Tomboy
{
	/// <summary>
	/// Utils that are used by different classes in Tomboy
	/// </summary>
	public static class Utils
	{
	
		/// <summary>
		/// Returns the URI of a Note
		/// </summary>
		/// <description>note://tomboy/d8903a05-40ba-4a77-a408-bbb50d76b837.note</description>
		/// <returns>
		/// The UR.
		/// </returns>
		/// <param name='filepath'>
		/// Filepath.
		/// </param>
		public static string GetURI (string filepath)
		{
			return "note://tomboy/" + Path.GetFileNameWithoutExtension (filepath);
		}
	
	}
}

