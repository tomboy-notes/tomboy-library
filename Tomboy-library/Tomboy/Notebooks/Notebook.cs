using System;
using Mono.Unix;
using Tomboy;

namespace Tomboy.Notebooks
{
	/// <summary>
	/// An object that represents a notebook in Tomboy
	/// </summary>
	public class Notebook
	{
		public static string NotebookTagPrefix = "notebook:";
		
		#region Fields
		private string name;
		private string normalizedName;
		private string templateNoteTitle;
		#endregion // Fields
		
		#region Constructors
		/// <summary>
		/// Construct a new Notebook with a given name
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/>.  This is the name that will be used
		/// to identify the notebook.
		/// </param>
		public Notebook (string name)
		{
			Name = name;
		}
		
		/// <summary>
		/// Default constructor not used
		/// </summary>
		protected Notebook ()
		{
		}
		
		#endregion // Constructors
		
		#region Properties
		public virtual string Name
		{
			get {
				return name;
			}
			set {
				if (value != null) {
					string trimmedName = (value as string).Trim ();
					if (trimmedName != String.Empty) {
						name = trimmedName;
						normalizedName = trimmedName.ToLower ();

						// The templateNoteTite should show the name of the
						// notebook.  For example, if the name of the notebooks
						// "Meetings", the templateNoteTitle should be "Meetings
						// Notebook Template".  Translators should place the
						// name of the notebook accordingly using "{0}".
						// TODO: Figure out how to make this note for
						// translators appear properly.
						string format = Catalog.GetString ("{0} Notebook Template");
						templateNoteTitle = string.Format (format, Name);
					}
				}
			}
		}
		
		public virtual string NormalizedName
		{
			get {
				return normalizedName;
			}
		}

		#endregion // Properties
		
		#region Public Methods
		#endregion // Public Methods
		
		#region Private Methods
		#endregion // Private Methods
	}
}