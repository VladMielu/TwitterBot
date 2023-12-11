using Microsoft.AspNetCore.Mvc;
using TwitterBot.Model;
using Tweetinvi;
using Tweetinvi.Models;
using System.Text;
using TwitterBot.Models;
using Newtonsoft.Json.Linq;
using Tweetinvi.Core.Extensions;
using System.Text.RegularExpressions;


namespace TwitterBot.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class Tweets : ControllerBase
    {
        //API_KEY
        string api_key = "*";
        //API_KEY_SECRET
        string api_key_secret = "*";
        //ACCES_TOKEN
        string acces_token = "*";
        //ACCES_TOKEN_SECRET
        string acces_token_secret = "*";


        [HttpPost(Name = "Post Tweet")]
        public async Task<IActionResult> PostTodaysGamesTweet(string League)
        {
            List<string> list = new List<string>();

            try
            {
                using (HttpClient Httpclient = new HttpClient())
                {
                    string baseUrl = "https://www.thesportsdb.com/api/v1/json/3";
                    string endpoint = $"{baseUrl}/search_all_teams.php?l={League}";

                    HttpResponseMessage response = await Httpclient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        JObject jsonResponse = JObject.Parse(responseContent);

                        if (jsonResponse.ContainsKey("teams"))
                        {
                            JArray teamsArray = (JArray)jsonResponse["teams"];
                            foreach (JObject team in teamsArray)
                            {
                                string teamName = team.Value<string>("strTeam");
                                list.Add(teamName);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No teams found in the response.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            if (list.IsNullOrEmpty())
            {
                throw new Exception("Not a valid League");
            }

            IEnumerable<string> teams = list;
            teams = teams.Shuffle();

            string text = $"Today's games from {League}  are as follows:\n" +
                          $"\n{teams.ElementAt(0)} - {teams.ElementAt(1)} 20:00\n" +
                          $"{teams.ElementAt(2)} - {teams.ElementAt(3)} 20:45\n" +
                          $"{teams.ElementAt(4)} - {teams.ElementAt(5)} 21:30\n" +
                          "\n Tonight we will have a feast for the eyes.";
            PostTweetRequestDto newTweet = new PostTweetRequestDto(text);
            var client = new TwitterClient(api_key, api_key_secret, acces_token, acces_token_secret);

            var result = await client.Execute.AdvanceRequestAsync(PostTwitterRequest(newTweet, client));

            SaveTweetInfoToFile(ExtractTweetIdFromResponse(result.Content), text);

            return Ok(result.Content);
        }

        [HttpPost(Name = "Facts about a player")]
        public async Task<IActionResult> PostPlayerInfoTweet(string Player)
        {
            List<string> list = new List<string>();
            try
            {
                using (HttpClient Httpclient = new HttpClient())
                {
                    string baseUrl = "https://www.thesportsdb.com/api/v1/json/3";
                    string endpoint = $"{baseUrl}/searchplayers.php?p={Player}";

                    HttpResponseMessage response = await Httpclient.GetAsync(endpoint);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        JObject jsonResponse = JObject.Parse(responseContent);

                        JArray teamsArray = (JArray)jsonResponse["player"];
                        foreach (JObject player in teamsArray)
                        {
                            list.Add(player.Value<string>("strPlayer")); //Player name
                            list.Add(CalculateAge(DateTime.Parse(player.Value<string>("dateBorn"))).ToString()); //Player Age
                            list.Add(player.Value<string>("strHeight")); //Player height
                            list.Add(player.Value<string>("strNationality")); //Player nationality
                            if (player.Value<string>("strTeam") == "_Retired Soccer")
                            {
                                list.Add("Retired");
                                list.Add("Retired");
                            }
                            else
                            {
                                list.Add(player.Value<string>("strTeam")); //Player team
                                list.Add(player.Value<string>("strNumber")); //Player number

                            }
                            //list.Add(player.Value<string>("strDescriptionEN")); //Player description
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.ReasonPhrase);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            if (list.IsNullOrEmpty())
            {
                throw new Exception("Not a valid player name");
            }
            string text = $"Player: {list.ElementAt(0)}\n" +
                          $"Age: {list.ElementAt(1)}\n" +
                          $"Height: {list.ElementAt(2)}\n" +
                          $"Country: {list.ElementAt(3)}\n" +
                          $"Team Name: {list.ElementAt(4)}\n" +
                          $"Number: {list.ElementAt(5)}";

            PostTweetRequestDto newTweet = new PostTweetRequestDto(text);
            var client = new TwitterClient(api_key, api_key_secret, acces_token, acces_token_secret);

            var result = await client.Execute.AdvanceRequestAsync(PostTwitterRequest(newTweet, client));

            SaveTweetInfoToFile(ExtractTweetIdFromResponse(result.Content), text);

            return Ok(result.Content);
        }
        [HttpGet(Name = "Get all tweets")]
        public string GetAllTweets()
        {
            string[] lines = System.IO.File.ReadAllLines("tweetInfo.txt");
            return string.Join("\n", lines);

        }
        [HttpDelete(Name = "Delete Tweet")]
        public async Task<IActionResult> DeleteTweet(string tweetId)
        {
            var client = new TwitterClient(api_key, api_key_secret, acces_token, acces_token_secret);

            var result = await client.Execute.AdvanceRequestAsync(DeleteTwitterRequest(tweetId, client));
            DeleteLinesWithValue("tweetInfo.txt", tweetId);


            return Ok(result.Content);
        }
        [HttpDelete(Name = "Delete Last Tweet")]
        public async Task<IActionResult> DeleteLastTweet()
        {
            var client = new TwitterClient(api_key, api_key_secret, acces_token, acces_token_secret);
            string tweetId = ExtractIdFromLine(GetLastLineFromFile("tweetInfo.txt"));

            var result = await client.Execute.AdvanceRequestAsync(DeleteTwitterRequest(tweetId, client));
            DeleteLinesWithValue("tweetInfo.txt", tweetId);


            return Ok(result.Content);
        }

        [HttpGet(Name = "My Info")]
        public async Task<IActionResult> MyInfo()
        {
            var client = new TwitterClient(api_key, api_key_secret, acces_token, acces_token_secret);

            var result = await client.Execute.AdvanceRequestAsync(MyInfoRequest(client));


            return Ok(result.Content);
        }

        private static Action<ITwitterRequest> PostTwitterRequest(PostTweetRequestDto newTweet, TwitterClient client)
        {
            return (ITwitterRequest request) =>
            {
                var jsonBody = client.Json.Serialize(newTweet);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                request.Query.Url = "https://api.twitter.com/2/tweets";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = content;
            };
        }
        private static Action<ITwitterRequest> MyInfoRequest(TwitterClient client)
        {
            return (ITwitterRequest request) =>
            {
                request.Query.Url = $"https://api.twitter.com/2/users/me";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.GET;
            };
        }

        private static Action<ITwitterRequest> DeleteTwitterRequest(string tweetId, TwitterClient client)
        {
            return (ITwitterRequest request) =>
            {
                request.Query.Url = $"https://api.twitter.com/2/tweets/{tweetId}";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.DELETE;
            };
        }
        static int CalculateAge(DateTime birthDate)
        {
            DateTime currentDate = DateTime.Now;
            int age = currentDate.Year - birthDate.Year;
            if (currentDate.Month < birthDate.Month || (currentDate.Month == birthDate.Month && currentDate.Day < birthDate.Day))
            {
                age--;
            }
            return age;
        }
        private string ExtractTweetIdFromResponse(string responseContent)
        {
            string tweetId = string.Empty;
            JObject jsonResponse = JObject.Parse(responseContent);
            return jsonResponse["data"]["id"].ToString();

        }
        private void SaveTweetInfoToFile(string tweetId, string tweetText)
        {
            using (StreamWriter writer = new StreamWriter("tweetInfo.txt", true))
            {
                writer.WriteLine($"id: {tweetId} , text: {tweetText.Replace("\n", "").Replace("\r", "")}");
            }
        }
        private string GetLastLineFromFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line = null;
                string lastLine = null;

                while ((line = reader.ReadLine()) != null)
                {
                    lastLine = line;
                }

                return lastLine;
            }
        }
        private void DeleteLinesWithValue(string filePath, string valueToDelete)
        {
            if (System.IO.File.Exists(filePath))
            {
                string[] lines = System.IO.File.ReadAllLines(filePath);

                string[] filteredLines = lines
                    .Where(line => !line.Contains(valueToDelete))
                    .ToArray();

                System.IO.File.WriteAllLines(filePath, filteredLines);
            }
            else
            {
                Console.WriteLine("The file doesn't exist.");
            }
        }
        static string ExtractIdFromLine(string line)
        {
            string pattern = @"id: (\d+)";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
