using SocialMediaSubscriber.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialMediaSubscriber.Data
{
	public class DataContext : DbContext
	{
		public DataContext() : base("ProHUB")
		{
			this.Configuration.LazyLoadingEnabled = false;
			this.Configuration.AutoDetectChangesEnabled = false;
			this.Configuration.ValidateOnSaveEnabled = false;
			this.Configuration.ProxyCreationEnabled = false;
		}

		public List<Profile> GetProfiles()
		{
			var profiles = new List<Profile>();

			try
			{
				var sql = new StringBuilder();
				sql.Append("SELECT e.ECardID AS Id, e.FullName, ISNULL(f.Url, '') as Facebook, ISNULL(t.Url, '') AS Twitter ");
				sql.Append("FROM ECard_Master e ");
				sql.Append("LEFT OUTER JOIN Links f ON f.ListingID = e.ECardID AND f.LinkTypeID = 1 ");
				sql.Append("LEFT OUTER JOIN Links t ON t.ListingID = e.ECardID AND t.LinkTypeID = 3 ");
				sql.Append("WHERE e.IsFeatured = 1 ");
				sql.Append("AND e.IsApproved = 1 ");
				sql.Append("AND e.IsOutdated = 0 ");
				sql.Append("ORDER BY e.FullName");
				List<SqlParameter> parameters = new List<SqlParameter>();
				profiles = this.Database.SqlQuery<Profile>(sql.ToString(), parameters.ToArray()).ToList();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
			}

			return profiles;
		}

		public List<Profile> GetHashBangProfiles()
		{
			var profiles = new List<Profile>();

			try
			{
				var sql = new StringBuilder();
				sql.Append("SELECT e.ECardID AS Id, e.FullName, ISNULL(t.Url, '') AS Twitter ");
				sql.Append("FROM ECard_Master e ");
				sql.Append("INNER JOIN Links t ON t.ListingID = e.ECardID AND t.LinkTypeID = 3 ");
				sql.Append("WHERE e.IsFeatured = 1 ");
				sql.Append("AND e.IsApproved = 1 ");
				sql.Append("AND e.IsOutdated = 0 ");
				sql.Append("AND t.Url NOT LIKE '%#!%'");
					List<SqlParameter> parameters = new List<SqlParameter>();
				profiles = this.Database.SqlQuery<Profile>(sql.ToString(), parameters.ToArray()).ToList();
			}
			catch (Exception exc)
			{
				Console.WriteLine(exc.Message);
			}

			return profiles;
		}

		public List<Profile> GetTestProfiles()
		{
			var profiles = new List<Profile>();

			profiles.Add(new Profile { Id = 142484, FullName = "Bill Williams", Facebook = "https://www.facebook.com/profile.php?id=100005187033016", Twitter = "http://twitter.com/malevolenc" });
			profiles.Add(new Profile { Id = 172459, FullName = "Tom Grealy", Facebook = "https://www.facebook.com/tom.grealy", Twitter = "http://twitter.com/TommyHUBS" });

			return profiles;
		}
	}
}
