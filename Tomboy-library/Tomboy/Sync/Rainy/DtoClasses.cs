using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Tomboy.Tags;
using System.Linq;
using ServiceStack.Common;

namespace Tomboy.Sync.DTO
{
	[DataContract]
	public class ApiRequest
	{
	}

	[DataContract]
	public class ApiResponse
	{
		[DataMember (Name = "user-ref")]
		public ContentRef UserRef { get; set; }

		[DataMember (Name = "oauth_request_token_url")]
		public string OAuthRequestTokenUrl { get; set; }

		[DataMember (Name ="oauth_authorize_url")]
		public string OAuthAuthorizeUrl { get; set; }

		[DataMember (Name ="oauth_access_token_url")]
		public string OAuthAccessTokenUrl { get; set; }

		[DataMember (Name = "api-version")]
		public string ApiVersion { get; set; }

	}

	[DataContract]
	public class ContentRef
	{
		[DataMember (Name = "api-ref")]
		public string ApiRef { get; set; }

		[DataMember (Name = "href")]
		public string Href { get; set; }
	}

	[DataContract]
	public class UserRequest
	{
	}

	[DataContract]
	public class UserResponse
	{
		[DataMember (Name = "user-name")]
		public string Username { get; set; }

		[DataMember (Name = "first-name")]
		public string Firstname { get; set; }

		[DataMember (Name = "last-name")]
		public string Lastname { get; set; }

		[DataMember (Name = "notes-ref")]
		public ContentRef NotesRef { get; set; }

		[DataMember (Name = "latest-sync-revision")]
		public long LatestSyncRevision { get; set; }

		[DataMember (Name = "current-sync-guid")]
		public string CurrentSyncGuid { get; set; }
	}

	[DataContract]
	public class GetNotesRequest
	{
	}

	[DataContract]
	public class GetNotesResponse
	{
		[DataMember (Name ="latest-sync-revision")]
		public long LatestSyncRevision { get; set; }

		[DataMember (Name = "notes")]
		public IList<DTONote> Notes { get; set; }

	}

	[DataContract]
	public class PutNotesRequest
	{
		[DataMember (Name = "latest-sync-revision")]
		public long? LatestSyncRevision { get; set; }

		[DataMember (Name = "note-changes")]
		public IList<DTONote> Notes { get; set; }

	}

	// PutNotesResponse is the same as GetNotesResponse
	[DataContract]
	public class PutNotesResponse
	{
		[DataMember (Name ="latest-sync-revision")]
		public long LatestSyncRevision { get; set; }
		
		[DataMember (Name = "notes")]
		public IList<DTONote> Notes { get; set; }
	}

	[DataContract]
	public class DTONote
	{
		[DataMember (Name = "title")]
		public string Title { get; set; }

		[DataMember (Name = "note-content")]
		public string Text { get; set; }

		[DataMember (Name = "note-content-version")]
		public double NoteContentVersion {
			get {
				//return double.Parse (Tomboy.Reader.CURRENT_VERSION);
				return 0.3;
			}
		}

		public string Uri { get; set; }

		[DataMember (Name = "guid")]
		public string Guid { get ; set; }

		public DateTime CreateDate { get; set; }

		[DataMember (Name = "create-date")]
		public string CreateDateFormated {
			get {
				return CreateDate.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			}
			set {
				CreateDate = DateTime.Parse (value);
			}
		}

		public DateTime ChangeDate { get; set; }

		[DataMember (Name = "last-change-date")]
		public string LastChangeDateFormated {
			get {
				return ChangeDate.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			}
			set {
				ChangeDate = DateTime.Parse (value);
			}
		}

		public DateTime MetadataChangeDate { get; set; }
		[DataMember (Name = "last-metadata-change-date")]
		public string MetadataChangeDateFormated {
			get {
				return MetadataChangeDate.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			}
			set {
				MetadataChangeDate = DateTime.Parse (value);
			}
		}

		[DataMember (Name = "last-sync-revision")]
		public long LastSyncRevision { get; set; }

		public IDictionary<string, Tag> Tags { get; set; }

		[DataMember (Name = "tags")]
		public string[] TagsAsString {
			get {
				if (Tags == null) return null;
				return Tags.Values.Select (tag => tag.Name).ToArray ();
			}
			set {
				Tags = new Dictionary<string, Tag> ();
				foreach (string tag in value) {
					var t = new Tag (tag);
					Tags.Add (t.Name, t);
				}
			}
		}

		[DataMember (Name = "command")]
		public string Command { get; set; }

	}

	// extension methods for converting between Tomboy.Note and Tomboy.Sync.DTO.DTONote
	// and vice versa
	public static class NoteConverter
	{
		// Tomboy.Note -> DTONote
		public static DTONote ToDTONote (this Tomboy.Note tomboy_note)
		{
			DTONote dto_note = new DTONote ();

			// using ServiceStack simple auto mapper
			dto_note.PopulateWith (tomboy_note);

			return dto_note;
		}

		// DTONote -> Tomboy.Note
		public static Tomboy.Note ToTomboyNote (this DTONote dto_note)
		{
			var tomboy_note = new Tomboy.Note ();

			tomboy_note.PopulateWith (dto_note);

			return tomboy_note;
		}

		// same as above but for IList's
		public static IList<DTONote> ToDTONotes (this IList<Tomboy.Note> tomboy_notes)
		{
			return tomboy_notes.Select (n => n.ToDTONote ()).ToList ();
		}
		public static IList<Tomboy.Note> ToTomboyNotes (this IList<DTONote> dto_notes)
		{
			return dto_notes.Select (n => n.ToTomboyNote ()).ToList ();
		}
	}
}

