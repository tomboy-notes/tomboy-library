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
using System.Runtime.Serialization;
using System.Collections.Generic;
using Tomboy.Tags;
using System.Linq;
using ServiceStack.Common;

namespace Tomboy.Sync.Web.DTO
{
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
		public int LatestSyncRevision { get; set; }

		[DataMember (Name = "note-changes")]
		public IList<DTONote> Notes { get; set; }
	}

	[DataContract]
	public class DTONote
	{
		public DTONote ()
		{
			// set some default values
			this.Tags = new string[] {};
			var epoch_start = DateTime.MinValue.AddYears (1).ToString (Writer.DATE_TIME_FORMAT);
			this.MetadataChangeDate = epoch_start;
			this.ChangeDate = epoch_start;
			this.CreateDate = epoch_start;
		}
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


		// the date fields are strings, in contract to DateTime
		// with the Tomboy.Note
		[DataMember (Name = "create-date")]
		public string CreateDate { get; set; }

		[DataMember (Name = "last-change-date")]
		public string ChangeDate { get; set; }

		[DataMember (Name = "last-metadata-change-date")]
		public string MetadataChangeDate { get; set; }

		[DataMember (Name = "last-sync-revision")]
		public long LastSyncRevision { get; set; }

		[DataMember (Name = "tags")]
		public string[] Tags { get; set; }

		[DataMember (Name = "open-on-startup")]
		public bool OpenOnStartup { get; set; }

		[DataMember (Name = "pinned")]
		public bool Pinned { get; set; }

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

			// copy over tags
			dto_note.Tags = tomboy_note.Tags.Keys.ToArray ();

			// correctly format the DateTime to strings
			dto_note.CreateDate = tomboy_note.CreateDate.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			dto_note.ChangeDate = tomboy_note.ChangeDate.ToString (Tomboy.Writer.DATE_TIME_FORMAT);
			dto_note.MetadataChangeDate = tomboy_note.MetadataChangeDate.ToString (Tomboy.Writer.DATE_TIME_FORMAT);

			return dto_note;
		}

		// DTONote -> Tomboy.Note
		public static Tomboy.Note ToTomboyNote (this DTONote dto_note)
		{
			var tomboy_note = new Tomboy.Note ();

			tomboy_note.PopulateWith (dto_note);

			// Guid's set is internal and cannot be set by PopulateWith since it is in another assembly
			tomboy_note.Guid = dto_note.Guid;

			// copy over tags
			foreach (var string_tag in dto_note.Tags) {
				var tomboy_tag = TagManager.Instance.GetOrCreateTag (string_tag);
				tomboy_note.Tags.Add (string_tag, tomboy_tag);
			}

			// create DateTime from strings
			tomboy_note.ChangeDate = DateTime.Parse (dto_note.ChangeDate);
			tomboy_note.CreateDate = DateTime.Parse (dto_note.CreateDate);
			tomboy_note.MetadataChangeDate = DateTime.Parse (dto_note.MetadataChangeDate);

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

