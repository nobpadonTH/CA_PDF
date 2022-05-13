using CA_SVC.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Serializers.NewtonsoftJson;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace CA_SVC.Clients
{
    /// <summary>
    /// SiamSmile's Shortlink API Client
    /// </summary>
    /// <remarks>
    /// Use for shorten a long link.
    /// Normally URL for UAT is http://uat.siamsmile.co.th:9220
    /// </remarks>
    public class ShortLinkClient
    {
        private readonly HttpRequest _request;
        private readonly RestClient _client;
        private readonly ServiceURL _configuration;
        private const string _clientTitle = "Short Link Client";

        public ShortLinkClient(IHttpContextAccessor accessor, IOptions<ServiceURL> configuration)
        {
            _configuration = configuration.Value;
            _request = accessor.HttpContext.Request;
            _client = new RestClient(_configuration.ShortLinkApi);
            _client.UseNewtonsoftJson();

            Log.Verbose("{ApiClient} Create client", _clientTitle);
        }

        /// <summary>
        /// Generete shorten link by URL
        /// </summary>
        /// <remarks>
        /// Endpoint : /GenerateLinkShorten
        /// </remarks>
        /// <param name="url">URL for shorten</param>
        /// <param name="expiredDate">(optional) Expirely date of shorten link</param>
        /// <returns>Shorten link information in <see cref="ShortLinkResponse"/></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task<ShortLinkResponse> GenerateLinkShortenAsync(string url, DateTime? expiredDate = null)
        {
            Log.Verbose("{ApiClient} Start generate link", _clientTitle);
            return GenerateLinkShortenAsync(url, System.Threading.CancellationToken.None, expiredDate);
        }

        private async Task<ShortLinkResponse> GenerateLinkShortenAsync(string url, CancellationToken none, DateTime? expiredDate = null)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Error("{ApiClient} Url is null or white space", _clientTitle);
                throw new ArgumentNullException("URL must set and can not be null.");
            }

            if (this.ValidateUrl(url) == false)
            {
                Log.Error("{ApiClient} Url is invalid", _clientTitle);
                throw new InvalidOperationException("URL is invalid.");
            }

            Log.Verbose("{ApiClient} Url validation pass", _clientTitle);

            // Encoding URL query string
            url = HttpUtility.UrlEncode(url);
            var endpoint = $"GenerateLinkShorten?Url={url}";

            if (!(expiredDate is null))
                endpoint += "&expiredDate=" + HttpUtility.UrlEncode(expiredDate.ToString());

            Log.Debug("{ApiClient} [URL={URL};Expiration Date={ExpiredDate}]", _clientTitle, url, expiredDate);

            // Add client's authorization
            if (_request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues token))
            {
                _client.Authenticator = new JwtAuthenticator(token.ToString()[7..]);
                Log.Debug("{ApiClient} [JWT={Token}]", _clientTitle, token.ToString());
            }

            // Start request and get response
            var request = new RestRequest(endpoint, DataFormat.Json);
            var response = await Task.FromResult(_client.Post<ShortLinkResponse>(request));
            Log.Verbose("{ApiClient} Requested to API", _clientTitle);

            // Check data is valid
            if (response.Data is null)
            {
                Log.Error(response.ErrorException, "{ApiClient} Got an error [Message={ErrorMessage}]", _clientTitle, response.ErrorMessage);
                throw new InvalidOperationException(response.ErrorMessage, response.ErrorException);
            }

            Log.Debug("{ApiClient} [Response={Response}]", _clientTitle, JsonConvert.SerializeObject(response.Data));

            // Return data
            return response.Data;
        }

        public bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uri);
        }

        public class ShortLinkResponse
        {
            [JsonProperty("shortURL")]
            public string ShortURL { get; set; }

            [JsonProperty("expiredDate")]
            public DateTime ExpiredDate { get; set; }
        }
    }
}