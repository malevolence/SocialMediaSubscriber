using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialMediaSubscriber.Data;
using SocialMediaSubscriber.Models;
using Twitterizer;
using System.Web;

namespace SocialMediaSubscriber
{
	class Program
	{
		private static DataContext db = new DataContext();
		private static int twitterCount = 0;
		private static int facebookCount = 0;
		private static OAuthTokens tokens = new OAuthTokens();
		private static List<string> currentTwitterFriends = new List<string>();

		static void Main(string[] args)
		{
			tokens.ConsumerKey = "9MUpnlPtNbKYNmJt5jsLaPIQM";
			tokens.ConsumerSecret = "OzBCWTLC4QVn5081LhcRPV6DDbjzUh53nvlUhNmbOy5KoK8aFA";
			tokens.AccessToken = "18138503-SSeuXTfCxyYeBzSu1afhUAsKTX59iJ4UK4zObfmEt";
			tokens.AccessTokenSecret = "9zNHI9DYGVYl7n4dZs8VlsCT6T8W5WHRd4uY7wQAm1lp5";

			// download all of the screen names for the people we are already following
			LoadExistingTwitterFriends();

			var profiles = db.GetHashBangProfiles();

			Console.WriteLine("Found {0} Featured Profiles. Commencing processing...", profiles.Count);
			Console.WriteLine("===================================================");

			foreach (var profile in profiles)
			{
				FollowOnTwitter(profile);
				Console.WriteLine("----------------------------------");
			}

			Console.WriteLine("===================================================");
			Console.WriteLine("Processing Complete!");
			Console.WriteLine("Successfully followed {0} featured profiles on Twitter", twitterCount);
			Console.WriteLine("Successfully followed {0} featured profiles on Facebook", facebookCount);
		}

		private static void FollowOnFacebook(Profile profile)
		{
			if (!string.IsNullOrWhiteSpace(profile.Facebook))
			{
				string screenName = "";

				try
				{
					bool result = false;

					// make the call
					Console.WriteLine("Attempting to follow this {0} on Facebook", profile.Facebook);

					var url = new Uri(profile.Facebook);
					if (url.Segments != null && url.Segments.Length > 0)
						screenName = url.Segments[url.Segments.Length - 1].Trim();

					if (!string.IsNullOrEmpty(screenName))
					{
						// lookup by screenname
						// if successful, set result = true
					}
					else
					{
						var queryDict = HttpUtility.ParseQueryString(url.Query);
						string id = queryDict["id"];
						if (!string.IsNullOrEmpty(id))
						{
							// lookup by id
							// if successful, set result = true
						}
					}

					if (result)
						facebookCount++;
				}
				catch (Exception exc)
				{
					Console.WriteLine("***** ERROR *****");
					Console.WriteLine(exc.Message);
				}
			}
		}

		private static void FollowOnTwitter(Profile profile)
		{
			if (!string.IsNullOrWhiteSpace(profile.Twitter))
			{
				try
				{
					string screenName = "";

					if (!profile.Twitter.StartsWith("http"))
						profile.Twitter = "http://" + profile.Twitter;

					profile.Twitter = profile.Twitter.Replace("#!/", "");

					Console.WriteLine("Attempting to follow {0} on Twitter", profile.Twitter);

					var url = new Uri(profile.Twitter);
					if (url.Segments != null && url.Segments.Length > 0)
						screenName = url.Segments[url.Segments.Length - 1].Trim();

					if (!string.IsNullOrEmpty(screenName))
					{
						Console.WriteLine("Found screen name {0} generated from {1}", screenName, profile.Twitter);

						// ensure we aren't already following them
						if (currentTwitterFriends.IndexOf(screenName) == -1)
						{
							var response = TwitterFriendship.Create(tokens, screenName);
							if (response.Result == RequestResult.Success)
							{
								// if successful, increment count
								Console.WriteLine("Successfully followed {0} - {1} [{2}]", screenName, profile.FullName, profile.Id);
								twitterCount++;
							}
							else
							{
								Console.WriteLine(response.ErrorMessage);
							}
						}
						else
						{
							Console.Write("Already following {0}. Skipping.", screenName);
						}
					}
				}
				catch (Exception exc)
				{
					Console.WriteLine("***** ERROR *****");
					Console.WriteLine(exc.Message);
				}
			}
		}

		private static void LoadExistingTwitterFriends()
		{
			long cursor = -1;
			while (cursor != 0)
			{
				var response = TwitterFriendship.FriendsIds(tokens, new UsersIdsOptions { Cursor = cursor });
				if (response.Result == RequestResult.Success)
				{
					cursor = response.ResponseObject.NextCursor;
					// copy the ids
				}
				else
				{
					cursor = 0;
				}
			}
		}
	}
}
