using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using RestSharp;
using Wobble.Logging;

namespace Quaver.Shared.Online.API.Multiplayer;

public class APIRequestMultiplayerGameInformation : APIRequest<MultiplayerGameInformationResponse>
{
    /// <summary>
    ///     The id of the map to lookup
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    public APIRequestMultiplayerGameInformation(int id) => Id = id;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override MultiplayerGameInformationResponse ExecuteRequest()
    {
        var request = new RestRequest($"{APIEndpoint}multiplayer/games/{Id}", Method.GET);
        var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

        var response = client.Execute(request);

        var json = JObject.Parse(response.Content);

        var responseParsed = JsonConvert.DeserializeObject<MultiplayerGameInformationResponse>(json.ToString());

        return responseParsed;
    }
}