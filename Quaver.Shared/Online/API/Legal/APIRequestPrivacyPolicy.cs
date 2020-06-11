using RestSharp;

namespace Quaver.Shared.Online.API.Legal
{
    public class APIRequestPrivacyPolicy : APIRequest<string>
    {
        private const string BaseUrl = "https://raw.githubusercontent.com";
        
        public override string ExecuteRequest()
        {
            var request = new RestRequest($"{BaseUrl}/Quaver/Quaver.Wiki/master/Legal/Privacy/en.md", Method.GET);
            var client = new RestClient(BaseUrl);

            var response = client.Execute(request);

            return string.IsNullOrEmpty(response.Content) ? null : response.Content;
        }
    }
}