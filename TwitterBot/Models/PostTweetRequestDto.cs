using Newtonsoft.Json;

namespace TwitterBot.Model
{
    public class PostTweetRequestDto
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
        public PostTweetRequestDto(string text) 
        {
            Text = text;
        }
    }
   
}
