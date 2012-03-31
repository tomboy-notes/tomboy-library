using System;
using System.Collections.Generic;

namespace Tomboy
{
	/// <summary>
	/// Handles interaction with backend system is used to store notes.
	/// </summary>
	public interface IStorage
	{
		/// <summary>
		/// Gets notes.
		/// </summary>
		/// <returns>
		/// A Generic list of Notes
		/// </returns>
		List<Note> GetNotes ();
		
		/// <summary>
		/// Sets the path to where Notes are located
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		void SetPath (string path);
	}
}

