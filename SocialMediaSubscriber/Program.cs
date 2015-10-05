using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SocialMediaSubscriber.Data;
using SocialMediaSubscriber.Models;
using System.Web;
using LinqToTwitter;

namespace SocialMediaSubscriber
{
	class Program
	{
		private static DataContext db = new DataContext();
		private static int twitterCount = 0;
		private static int facebookCount = 0;
		private static List<string> currentTwitterFriends = new List<string>();

		static void Main(string[] args)
		{
            try
            {
                Task demoTask = DoStuffAsync();
                demoTask.Wait();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.ToString());
            }

            Console.WriteLine("Press any key to close console window...");
            Console.ReadKey(true);


			//var profiles = db.GetHashBangProfiles();

			//Console.WriteLine("Found {0} Featured Profiles. Commencing processing...", profiles.Count);
			//Console.WriteLine("===================================================");

			//foreach (var profile in profiles)
			//{
			//	FollowOnTwitter(profile);
			//	Console.WriteLine("----------------------------------");
			//}

			//Console.WriteLine("===================================================");
			//Console.WriteLine("Processing Complete!");
			//Console.WriteLine("Successfully followed {0} featured profiles on Twitter", twitterCount);
			//Console.WriteLine("Successfully followed {0} featured profiles on Facebook", facebookCount);
		}

        static async Task DoStuffAsync()
        {
            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = "9MUpnlPtNbKYNmJt5jsLaPIQM",
                    ConsumerSecret = "OzBCWTLC4QVn5081LhcRPV6DDbjzUh53nvlUhNmbOy5KoK8aFA",
                    AccessToken = "18138503-SSeuXTfCxyYeBzSu1afhUAsKTX59iJ4UK4zObfmEt",
                    AccessTokenSecret = "9zNHI9DYGVYl7n4dZs8VlsCT6T8W5WHRd4uY7wQAm1lp5",
                }
            };

            await auth.AuthorizeAsync();

            var twitterContext = new TwitterContext(auth);
            await LoadExistingTwitterFriends(twitterContext);
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

		private static async void FollowOnTwitter(TwitterContext twitterContext, Profile profile)
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
							var user = await twitterContext.CreateFriendshipAsync(screenName, false);
							if (user != null && user.Status != null)
							{
                                // if successful, increment count
                                Console.WriteLine("User Name: {0}, Status: {1}", user.Name, user.Status);
								Console.WriteLine("Successfully followed {0} - {1} [{2}]", screenName, profile.FullName, profile.Id);
								twitterCount++;
							}
							else
							{
								Console.WriteLine("NULL response");
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

		static async Task LoadExistingTwitterFriends(TwitterContext twitterContext)
		{
            long cursor = -1;
            Friendship friendship;

            try
            {
                Console.WriteLine("Loading existing twitter friends...");
                while (cursor != 0)
                {
                    Console.WriteLine("Cursor: {0}", cursor);
                    friendship = await twitterContext.Friendship.Where(x => x.Type == FriendshipType.FriendsList && x.Cursor == cursor && x.ScreenName == "ProductionHUB" && x.SkipStatus == true && x.Count == 200).SingleOrDefaultAsync();
                    //friendship = await (from friend in twitterContext.Friendship where friend.Type == FriendshipType.FriendsList && friend.ScreenName == "ProductionHUB" && friend.Cursor == cursor && friend.Count == 200 && friend.SkipStatus == true select friend).SingleOrDefaultAsync();
                    if (friendship != null && friendship.Users != null && friendship.CursorMovement != null)
                    {
                        cursor = friendship.CursorMovement.Next;
                        Console.WriteLine("{0} users returned in this batch", friendship.Users.Count);
                        friendship.Users.ForEach(friend => currentTwitterFriends.Add(friend.ScreenNameResponse));
                    }
                    else
                    {
                        Console.WriteLine("Something was null when retrieving friends");
                    }
                }

                Console.WriteLine("{0} existing friends loaded from twitter.", currentTwitterFriends.Count);
            }
            catch (Exception exc)
            {
                Console.WriteLine("****** ERROR LOADING EXISTING FRIENDS *******");
                Console.WriteLine(exc.Message);
            }
        }
	}
}
