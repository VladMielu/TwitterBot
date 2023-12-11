using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Memory;
using Tweetinvi;
using Tweetinvi.Models;
using TwitterBot.Models;



namespace TwitterBot.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class Paid : Controller
    {
        //API_KEY
        string api_key = "GzTOVLuY4VORtmyFNeFH7LFMh";
        //API_KEY_SECRET
        string api_key_secret = "zKvzV7YFHVmpWyIrTZC1fmo6pJwscDgU9B7tvwSH6HDtvpgc2X";
        //ACCES_TOKEN
        string acces_token = "1711739804583366656-l5twi1vbD0K4JqRbA1PycG8nj6TRjH";
        //ACCES_TOKEN_SECRET
        string acces_token_secret = "v3XV8UqMxNr4T9hlWxJOYuJZ2rCXFxdx66yQfGm8Na41f";
        //BEARER_TOKEN (OPTIONAL)
        string bearer_token = "AAAAAAAAAAAAAAAAAAAAALDCqQEAAAAAV%2FkfCoeIiK0qpR4FIlt4BUPqdTI%3DFUNCh19oomQm9ibTLmB7NRBrhFow9c6Em9ULqxp8MTgmTUigyu";

        [HttpPost(Name = "Upload Image")]
        public async Task<IActionResult> PostImageTweet(IFormFile imageInput)
        {
            string img = IFormFileToBase64String(imageInput);
            var client = new TwitterClient(api_key, api_key_secret, bearer_token);

            var result = await client.Execute.AdvanceRequestAsync(ImageTwitterRequest(img, client));


            return Ok(result.Content);
        }

        private static Action<ITwitterRequest> ImageTwitterRequest(string tweetImage, TwitterClient client)
        {
            return (ITwitterRequest request) =>
            {
                request.Query.Url = $"https://upload.twitter.com/1.1/media/upload.json?media_data={tweetImage}";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
            };
        }
        public static string IFormFileToBase64String(IFormFile file)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                byte[] bytes = memoryStream.ToArray();

                string base64String = Convert.ToBase64String(bytes);
                return base64String;
            }
        }
    }
}
