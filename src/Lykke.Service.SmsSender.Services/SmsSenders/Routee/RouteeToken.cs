using Newtonsoft.Json;

namespace Lykke.Service.SmsSender.Services.SmsSenders.Routee
{
    public class RouteeToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("scope")]
        public string Scope { get; set; }
        [JsonProperty("permissions")]
        public string[] Permissions { get; set; }
    }
}
