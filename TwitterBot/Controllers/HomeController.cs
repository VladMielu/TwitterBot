using Microsoft.AspNetCore.Mvc;
using Tweetinvi;
using Tweetinvi.Models;
using TwitterBot.Models;
using System.Text;


namespace TwitterBot.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HomeController : Controller
    {
        string api_key = "fPV194ZCE0MnuWsUPXSA3RL4r";
        string api_key_secret = "s6Q9WCHxtay5m53q5qqWbjMR1sPND6NgcdM3TZ6H4M9tAdw45p";
        string acces_token = "1711739804583366656-Fdbevknmr2VKoZWYgmYoU0Gm5nOwTG";
        string acces_token_secret = "87UW7l8srT7xPEPHCGKXJY1n2RuZzVeyYtM0t50sufIdk";

        [HttpPost(Name = "Post Image Tweet")]
        public async Task<IActionResult> PostImageTweet(IFormFile imageInput)
        {

            var stream = new MemoryStream();

            imageInput.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            byte[] image = ConvertImageToByteArray(Image.Load(stream));

            PostImageRequestdto tweetImage = new PostImageRequestdto(image);

            var client = new TwitterClient(api_key, api_key_secret, acces_token, acces_token_secret);

            var result = await client.Execute.AdvanceRequestAsync(ImageTwitterRequest(tweetImage, client));


            return Ok(result);
        }
        private static Action<ITwitterRequest> ImageTwitterRequest(PostImageRequestdto tweetImage, TwitterClient client)
        {
            return (ITwitterRequest request) =>
            {
                var jsonBody = client.Json.Serialize(tweetImage);
                var content = new StringContent(jsonBody, Encoding.UTF8, "multipart/form-data");

                request.Query.Url = "https://upload.twitter.com/1.1/media";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = content;
            };
        }
        internal byte[] ConvertImageToByteArray(Image imageInput)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                imageInput.SaveAsJpeg(stream);

                return stream.ToArray();
            }
        }
    }
}
