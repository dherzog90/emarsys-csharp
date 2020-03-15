using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Emarsys
{
    public class EmarsysClient
    {
        private const string DefaultMediaType = "application/json";
        private const string DefaultBaseApiUri = "https://api.emarsys.net/api/v2/";
        private const string DefaultAuthHeaderName = "X-WSSE";

        private readonly EmarsysOptions _options;
        private readonly HttpClient _client;

        public EmarsysClient(EmarsysOptions options)
        {
            _options = options;

            if (_client == null)
            {
                _client = new HttpClient { BaseAddress = new Uri(DefaultBaseApiUri) };
            }
        }

        public EmarsysClient(string baseApiUri, EmarsysOptions options) : this(options)
        {
            _client = new HttpClient { BaseAddress = new Uri(baseApiUri) };
        }

        public EmarsysClient(HttpClient client, EmarsysOptions options) : this(options)
        {
            _client = client;
        }

        public enum Method
        {
            DELETE,
            GET,
            POST,
            PUT,
        }

        public async Task<EmarsysResponse> SendRequest(EmarsysClient.Method method, string urlPath, string requestBody = null, CancellationToken cancellationToken = default)
        {
            var response = await MakeRequest(method, urlPath, requestBody, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmarsysResponse>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<EmarsysResponse<TModel>> SendRequest<TModel>(EmarsysClient.Method method, string urlPath, string requestBody = null, CancellationToken cancellationToken = default)
        {
            var response = await MakeRequest(method, urlPath, requestBody, cancellationToken);
            var result = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EmarsysResponse<TModel>>(result, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<HttpResponseMessage> MakeRequest(EmarsysClient.Method method, string urlPath, string requestBody = null, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(method.ToString()),
                RequestUri = new Uri(_client.BaseAddress, urlPath),
                Content = requestBody == null ? null : new StringContent(requestBody, Encoding.UTF8, DefaultMediaType)
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(DefaultMediaType));
            request.Headers.Add(DefaultAuthHeaderName, GetAuthHeader());
            return await _client.SendAsync(request, cancellationToken);
        }

        private string GetAuthHeader()
        {
            var nonce = GetRandomString(32);
            var timestamp = DateTime.UtcNow.ToString("o");
            var passwordDigest = Convert.ToBase64String(Encoding.UTF8.GetBytes(Sha1(nonce + timestamp + _options.Secret)));
            return $"Username=\"{_options.Key}\", PasswordDigest=\"{passwordDigest}\", Nonce=\"{nonce}\", Created=\"{timestamp}\"  Content-type: application/json;charset=\"utf-8\"";
        }

        private static string Sha1(string input)
        {
            var hashInBytes = new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join(string.Empty, Array.ConvertAll(hashInBytes, b => b.ToString("x2")));
        }

        private static string GetRandomString(int length)
        {
            var random = new Random();
            var chars = new[] { "0", "2", "3", "4", "5", "6", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++) sb.Append(chars[random.Next(chars.Length)]);
            return sb.ToString();
        }
    }
}
