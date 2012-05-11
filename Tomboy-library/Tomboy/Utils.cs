//  
//  Author:
//       jjennings <jaredljennings@gmail.com>
//  
//  Copyright (c) 2012 jjennings
// 
// This library is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as
// published by the Free Software Foundation; either version 2.1 of the
// License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

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
		
		/// <summary>
		/// Gets the note file name from URI.
		/// </summary>
		/// <returns>
		/// The note file name from URI.
		/// </returns>
		/// <param name='note'>
		/// Note.
		/// </param>
		public static string GetNoteFileNameFromURI (Note note)
		{
			string name = "";
			int begin = note.Uri.LastIndexOf ("/");
			begin++;
			name = note.Uri.Substring (begin,(note.Uri.Length - begin));
			name += ".note";
			return name;
		}
	
	}
}

