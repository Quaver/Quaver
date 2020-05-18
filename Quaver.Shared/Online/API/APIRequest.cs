using Quaver.Server.Client;

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
        ///     Performs an API Request and returns <see cref="T"/>
        /// </summary>
        /// <returns></returns>
        public abstract T ExecuteRequest();
    }
}