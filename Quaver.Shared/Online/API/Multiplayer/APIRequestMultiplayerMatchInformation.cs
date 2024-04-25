using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using RestSharp;
using Wobble.Logging;

namespace Quaver.Shared.Online.API.Multiplayer;

public class APIRequestMultiplayerMatchInformation : APIRequest<MultiplayerMatchInformationResponse>
{
    /// <summary>
    ///     The id of the map to lookup
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    public APIRequestMultiplayerMatchInformation(int id) => Id = id;

    /// <inheritdoc />
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override MultiplayerMatchInformationResponse ExecuteRequest()
    {
        var request = new RestRequest($"{APIEndpoint}multiplayer/match/{Id}", Method.GET);
        var client = new RestClient(OnlineClient.API_ENDPOINT) { UserAgent = "Quaver" };

        var response = client.Execute(request);
        
        var json = JObject.Parse(response.Content);

        var responseParsed = JsonConvert.DeserializeObject<MultiplayerMatchInformationResponse>(json.ToString());

        return responseParsed;
    }
}