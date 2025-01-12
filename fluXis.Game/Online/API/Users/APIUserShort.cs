using Newtonsoft.Json;

namespace fluXis.Game.Online.API.Users;

public class APIUserShort
{
    [JsonProperty("id")]
    public int ID { get; set; } = -1;

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonProperty("social")]
    public APIUserSocials Socials { get; set; } = new();

    [JsonProperty("displayname")]
    public string DisplayName { get; set; } = "";

    [JsonProperty("role")]
    public int Role { get; set; }

    public string GetAvatarUrl(APIEndpointConfig endpoint) => endpoint.APIUrl + "/assets/avatar/" + ID;
    public string GetBannerUrl(APIEndpointConfig endpoint) => endpoint.APIUrl + "/assets/banner/" + ID;

    public string GetName() => string.IsNullOrEmpty(DisplayName) ? Username : DisplayName;

    public static APIUserShort Dummy => new() { ID = -1, Username = "Dummy Player" };

    public class APIUserSocials
    {
        [JsonProperty("twitter")]
        public string Twitter { get; set; } = string.Empty;

        [JsonProperty("twitch")]
        public string Twitch { get; set; } = string.Empty;

        [JsonProperty("youtube")]
        public string YouTube { get; set; } = string.Empty;

        [JsonProperty("discord")]
        public string Discord { get; set; } = string.Empty;
    }
}
