using Quaver.Server.Client;
using RestSharp;
using Wobble.Logging;

namespace Quaver.Shared.Online.API
{
    /// <summary>
    ///     Used for API Requests that don't require authentication. Otherwise they should be in
    ///     Quaver.Server.Client -<see cref="OnlineClient"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    // ReSharper disable once InconsistentNaming
    public abstract class APIRequest<T>
    {
        public string APIEndpoint { get; } = $"{OnlineClient.API_ENDPOINT}/v1/";

        /// <summary>
        ///     Executes a REST request and writes detailed failure information to the network log.
        /// </summary>
        protected IRestResponse ExecuteApiRequest(RestClient client, IRestRequest request) =>
            ApiRequestExecutor.Execute(client, request, LogNetworkFailure);

        private static void LogNetworkFailure(string message) => Logger.Error(message, LogType.Network);

        /// <summary>
        ///     Performs an API Request and returns <see cref="T"/>
        /// </summary>
        /// <returns></returns>
        public abstract T ExecuteRequest();
    }
}
