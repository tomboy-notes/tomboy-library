using ServiceStack.DataAnnotations;
using Tomboy.Sync.Web.DTO;
using Tomboy.OAuth;
using System;

namespace Tomboy.Db {

	public class DBNote : DTONote 
	{
		[PrimaryKey]
		public virtual string CompoundPrimaryKey {
			get {
				return Guid;
			}
		}

		public new string Guid { get; set; }

		// to associate a note to a username
		public string Username { get; set; }

		public bool IsEncypted { get; set; }
		public string EncryptedKey { get; set; }
	}


	public class DBAccessToken : OAuthToken
	{
		[PrimaryKey]
		public new string Token { get; set; }

		// a short name for the device, i.e. "my laptop", or "my phone"
		// will be set by the user 
		public string DeviceName { get; set; }

		// TODO these shouldn't be in the library, its Rainy specific
		public string UserName { get; set; }
		public string[] Roles { get; set; }
		// the TokenKey encrypts the master_key; the so encrypted master_key
		// is sent back as access token to the user
		public string TokenKey { get; set; }

		public string Realm { get; set; }

		public string ConsumerKey { get; set; }

		public DateTime ExpiryDate { get; set; }

	}
	public static class DbClassConverter
	{
		public static DBNote ToDBNote (this DTONote dto, string username)
		{
			// ServiceStack's .PopulateWith is for some reasons
			// ORDERS of magnitudes slower than manually copying
			// TODO evaluate PopulateWith performance / bottleneck
			// or other mappers like ValueInjecter

			var db = new DBNote ();

			db.Guid = dto.Guid;
			db.Title = dto.Title;
			db.Text = dto.Text;
			db.Tags = dto.Tags;

			// dates
			db.ChangeDate = dto.ChangeDate;
			db.MetadataChangeDate = dto.MetadataChangeDate;
			db.CreateDate = dto.CreateDate;

			db.OpenOnStartup = dto.OpenOnStartup;
			db.Pinned = dto.Pinned;

			db.Username = username;

			return db;
		}

		public static DTONote ToDTONote (this DBNote db)
		{
			var dto = new DTONote ();

			dto.Guid = db.Guid;
			dto.Title = db.Title;
			dto.Text = db.Text;
			dto.Tags = db.Tags;

			// dates
			dto.ChangeDate = db.ChangeDate;
			dto.MetadataChangeDate = db.MetadataChangeDate;
			dto.CreateDate = db.CreateDate;

			dto.OpenOnStartup = db.OpenOnStartup;
			dto.Pinned = db.Pinned;

			return dto;
		}
	}
}