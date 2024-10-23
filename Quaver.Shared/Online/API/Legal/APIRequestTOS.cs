using RestSharp;

namespace Quaver.Shared.Online.API.Legal
{
    public class APIRequestTOS : APIRequest<string>
    {
        private const string BaseUrl = "https://wiki.quavergame.com";

        public override string ExecuteRequest()
        {
            var request = new RestRequest($"{BaseUrl}/md/Legal/Terms/en.md", Method.GET);
            var client = new RestClient(BaseUrl);

            var response = client.Execute(request);

            return string.IsNullOrEmpty(response.Content) ? null : response.Content;
        }
    }
}