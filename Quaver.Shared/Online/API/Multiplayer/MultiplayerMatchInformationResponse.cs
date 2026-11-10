using Newtonsoft.Json;

namespace Quaver.Shared.Online.API.Multiplayer;

public class MultiplayerMatchInformationResponse
{
    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("game")]
    public MultiplayerGameInformationResponseGame Game { get; set; }

    [JsonProperty("match")]
    public MultiplayerGameInformationResponseMatch Match { get; set; }
}
